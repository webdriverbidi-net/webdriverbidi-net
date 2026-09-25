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
using WebDriverBiDi.Session;

public class RemoteLaunchTests
{
    private const string SessionId = "grid-session";
    private const string BrowserWebSocketUrl = "ws://grid.example:4444/session/grid-session/se/bidi";

    [Fact]
    public async Task GridSessionIsCreatedUnderPathPrefixWithCapabilitiesHeadersAndCredentials()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, "/wd/hub");
        RemoteGridOptions options = new() { Headers = { ["X-Team"] = "web" } };
        Dictionary<string, object?> vendorOptions = new()
        {
            ["numbers"] = new object[] { 1, 2L, 2.5, 3.25m, 1.5f, ulong.MaxValue, (sbyte)-1, (byte)2, (short)3, (ushort)4, 5u },
            ["flags"] = new List<bool> { true, false },
            ["nothing"] = null,
        };
        Uri gridUrl = new($"http://user:p%40ss@localhost:{server.UrlFor("/").Port}/wd/hub");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .LaunchUsingRemoteGrid(gridUrl, options)
            .WithSessionCapability("browserVersion", "130")
            .WithSessionCapability("vendor:options", vendorOptions)
            .Build();
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
        Assert.Equal("[1,2,2.5,3.25,1.5,18446744073709551615,-1,2,3,4,5]", capabilities["vendor:options"]!["numbers"]!.ToJsonString());
        Assert.Equal("[true,false]", capabilities["vendor:options"]!["flags"]!.ToJsonString());
        Assert.Null(capabilities["vendor:options"]!["nothing"]);
        Assert.Contains(server.Requests, request => request.Method == "DELETE" && request.PathAndQuery == $"/wd/hub/session/{SessionId}");
        Assert.All(server.Requests, request => Assert.StartsWith("Basic ", request.Headers["Authorization"]));
    }

    [Theory]
    [InlineData(BrowserKind.Firefox, true, "browserName")]
    [InlineData(BrowserKind.Chrome, true, "webSocketUrl")]
    [InlineData(BrowserKind.Chrome, false, "goog:chromeOptions")]
    [InlineData(BrowserKind.Firefox, false, "moz:firefoxOptions")]
    [InlineData(BrowserKind.Safari, false, "safari:experimentalWebSocketUrl")]
    [InlineData(BrowserKind.Safari, false, "safari:options")]
    public void BuildRejectsCapabilityTheLauncherSets(BrowserKind browser, bool onGrid, string name)
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser);
        builder = onGrid ? builder.LaunchUsingRemoteGrid(new Uri("http://grid.example/")) : builder.LaunchUsingDriver();
        if (browser == BrowserKind.Safari)
        {
            builder.AtDefaultInstallationLocation();
        }

        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(builder.WithSessionCapability(name, "value").Build);

        Assert.Contains($"The {name} capability cannot be added", exception.Message);
    }

    [Fact]
    public async Task ProxyIsWrittenAsTheCoreLibraryWritesIt()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, string.Empty);
        ManualProxyConfiguration proxy = new() { HttpProxy = "proxy.local:3128", SocksProxy = "socks.local:1080", SocksVersion = 5 };
        proxy.NoProxyAddresses.Add("localhost");
        proxy.AdditionalData["vendor:setting"] = "on";
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(server.UrlFor("/")).WithSessionCapability("proxy", proxy).Build();
        proxy.HttpProxy = "changed.local:3128";
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode capabilities = JsonNode.Parse(Assert.Single(server.Requests, request => request.Method == "POST").Body)!["capabilities"]!["firstMatch"]![0]!;
        JsonNode expected = JsonNode.Parse("""{"proxyType":"manual","httpProxy":"proxy.local:3128","socksProxy":"socks.local:1080","socksVersion":5,"noProxy":["localhost"],"vendor:setting":"on"}""")!;
        Assert.True(JsonNode.DeepEquals(expected, capabilities["proxy"]), capabilities["proxy"]!.ToJsonString());
    }

    [Fact]
    public void BuildRejectsProxyThatIsNotProxyConfiguration()
    {
        Dictionary<string, object?> proxyDictionary = new() { ["proxyType"] = "manual", ["httpProxy"] = "proxy.local:3128" };

        BrowserLauncherConfigurationException dictionary = Assert.Throws<BrowserLauncherConfigurationException>(GridBuilder().WithSessionCapability("proxy", proxyDictionary).Build);
        BrowserLauncherConfigurationException misplaced = Assert.Throws<BrowserLauncherConfigurationException>(GridBuilder().WithSessionCapability("vendor:proxy", new PacProxyConfiguration("http://proxy.local/proxy.pac")).Build);

        Assert.Contains("must be a ProxyConfiguration", dictionary.Message);
        Assert.Contains("can only be the value of the proxy capability", misplaced.Message);
    }

    [Fact]
    public void BuildRejectsProxyWhoseAdditionalDataRepeatsAPropertyOrCannotBeWritten()
    {
        ManualProxyConfiguration repeated = new() { HttpProxy = "proxy.local:3128" };
        repeated.AdditionalData["httpProxy"] = "other.local:3128";
        ManualProxyConfiguration unwritable = new();
        unwritable.AdditionalData["vendor:setting"] = new UnregisteredValue();

        BrowserLauncherConfigurationException repeatedException = Assert.Throws<BrowserLauncherConfigurationException>(GridBuilder().WithSessionCapability("proxy", repeated).Build);
        BrowserLauncherConfigurationException unwritableException = Assert.Throws<BrowserLauncherConfigurationException>(GridBuilder().WithSessionCapability("proxy", unwritable).Build);

        Assert.Contains("entry named httpProxy", repeatedException.Message);
        Assert.StartsWith("The proxy capability cannot be written", unwritableException.Message);
    }

    [Fact]
    public void BuildRejectsSessionCapabilitiesWhenTheSessionIsNotCreatedByTheLauncher()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).WithSessionCapability("browserVersion", "130").Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(new Uri("ws://127.0.0.1:9222/session")).WithSessionCapability("browserVersion", "130").Build);
        Assert.Throws<ArgumentException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).WithSessionCapability(string.Empty, "value"));
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

    [Fact]
    public async Task SafariGridSessionKeepsCallersWebSocketUrlCapability()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, string.Empty);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Safari).LaunchUsingRemoteGrid(server.UrlFor("/")).WithSessionCapability("safari:experimentalWebSocketUrl", false).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        Transport transport = launcher.CreateTransport();

        JsonNode capabilities = JsonNode.Parse(Assert.Single(server.Requests, request => request.Method == "POST").Body)!["capabilities"]!["firstMatch"]![0]!;
        Assert.False((bool?)capabilities["safari:experimentalWebSocketUrl"]);
        Assert.IsType<Transport>(transport);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task SessionWithoutWebSocketUrlIsEnded(HttpStatusCode deleteStatus)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses("/status", new ServedResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes("{\"value\":{\"ready\":true}}"), ContentType: "application/json"));
        server.AddResponses("/session", HttpRequestMethod.Post, new ServedResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes($"{{\"value\":{{\"sessionId\":\"{SessionId}\",\"capabilities\":{{}}}}}}"), ContentType: "application/json"));
        server.AddResponses($"/session/{SessionId}", HttpRequestMethod.Delete, new ServedResponse(deleteStatus, Encoding.UTF8.GetBytes("{\"value\":null}"), ContentType: "application/json"));
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(server.UrlFor("/")).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.False(launcher.IsRunning);
        Assert.Equal(1, server.RequestCount($"/session/{SessionId}"));
    }

    [Fact]
    public async Task DisposingInstanceWhoseQuitFailsDoesNotThrow()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGrid(server, string.Empty);
        server.AddResponses($"/session/{SessionId}", HttpRequestMethod.Delete, new ServedResponse(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes("{}"), ContentType: "application/json"));
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(server.UrlFor("/")).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await instance.DisposeAsync();

        Assert.False(instance.IsRunning);
        Assert.Equal(1, server.RequestCount($"/session/{SessionId}"));
    }

    [Theory]
    [MemberData(nameof(UnsupportedCapabilities))]
    public void BuildRejectsCapabilityValuesThatAreNotJson(object value, string expectedPath)
    {
        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(GridBuilder().WithSessionCapability("vendor:options", new Dictionary<string, object?>() { ["value"] = value }).Build);

        Assert.Contains(expectedPath, exception.Message);
    }

    public static TheoryData<object, string> UnsupportedCapabilities => new()
    {
        { DateTime.UnixEpoch, "vendor:options.value has a value of type DateTime" },
        { double.NaN, "vendor:options.value is NaN" },
        { float.PositiveInfinity, "which JSON cannot represent" },
        { float.NaN, "which JSON cannot represent" },
        { new object[] { "valid", new Uri("http://example") }, "vendor:options.value[1] has a value of type Uri" },
        { new Dictionary<int, string>() { [1] = "one" }, "vendor:options.value has a key of type Int32" },
    };

    [Theory]
    [InlineData("Content-Type")]
    [InlineData("Not A Header Name")]
    public void BuildRejectsHeaderThatIsNotRequestHeader(string name)
    {
        RemoteGridOptions options = new() { Headers = { [name] = "text/plain" } };

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

    private static BrowserLauncherBuilder GridBuilder() => BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/"));

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

    private sealed class UnregisteredValue
    {
    }
}
