// <copyright file="BrowserTestHelper.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration;

using WebDriverBiDi.Client.Launchers;

/// <summary>
/// Helper class for managing browser-specific integration tests across different environments.
/// </summary>
public static class BrowserTestHelper
{
    /// <summary>
    /// Gets a value indicating whether the tests are running in a CI environment.
    /// </summary>
    public static bool IsCI => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>
    /// Ensures the specified browser is available for testing.
    /// In CI environments, skips the test if the browser executable is not configured.
    /// In local/IDE environments, assumes the browser launcher will auto-detect system browsers.
    /// </summary>
    /// <param name="browser">The browser type to check.</param>
    /// <exception cref="IgnoreException">Thrown when running in CI and the browser is not configured.</exception>
    public static void EnsureBrowserAvailable(TestBrowser browser)
    {
        if (IsCI)
        {
            string executablePath = GetBrowserExecutable(browser);
            if (string.IsNullOrEmpty(executablePath))
            {
                Assert.Ignore($"Skipping {browser} test in CI - {GetExecutableEnvironmentVariable(browser)} not configured");
            }
        }
    }

    /// <summary>
    /// Gets the browser executable path from environment variables.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The path to the browser executable, or empty string if not set.</returns>
    public static string GetBrowserExecutable(TestBrowser browser)
    {
        string envVar = GetExecutableEnvironmentVariable(browser);
        return Environment.GetEnvironmentVariable(envVar) ?? string.Empty;
    }

    /// <summary>
    /// Converts a browser enum value to its string representation for command-line arguments.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The lowercase string name of the browser (e.g., "firefox", "chrome").</returns>
    public static string ToBrowserString(TestBrowser browser)
    {
        return browser.ToString().ToLowerInvariant();
    }

    public static BrowserLauncher GetBrowserLauncher(TestBrowser browser)
    {
        string executablePath = GetBrowserExecutable(browser);

        Browser launcherBrowser = browser switch
        {
            TestBrowser.Firefox => Browser.Firefox,
            TestBrowser.Chrome => Browser.Chrome,
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };

        BrowserLauncherBuilder builder = BrowserLauncher.Configure(launcherBrowser)
            .WithReleaseChannel(BrowserReleaseChannel.Alpha);

        if (string.IsNullOrEmpty(executablePath))
        {
            builder.AtAutomaticallyDownloadedLocation();
        }
        else
        {
            builder.AtLocation(executablePath);
        }

        return builder.Build();
    }

    /// <summary>
    /// Gets the environment variable name for the specified browser's executable.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The environment variable name (e.g., "FIREFOX_EXECUTABLE").</returns>
    private static string GetExecutableEnvironmentVariable(TestBrowser browser)
    {
        return browser switch
        {
            TestBrowser.Firefox => "FIREFOX_EXECUTABLE",
            TestBrowser.Chrome => "CHROME_EXECUTABLE",
            _ => $"{browser.ToString().ToUpperInvariant()}_EXECUTABLE"
        };
    }
}
