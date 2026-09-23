// <copyright file="RemoteLaunchTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using PinchHitter;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

public class RemoteLaunchTests
{
    private const string SessionId = "grid-session";
    private const string BrowserWebSocketUrl = "ws://grid.example:4444/session/grid-session/se/bidi";

    [Fact]
    public async Task GridSessionIsCreatedUnderPathPrefixWithCapabilitiesHeadersAndCredentials()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, "/wd/hub");
        RemoteGridOptions options = new()
        {
            Capabilities =
            {
                ["browserVersion"] = "130",
                ["vendor:options"] = new Dictionary<string, object?>()
                {
                    ["numbers"] = new object[] { 1, 2L, 2.5, 3.25m, 1.5f, ulong.MaxValue },
                    ["flags"] = new List<bool> { true, false },
                    ["nothing"] = null,
                },
            },
            Headers = { ["X-Team"] = "web" },
        };
        Uri gridUrl = new($"http://user:p%40ss@localhost:{server.UrlFor("/").Port}/wd/hub");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(gridUrl, options).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        await instance.CloseAsync(TestContext.Current.CancellationToken);

        Assert.Equal(BrowserWebSocketUrl, instance.ConnectionString);
        ReceivedRequest newSession = Assert.Single(server.Requests, request => request.Method == "POST");
        Assert.Equal("/wd/hub/session", newSession.PathAndQuery);
        Assert.Equal($"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("user:p@ss"))}", newSession.Headers["Authorization"]);
        Assert.Equal("web", newSession.Headers["X-Team"]);
        JsonNode capabilities = JsonNode.Parse(newSession.Body)!["capabilities"]!["firstMatch"]![0]!;
        Assert.Equal("chrome", (string?)capabilities["browserName"]);
        Assert.True((bool?)capabilities["webSocketUrl"]);
        Assert.Equal("130", (string?)capabilities["browserVersion"]);
        Assert.Equal("[1,2,2.5,3.25,1.5,18446744073709551615]", capabilities["vendor:options"]!["numbers"]!.ToJsonString());
        Assert.Equal("[true,false]", capabilities["vendor:options"]!["flags"]!.ToJsonString());
        Assert.Null(capabilities["vendor:options"]!["nothing"]);
        Assert.Contains(server.Requests, request => request.Method == "DELETE" && request.PathAndQuery == $"/wd/hub/session/{SessionId}");
        Assert.All(server.Requests, request => Assert.StartsWith("Basic ", request.Headers["Authorization"]));
    }

    [Fact]
    public async Task LauncherCapabilitiesTakePrecedenceOverGridOptions()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, string.Empty);
        RemoteGridOptions options = new() { Capabilities = { ["browserName"] = "other", ["webSocketUrl"] = false } };
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).LaunchUsingRemoteGrid(server.UrlFor("/"), options).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode capabilities = JsonNode.Parse(Assert.Single(server.Requests, request => request.Method == "POST").Body)!["capabilities"]!["firstMatch"]![0]!;
        Assert.Equal("firefox", (string?)capabilities["browserName"]);
        Assert.True((bool?)capabilities["webSocketUrl"]);
    }

    [Fact]
    public async Task SafariGridSessionRequestsExperimentalWebSocketUrl()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, string.Empty);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Safari).LaunchUsingRemoteGrid(server.UrlFor("/")).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode capabilities = JsonNode.Parse(Assert.Single(server.Requests, request => request.Method == "POST").Body)!["capabilities"]!["firstMatch"]![0]!;
        Assert.True((bool?)capabilities["safari:experimentalWebSocketUrl"]);
    }

    [Theory]
    [MemberData(nameof(UnsupportedCapabilities))]
    public void BuildRejectsCapabilityValuesThatAreNotJson(object value, string expectedPath)
    {
        RemoteGridOptions options = new() { Capabilities = { ["vendor:options"] = new Dictionary<string, object?>() { ["value"] = value } } };

        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/"), options).Build);

        Assert.Contains(expectedPath, exception.Message);
    }

    public static TheoryData<object, string> UnsupportedCapabilities => new()
    {
        { DateTime.UnixEpoch, "vendor:options.value has a value of type DateTime" },
        { double.NaN, "vendor:options.value is NaN" },
        { new object[] { "valid", new Uri("http://example") }, "vendor:options.value[1] has a value of type Uri" },
        { new Dictionary<int, string>() { [1] = "one" }, "vendor:options.value has a key of type Int32" },
    };

    [Fact]
    public void BuildRejectsHeaderThatIsNotRequestHeader()
    {
        RemoteGridOptions options = new() { Headers = { ["Content-Type"] = "text/plain" } };

        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/"), options).Build);
    }

    [Theory]
    [InlineData("ws://grid.example/")]
    [InlineData("/wd/hub")]
    public void LaunchUsingRemoteGridRejectsUrlThatIsNotHttp(string url)
    {
        Assert.Throws<ArgumentException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri(url, UriKind.RelativeOrAbsolute)));
    }

    [Theory]
    [InlineData("http://127.0.0.1:9222/")]
    [InlineData("/session")]
    public void ConnectToExistingRejectsUrlThatIsNotWebSocket(string url)
    {
        Assert.Throws<ArgumentException>(() => BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(new Uri(url, UriKind.RelativeOrAbsolute)));
    }

    [Fact]
    public void BuildRejectsLocalSettingsWithRemoteStrategies()
    {
        Uri webSocketUrl = new("ws://127.0.0.1:9222/session");
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(webSocketUrl).WithHeadlessOption().Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(webSocketUrl).WithPort(9222).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(webSocketUrl).WithVersion(BrowserVersion.Specific("130.0")).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(webSocketUrl).WithConnection(ConnectionKind.Pipes).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(webSocketUrl).AtLocation("/opt/chrome").Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/")).WithPort(4444).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/")).ConnectToExisting(webSocketUrl));
    }

    [Fact]
    public async Task ConnectToExistingAttachesAndDetachesWithoutStartingAnything()
    {
        Uri webSocketUrl = new("ws://127.0.0.1:9222/session");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(webSocketUrl).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        bool wasRunning = instance.IsRunning;
        await Assert.ThrowsAsync<InvalidOperationException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));
        await instance.CloseAsync(TestContext.Current.CancellationToken);

        Assert.True(wasRunning);
        Assert.Equal(webSocketUrl.AbsoluteUri, instance.ConnectionString);
        Assert.Equal(0, instance.ProcessId);
        Assert.False(launcher.IsRunning);
        Assert.False(launcher.IsBiDiSessionInitialized);
        Assert.False(launcher.IsBrowserCloseAllowed);
    }

    [Theory]
    [InlineData("ws://127.0.0.1:9222/devtools/browser/0b8e2c1a", typeof(ChromiumTransport))]
    [InlineData("ws://127.0.0.1:9222/session", typeof(Transport))]
    public async Task ConnectToExistingUsesMapperOnlyForDevToolsEndpoint(string url, Type expectedTransportType)
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(new Uri(url)).Build();

        Transport transport = launcher.CreateTransport();

        Assert.IsType(expectedTransportType, transport);
    }

    private static void ServeGrid(DownloadServer server, string pathPrefix)
    {
        byte[] Json(string text) => Encoding.UTF8.GetBytes(text);
        const string JsonType = "application/json;charset=utf-8";
        server.AddResponses($"{pathPrefix}/status", new ServedResponse(HttpStatusCode.OK, Json("{\"value\":{\"ready\":true}}"), ContentType: JsonType));
        server.AddResponses(
            $"{pathPrefix}/session",
            HttpRequestMethod.Post,
            new ServedResponse(HttpStatusCode.OK, Json($"{{\"value\":{{\"sessionId\":\"{SessionId}\",\"capabilities\":{{\"webSocketUrl\":\"{BrowserWebSocketUrl}\"}}}}}}"), ContentType: JsonType));
        server.AddResponses($"{pathPrefix}/session/{SessionId}", HttpRequestMethod.Delete, new ServedResponse(HttpStatusCode.OK, Json("{\"value\":null}"), ContentType: JsonType));
    }
}
