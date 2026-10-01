// <copyright file="BrowsersReadmeSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Compiled counterparts for the code blocks in src/Dramaturge.Browsers/README.md, which nuget.org renders, so its
// samples cannot be region references. Each fence names the region it mirrors in a
// '<!-- readme-csharp: path#Region -->' marker, and docs/tools/validate-doc-regions.sh compares the two. The regions
// are also shown by docs/articles/browser-setup.md.

namespace Dramaturge.Docs.Code;

using Dramaturge.Browsers;
using WebDriverBiDi;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for the Dramaturge.Browsers package README. Compiled at build time to prevent API drift.
/// </summary>
public static class BrowsersReadmeSamples
{
    /// <summary>
    /// The Dramaturge.Browsers package README's quick start.
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
    /// The Dramaturge.Browsers package README's examples of choosing the browser.
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
    /// The Dramaturge.Browsers package README's remote grid example.
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
    /// The Dramaturge.Browsers package README's session capabilities example.
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
    /// The Dramaturge.Browsers package README's example of attaching to a running browser.
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
    /// The Dramaturge.Browsers package README's example of locating without launching.
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
    /// The Dramaturge.Browsers package README's cache management example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowsersCacheManagement()
    {
        #region BrowsersCacheManagement
        foreach (CachedInstallation installation in BrowserCache.List())
        {
            Console.WriteLine($"{installation} ({installation.Channel ?? "driver"}): {installation.Size / (1024 * 1024)} MB");

            // Keeps only what a channel or driver request has resolved to in the last week.
            if (installation.LastResolved is null || installation.LastResolved < DateTimeOffset.UtcNow.AddDays(-7))
            {
                await BrowserCache.RemoveAsync(installation);
            }
        }
        #endregion
    }

    /// <summary>
    /// The Dramaturge.Browsers package README's download options example.
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
}
