// <copyright file="BrowserTestHelper.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration;

using WebDriverBiDi.Client;
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
    public static void EnsureBrowserAvailable(Browser browser)
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
    public static string GetBrowserExecutable(Browser browser)
    {
        string envVar = GetExecutableEnvironmentVariable(browser);
        return Environment.GetEnvironmentVariable(envVar) ?? string.Empty;
    }

    /// <summary>
    /// Converts a browser enum value to its string representation for command-line arguments.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The lowercase string name of the browser (e.g., "firefox", "chrome").</returns>
    public static string ToBrowserString(Browser browser)
    {
        return browser.ToString().ToLowerInvariant();
    }

    public static BrowserLauncher GetBrowserLauncher(Browser browser)
    {
        BrowserType browserType = browser switch
        {
            Browser.Firefox => BrowserType.Firefox,
            Browser.Chrome => BrowserType.Chrome,
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };

        string executablePath = GetBrowserExecutable(browser);
        BrowserLauncher.Creator creator = BrowserLauncher.ForBrowser(browserType)
            .UsingReleaseChannel(BrowserReleaseChannel.Alpha);
        
        return string.IsNullOrEmpty(executablePath)
            ? creator.AutoLocateBrowser().Create()
            : creator.AtLocation(executablePath).Create();
    }

    /// <summary>
    /// Gets the environment variable name for the specified browser's executable.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The environment variable name (e.g., "FIREFOX_EXECUTABLE").</returns>
    private static string GetExecutableEnvironmentVariable(Browser browser)
    {
        return browser switch
        {
            Browser.Firefox => "FIREFOX_EXECUTABLE",
            Browser.Chrome => "CHROME_EXECUTABLE",
            _ => $"{browser.ToString().ToUpperInvariant()}_EXECUTABLE"
        };
    }
}
