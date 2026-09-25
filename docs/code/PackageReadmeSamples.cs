// <copyright file="PackageReadmeSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Compiled counterparts for the code blocks in the packed package READMEs, src/WebDriverBiDi/README.md,
// src/WebDriverBiDi.Logging/README.md, src/WebDriverBiDi.Browsers/README.md and
// src/WebDriverBiDi.Extensions/README.md. The Browsers regions are also shown by
// docs/articles/browser-setup.md and getting-started.md, and the Extensions regions by
// docs/articles/advanced/webdriverbidi-extensions.md.
//
// Those files are shipped inside the NuGet packages and rendered by nuget.org, which knows nothing of
// DocFX, so their samples cannot be region references. Each fence names the region it mirrors in a
// '<!-- readme-csharp: path#Region -->' marker instead, and docs/tools/validate-doc-regions.sh compares
// the two, so a README sample is compiled here and cannot drift from the API.

namespace WebDriverBiDi.Docs.Code;

using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebDriverBiDi;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for the packed package READMEs. Compiled at build time to prevent API drift.
/// </summary>
public static class PackageReadmeSamples
{
    /// <summary>
    /// The WebDriverBiDi package README's quick start.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CoreQuickStart()
    {
        #region CoreQuickStart
        // Assumes the browser is running with a WebSocket listening for
        // WebDriver BiDi traffic. Note that your URL will be different here.
        string webSocketUrl = "ws://localhost:5555";

        // Set a timeout of 10 seconds for command responses.
        BiDiDriver driver = new(TimeSpan.FromSeconds(10));
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Logging package README's quick start.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task LoggingQuickStart()
    {
        #region LoggingQuickStart
        // Configure logging with WebDriverBiDi events
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddWebDriverBiDi(); // Add WebDriverBiDi event logging
        });

        await using var serviceProvider = services.BuildServiceProvider();

