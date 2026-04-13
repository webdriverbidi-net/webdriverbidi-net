// <copyright file="BrowserSetupSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/browser-setup.md

#pragma warning disable CS8600, CS8602, CS8620

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Diagnostics;
using System.Net.Http;
using OpenQA.Selenium.Chrome;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Protocol;

/// <summary>
/// Snippets for browser setup documentation.
/// </summary>
public static class BrowserSetupSamples
{
    /// <summary>
    /// Connect using WebSocket URL from /json/version.
    /// </summary>
    public static async Task ConnectWithWebSocketUrl(BiDiDriver driver)
    {
        #region ConnectwithWebSocketURL
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123-def");
        #endregion
    }

    /// <summary>
    /// WebSocket connection example.
    /// </summary>
    public static async Task WebSocketConnection(
        string webSocketUrl,
        NavigateCommandParameters navParams)
    {
        #region WebSocketConnection
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Connect to browser at WebSocket URL
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

        try
        {
            // Use the driver
            var result = await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Firefox geckodriver connection.
    /// </summary>
    public static async Task FirefoxConnection(BiDiDriver driver)
    {
        #region FirefoxConnection
        await driver.StartAsync("ws://localhost:4444/session");
        #endregion
    }

    /// <summary>
    /// Programmatic browser launch before connecting.
    /// </summary>
    public static async Task ProgrammaticBrowserLaunch(
        BiDiDriver driver,
        string webSocketUrl)
    {
        #region ProgrammaticBrowserLaunch
        // Launch Chrome via Process, wait, get WebSocket URL via HttpClient /json/version, then connect
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// Pipe launcher pattern - implement IPipeServerProcessProvider to launch browser with --remote-debugging-pipe.
    /// </summary>
    public static async Task PipeLauncherPattern(NavigateCommandParameters navParams)
    {
        #region PipeLauncherPattern
        // Launcher implements IPipeServerProcessProvider
        BrowserLauncher launcher = BrowserLauncher.Configure(Browser.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Stable)
            .AtAutomaticallyDownloadedLocation()
            .WithConnection(ConnectionType.Pipes)
            .Build();

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();

        try
        {
            // Create driver with launcher's transport
            BiDiDriver driver = new BiDiDriver(
                TimeSpan.FromSeconds(30),
                launcher.CreateTransport());

            await driver.StartAsync("pipes");

            // Use the driver
            var result = await driver.BrowsingContext.NavigateAsync(navParams);

            await driver.StopAsync();
        }
        finally
        {
            await launcher.QuitBrowserAsync();
            await launcher.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Selenium Manager - launch Chrome with fixed debug port, get WebSocket URL, connect BiDiDriver.
    /// Conceptual: WebDriverBiDi.NET doesn't include Selenium; use together if needed.
    /// </summary>
    public static async Task SeleniumManagerIntegration(ChromeOptions chromeOptions)
    {
        #region SeleniumManagerIntegration
        BiDiDriver driver = null;

        // This is conceptual - WebDriverBiDi.NET doesn't include Selenium Manager
        // But you can use them together
        var seleniumDriver = new ChromeDriver(chromeOptions);
        string wsUrl = (string)((Dictionary<string, object>)seleniumDriver
            .ExecuteCdpCommand("Target.getTargets", new Dictionary<string, object>()))
            ["webSocketDebuggerUrl"];

        await driver.StartAsync(wsUrl);
        #endregion
    }

    /// <summary>
    /// WebSocket launcher pattern - launch Chrome via Process, discover URL via /json/version, connect.
    /// </summary>
    public static async Task WebSocketLauncherPattern(BiDiDriver driver, string webSocketUrl)
    {
        #region WebSocketLauncherPattern
        // Launch Chrome
        Process chromeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chrome.exe",
                Arguments = "--remote-debugging-port=9222 --user-data-dir=C:\\temp\\chrome",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        chromeProcess.Start();

        // Wait for Chrome to start
        await Task.Delay(2000);

        // Get WebSocket URL
        using HttpClient client = new HttpClient();
        string json = await client.GetStringAsync("http://localhost:9222/json/version");
        // Parse JSON to get webSocketDebuggerUrl

        // Connect
        await driver.StartAsync(webSocketUrl);

        // Later: clean up
        chromeProcess.Kill();
    }
    #endregion
}

/// <summary>
/// Conceptual pipe launcher - implement IPipeServerProcessProvider to launch browser with --remote-debugging-pipe.
/// </summary>
public class BrowserSetupPipeLauncher : IPipeServerProcessProvider
{
    public Process? PipeServerProcess => null; // Implement: launch browser process with pipe flags

    public Transport CreateTransport() => new Transport(new PipeConnection(this));

    public Task StartAsync() => Task.CompletedTask;
    public Task LaunchBrowserAsync() => Task.CompletedTask;
    public Task QuitBrowserAsync() => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
}
