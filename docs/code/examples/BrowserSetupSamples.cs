// <copyright file="BrowserSetupSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/browser-setup.md

#pragma warning disable CS8600, CS8602, CS8620

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using OpenQA.Selenium.Chrome;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for browser setup documentation.
/// </summary>
public static class BrowserSetupSamples
{
    /// <summary>
    /// Connect using the webSocketUrl returned by a driver's classic new-session response.
    /// </summary>
    public static async Task ConnectWithWebSocketUrl(BiDiDriver driver)
    {
        #region ConnectwithWebSocketURL
        // The value of "webSocketUrl" from chromedriver's new-session response
        await driver.StartAsync("ws://localhost:9515/session/8a4d1c2e-0b7f-4c9a-9d3e-5f6a7b8c9d0e");
        #endregion
    }

    #region CreateSessionThroughDriver
    /// <summary>
    /// Creates a WebDriver session through a classic driver executable (chromedriver, msedgedriver,
    /// geckodriver) and returns the WebDriver BiDi WebSocket URL it advertises.
    /// </summary>
    public static async Task<(string SessionId, string WebSocketUrl)> CreateBiDiSessionAsync(
        string driverUrl = "http://localhost:9515",
        bool headless = false)
    {
        using HttpClient client = new HttpClient();

        // "webSocketUrl": true asks the driver to expose the session over WebDriver BiDi.
        // Browser arguments go in the vendor-specific options (goog:chromeOptions here).
        string body = $$"""
            {
              "capabilities": {
                "alwaysMatch": {
                  "webSocketUrl": true,
                  "goog:chromeOptions": { "args": [{{(headless ? "\"--headless=new\"" : string.Empty)}}] }
                }
              }
            }
            """;
        using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync($"{driverUrl}/session", content);
        string json = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement value = doc.RootElement.GetProperty("value");
        string sessionId = value.GetProperty("sessionId").GetString()
            ?? throw new InvalidOperationException("The driver did not return a session ID.");
        string webSocketUrl = value.GetProperty("capabilities").TryGetProperty("webSocketUrl", out JsonElement url)
            ? url.GetString() ?? string.Empty
            : string.Empty;
        if (string.IsNullOrEmpty(webSocketUrl))
        {
            throw new InvalidOperationException(
                "The driver did not return a webSocketUrl; the browser or driver may not support WebDriver BiDi.");
        }

        return (sessionId, webSocketUrl);
    }
    #endregion

    /// <summary>
    /// WebSocket connection example.
    /// </summary>
    public static async Task WebSocketConnection(NavigateCommandParameters navParams)
    {
        #region WebSocketConnection
        // chromedriver is already running: chromedriver --port=9515
        (string sessionId, string webSocketUrl) = await CreateBiDiSessionAsync("http://localhost:9515");

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);

        // The driver created the session; do NOT call Session.NewSessionAsync here.
        try
        {
            NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            // session.end closes the browser and releases the driver's session
            await driver.Session.EndAsync();
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

        // No session exists yet on this endpoint; create one
        NewCommandResult sessionResult = await driver.Session.NewSessionAsync(new NewCommandParameters());
        #endregion
    }

    /// <summary>
    /// Firefox launched directly with --remote-debugging-port.
    /// </summary>
    public static async Task FirefoxDirectConnection(BiDiDriver driver)
    {
        #region FirefoxDirectConnection
        // firefox --remote-debugging-port=9222 exposes WebDriver BiDi directly
        await driver.StartAsync("ws://localhost:9222/session");
        NewCommandResult sessionResult = await driver.Session.NewSessionAsync(new NewCommandParameters());
        #endregion
    }

