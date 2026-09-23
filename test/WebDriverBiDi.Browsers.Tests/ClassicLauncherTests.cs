// <copyright file="ClassicLauncherTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Net;
using System.Text;
using PinchHitter;
using WebDriverBiDi.Browsers.TestUtilities;

// A scripted grid stands in for any WebDriver Classic remote end.
public class ClassicLauncherTests
{
    private const string JsonType = "application/json;charset=utf-8";
    private const string ReadyStatus = "{\"value\":{\"ready\":true}}";
    private const string SessionResponse = "{\"value\":{\"sessionId\":\"s1\",\"capabilities\":{\"webSocketUrl\":\"ws://grid.example/session/s1\"}}}";

    [Fact]
    public async Task StartFailsWhenRemoteEndNeverReportsReady()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses("/status", new ServedResponse(HttpStatusCode.ServiceUnavailable, Encoding.UTF8.GetBytes("{}"), ContentType: JsonType));
        await using BrowserLauncher launcher = CreateLauncher(server, BrowserKind.Chrome, TimeSpan.FromMilliseconds(300));

        BrowserLaunchException exception = await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.StartAsync(TestContext.Current.CancellationToken));

        Assert.Contains("did not report that it was ready within 0.3 seconds", exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "{\"value\":{\"error\":\"session not created\"}}", "Received status code InternalServerError")]
    [InlineData(HttpStatusCode.OK, "{\"value\":{\"capabilities\":{\"webSocketUrl\":\"ws://grid.example/\"}}}", "Could not detect session ID")]
    [InlineData(HttpStatusCode.OK, "{\"value\":{\"sessionId\":\"s1\",\"capabilities\":{}}}", "may not support the WebDriver BiDi protocol")]
    [InlineData(HttpStatusCode.OK, "{}", "Could not detect session ID")]
    [InlineData(HttpStatusCode.OK, "{\"value\":{\"sessionId\":null}}", "Could not detect session ID")]
    [InlineData(HttpStatusCode.OK, "{\"value\":{\"sessionId\":\"s1\",\"capabilities\":{\"webSocketUrl\":null}}}", "may not support the WebDriver BiDi protocol")]
    public async Task UnusableNewSessionResponseFailsLaunch(HttpStatusCode statusCode, string body, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeRemoteEnd(server, new ServedResponse(statusCode, Encoding.UTF8.GetBytes(body), ContentType: JsonType));
        await using BrowserLauncher launcher = CreateLauncher(server, BrowserKind.Chrome);
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserLaunchException exception = await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public async Task LaunchWhileSessionIsOpenIsRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeRemoteEnd(server, new ServedResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(SessionResponse), ContentType: JsonType));
        await using BrowserLauncher launcher = CreateLauncher(server, BrowserKind.Chrome);
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.True(launcher.IsRunning);
        Assert.True(launcher.IsBiDiSessionInitialized);
    }

    [Fact]
    public async Task FailedQuitIsReportedAndDisposeFallsBackToKill()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeRemoteEnd(server, new ServedResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(SessionResponse), ContentType: JsonType));
        server.AddResponses("/session/s1", HttpRequestMethod.Delete, new ServedResponse(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes("{}"), ContentType: JsonType));
        await using BrowserLauncher launcher = CreateLauncher(server, BrowserKind.Firefox);
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        CannotQuitBrowserException exception = await Assert.ThrowsAsync<CannotQuitBrowserException>(() => instance.CloseAsync(TestContext.Current.CancellationToken));
        await instance.KillAsync(TestContext.Current.CancellationToken);
        await instance.DisposeAsync();
        await instance.DisposeAsync();

        Assert.Contains("InternalServerError", exception.Message);
        Assert.False(launcher.IsBrowserCloseAllowed);
        Assert.False(instance.IsRunning);
        Assert.Equal(2, server.RequestCount("/session/s1"));
    }

    [Fact]
    public async Task QuitWithoutSessionSendsNothing()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        await using BrowserLauncher launcher = CreateLauncher(server, BrowserKind.Chrome);

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        Assert.True(launcher.IsBrowserCloseAllowed);
        Assert.Empty(server.RequestedUrls);
    }

    private static BrowserLauncher CreateLauncher(DownloadServer server, BrowserKind browser, TimeSpan? launchTimeout = null)
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser).LaunchUsingRemoteGrid(server.UrlFor("/"));
        if (launchTimeout is TimeSpan timeout)
        {
            builder.WithLaunchTimeout(timeout);
        }

        return builder.Build();
    }

    private static void ServeRemoteEnd(DownloadServer server, ServedResponse newSessionResponse)
    {
        server.AddResponses("/status", new ServedResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(ReadyStatus), ContentType: JsonType));
        server.AddResponses("/session", HttpRequestMethod.Post, newSessionResponse);
    }
}