        // The bridge starts listening when the logging pipeline is built, which happens the first time
        // ILoggerFactory (or an ILogger) is resolved. A generic or web host does this at startup.
        _ = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Use WebDriverBiDi normally - events will be logged
        await using var driver = new BiDiDriver();
        await driver.StartAsync("ws://localhost:9222");
        #endregion
    }

    /// <summary>
    /// Forwarding the driver's own log messages to an <see cref="ILogger"/>.
    /// </summary>
    /// <param name="driver">The driver whose messages to forward.</param>
    /// <param name="logger">The logger to forward them to.</param>
    public static void LoggingOnLogMessage(BiDiDriver driver, ILogger logger)
    {
        #region LoggingOnLogMessage
        driver.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            LogLevel level = e.Level switch
            {
                WebDriverBiDiLogLevel.Trace => LogLevel.Trace,
                WebDriverBiDiLogLevel.Debug => LogLevel.Debug,
                WebDriverBiDiLogLevel.Info => LogLevel.Information,
                WebDriverBiDiLogLevel.Warn => LogLevel.Warning,
                WebDriverBiDiLogLevel.Error => LogLevel.Error,
                _ => LogLevel.Critical,
            };
            logger.Log(level, "{Component}: {Message}", e.ComponentName, e.Message);
        });
        #endregion
    }

    /// <summary>
    /// The default event level.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void LoggingDefaultConfiguration(IServiceCollection services)
    {
        #region LoggingDefaultConfiguration
        services.AddLogging(builder => builder.AddWebDriverBiDi());
        #endregion
    }

    /// <summary>
    /// A custom event level.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void LoggingCustomEventLevel(IServiceCollection services)
    {
        #region LoggingCustomEventLevel
        services.AddLogging(builder => builder.AddWebDriverBiDi(EventLevel.Verbose)); // Capture all events
        #endregion
    }

    /// <summary>
    /// Filtering the bridge's output with the standard logging filters.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void LoggingFiltering(IServiceCollection services)
    {
        #region LoggingFiltering
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddWebDriverBiDi();
            builder.AddFilter("WebDriverBiDi.Logging.WebDriverBiDiEventSourceLogger", LogLevel.Information); // Only Info and above
        });
        #endregion
    }

    /// <summary>
    /// Adding the bridge to an ASP.NET Core host.
    /// </summary>
    /// <param name="args">The process command-line arguments.</param>
    public static void LoggingAspNetCore(string[] args)
    {
        #region LoggingAspNetCore
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddWebDriverBiDi();
        var app = builder.Build();
        #endregion
    }

    /// <summary>
    /// Adding the bridge to a console application.
    /// </summary>
    public static void LoggingConsoleApplication()
    {
        #region LoggingConsoleApplication
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddWebDriverBiDi(EventLevel.Verbose);
        });
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's quick start.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersQuickStart()
    {
        #region BrowsersQuickStart
        // Downloads Chrome for Testing into a local cache on first use.
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithHeadlessOption()
            .Build();
        await using BrowserInstance browser = await launcher.LaunchAsync();

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), launcher.CreateTransport());
        await driver.StartAsync(browser.ConnectionString);

        // A browser launched through a driver executable already has a session.
        if (!launcher.IsBiDiSessionInitialized)
        {
            await driver.Session.NewSessionAsync(new NewCommandParameters());
        }
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's examples of choosing the browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersChoosingBrowser()
    {
        #region BrowsersChoosingBrowser
        // The newest release of Chrome 131, as the smaller chrome-headless-shell build.
        await using BrowserLauncher chrome = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithVersion(BrowserVersion.Milestone(131))
            .WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true })
            .Build();

        // Firefox ESR, with an extra argument and a preference.
        FirefoxLaunchOptions firefoxOptions = new();
        firefoxOptions.Preferences["browser.startup.page"] = 0;
        await using BrowserLauncher firefox = BrowserLauncher.Configure(BrowserKind.Firefox)
            .WithReleaseChannel(BrowserReleaseChannel.ExtendedSupport)
            .WithArguments("--width=1280", "--height=800")
            .WithBrowserOptions(firefoxOptions)
            .Build();

        // The Chrome installed on this machine, launched through a downloaded chromedriver.
        await using BrowserLauncher installedChrome = BrowserLauncher.Configure(BrowserKind.Chrome)
            .AtDefaultInstallationLocation()
            .LaunchUsingDriver()
            .Build();
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's remote grid example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersRemoteGrid()
    {
        #region BrowsersRemoteGrid
        RemoteGridOptions gridOptions = new();
        gridOptions.Headers["X-Build-Id"] = "nightly-1234";

        // Credentials in the URL are sent as Basic authorization.
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .LaunchUsingRemoteGrid(new Uri("https://user:access-key@grid.example.com/wd/hub"), gridOptions)
            .WithSessionCapability("browserVersion", "131")
            .WithSessionCapability("goog:chromeOptions", new Dictionary<string, object?>() { ["args"] = new[] { "--headless=new" } })
            .Build();
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's session capabilities example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersSessionCapabilities()
    {
        #region BrowsersSessionCapabilities
        ManualProxyConfiguration proxy = new() { HttpProxy = "proxy.example.com:3128", SslProxy = "proxy.example.com:3128" };
        proxy.NoProxyAddresses.Add("localhost");

        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox)
            .LaunchUsingDriver()
            .WithSessionCapability("proxy", proxy)
            .WithSessionCapability("acceptInsecureCerts", true)
            .Build();
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's example of attaching to a running browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersConnectToExisting()
    {
        #region BrowsersConnectToExisting
        // Chrome started with --remote-debugging-port=9222 reports this DevTools URL at
        // http://127.0.0.1:9222/json/version; it is reached through the WebDriver BiDi mapper.
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .ConnectToExisting(new Uri("ws://127.0.0.1:9222/devtools/browser/0b8e2c1a-6d9e-4f35-a1c2-3b4d5e6f7a8b"))
            .Build();
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's example of locating without launching.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersLocateOnly()
    {
        #region BrowsersLocateOnly
        string firefoxPath = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Beta);
        string? geckodriverPath = await DriverLocator.FindDriverAsync(BrowserKind.Firefox);
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Browsers package README's download options example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersDownloadOptions()
    {
        #region BrowsersDownloadOptions
        BrowserDownloadOptions downloadOptions = new()
        {
            CacheDirectory = "/ci/cache/browsers",
            ManifestUrl = new Uri("https://mirror.example.com/browsers/manifest.json"),
            Progress = new Progress<BrowserDownloadProgress>(report => Console.WriteLine($"{report.Name}: {report.BytesReceived} bytes")),
        };

        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithDownloadOptions(downloadOptions)
            .Build();
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Extensions package README's quick start.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExtensionsQuickStart(BiDiDriver driver)
    {
        #region ExtensionsQuickStart
        IReadOnlyList<BrowsingContextInfo> tabs = await driver.BrowsingContext.GetTopLevelBrowsingContextsAsync();
        string contextId = tabs[0].BrowsingContextId;

        await driver.BrowsingContext.NavigateAsync(contextId, "https://example.com", ReadinessState.Complete);
        StringRemoteValue title = await driver.Script.CallFunctionAsync<StringRemoteValue>(contextId, "() => document.title");
        byte[] png = await driver.BrowsingContext.CaptureScreenshotAsync(contextId);
        File.WriteAllBytes("example.png", png);
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Extensions package README's input example.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context to type into.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExtensionsInput(BiDiDriver driver, string contextId)
    {
        #region ExtensionsInput
        IReadOnlyList<SharedReference> searchBoxes = await driver.BrowsingContext.LocateNodesByCssSelectorAsync(contextId, "input[name=q]");

        InputBuilder builder = new InputBuilder()
            .AddClickOnElementAction(searchBoxes[0])
            .AddSendKeysToActiveElementAction("WebDriver BiDi")
            .AddSendKeysToActiveElementAction(Keys.Enter);
        await driver.Input.PerformActionsAsync(contextId, builder);

        // Releases any keys or buttons an action sequence left pressed.
        await driver.Input.ReleaseActionsAsync(contextId);
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Extensions package README's network capture example.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context to monitor.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExtensionsNetworkCapture(BiDiDriver driver, string contextId)
    {
        #region ExtensionsNetworkCapture
        NetworkTrafficMonitorOptions options = new();
        options.BrowsingContextIds.Add(contextId);

        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync();
        await driver.BrowsingContext.NavigateAsync(contextId, "https://example.com", ReadinessState.Complete);

        // Waits up to ten seconds for requests still in flight; any that are still unfinished are kept for the next call.
        IReadOnlyList<NetworkRequest> traffic = await monitor.GetCapturedTrafficAsync(TimeSpan.FromSeconds(10));
        foreach (NetworkRequest request in traffic)
        {
            Console.WriteLine($"{request.ResponseStatusCode} {request.Method} {request.Url}");
        }

        File.WriteAllText("example.har", HarGenerator.Generate(traffic));
        #endregion
    }

    /// <summary>
    /// The WebDriverBiDi.Extensions package README's request modification and authentication example.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExtensionsNetworkModification(BiDiDriver driver)
    {
        #region ExtensionsNetworkModification
        NetworkTrafficMonitorOptions options = new()
        {
            CaptureBodies = false,
        };

        NetworkRequestModification toStaging = new("https://api.example.com/v1/orders")
        {
            ReplacementUrl = "https://staging-api.example.com/v1/orders",
        };
        toStaging.AdditionalHeaders["X-Test-Run"] = "nightly";
        options.RequestModifications.Add(toStaging);

        options.AuthCredentials.Add(new AuthChallengeCredentials("tester", "secret") { Realm = "staging" });

        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync();
        #endregion
    }
}