    /// <summary>
    /// Programmatic browser launch before connecting.
    /// </summary>
    public static async Task ProgrammaticBrowserLaunch(BiDiDriver driver)
    {
        #region ProgrammaticBrowserLaunch
        // Start chromedriver (see the WebSocket Launcher Pattern), create a session, then connect
        (string sessionId, string webSocketUrl) = await CreateBiDiSessionAsync("http://localhost:9515");
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// Pipe launcher pattern - implement IPipeServerProcessProvider to launch browser with --remote-debugging-pipe.
    /// </summary>
    public static async Task PipeLauncherPattern(NavigateCommandParameters navParams)
    {
        #region PipeLauncherPattern
        // Launcher implements IPipeServerProcessProvider. Its CreateTransport() returns a
        // ChromiumTransport, which installs the BiDi-over-CDP mapper the pipe requires.
        BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Stable)
            .AtAutomaticallyDownloadedLocation()
            .WithConnection(ConnectionKind.Pipes)
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

            // The mapper does not create a session; do it here
            await driver.Session.NewSessionAsync(new NewCommandParameters());

            // Use the driver
            NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navParams);

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
    /// Selenium integration - let Selenium create the session with webSocketUrl, then connect BiDiDriver to it.
    /// </summary>
    public static async Task SeleniumManagerIntegration(ChromeOptions chromeOptions)
    {
        #region SeleniumManagerIntegration
        // Ask Selenium to request a WebDriver BiDi WebSocket for the session it creates.
        // Selenium Manager locates (or downloads) chromedriver and the browser for you.
        chromeOptions.UseWebSocketUrl = true;
        ChromeDriver seleniumDriver = new ChromeDriver(chromeOptions);

        string webSocketUrl = seleniumDriver.Capabilities.GetCapability("webSocketUrl") as string
            ?? throw new InvalidOperationException("Selenium did not return a webSocketUrl capability.");

        BiDiDriver driver = new BiDiDriver();
        await driver.StartAsync(webSocketUrl);

        // The session already exists; do not call Session.NewSessionAsync.
        // Stop the BiDiDriver before seleniumDriver.Quit() ends the session.
        #endregion
    }

    /// <summary>
    /// WebSocket launcher pattern - launch chromedriver via Process, create a session, connect.
    /// </summary>
    public static async Task WebSocketLauncherPattern(BiDiDriver driver)
    {
        #region WebSocketLauncherPattern
        // Launch chromedriver (it launches Chrome itself when a session is created)
        Process driverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chromedriver",
                Arguments = "--port=9515",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            },
        };
        driverProcess.Start();

        // Wait until the driver answers /status
        using HttpClient client = new HttpClient();
        while (true)
        {
            try
            {
                using HttpResponseMessage status = await client.GetAsync("http://localhost:9515/status");
                if (status.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch (HttpRequestException)
            {
                // Not listening yet
            }

            await Task.Delay(100);
        }

        // Create the session and connect to its WebDriver BiDi endpoint
        (string sessionId, string webSocketUrl) = await CreateBiDiSessionAsync("http://localhost:9515");
        await driver.StartAsync(webSocketUrl);

        // Later: clean up - end the session (closes the browser), then stop the driver process
        await driver.Session.EndAsync();
        await driver.StopAsync();
        driverProcess.Kill();
        #endregion
    }
}

/// <summary>
/// Conceptual pipe launcher - implement IPipeServerProcessProvider to launch browser with --remote-debugging-pipe.
/// </summary>
#region ImplementingIPipeServerProcessProvider
public class BrowserSetupPipeLauncher : IPipeServerProcessProvider
{
    public Process? PipeServerProcess => null; // Implement: launch browser process with pipe flags

    // For a Chromium browser the pipe carries CDP, so the Transport returned here must translate
    // WebDriver BiDi to CDP (see ChromiumTransport in the WebDriverBiDi.Client demonstration
    // library, which injects the chromium-bidi mapper). A plain Transport is shown only for shape.
    public Transport CreateTransport() => new Transport(new PipeConnection(this));

    public Task StartAsync() => Task.CompletedTask;
    public Task LaunchBrowserAsync() => Task.CompletedTask;
    public Task QuitBrowserAsync() => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
}
#endregion
