// <copyright file="ChromeDriverLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Object for launching a Chrome browser to connect to using a WebDriverBiDi session
/// using a local instance of the chromedriver browser driver executable.
/// </summary>
public class ChromeDriverLauncher : ClassicDriverExecutableBrowserLauncher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriverLauncher" /> class using Chrome browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="settings">The Chrome browser locator settings to use for locating the browser and driver executables.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    /// <exception cref="ArgumentException">Thrown when settings.IncludeDriver is false.</exception>
    internal ChromeDriverLauncher(ChromeBrowserLocatorSettings settings)
        : base(settings, 0)
    {
    }

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected override Dictionary<string, object> CreateBrowserLaunchCapabilities()
    {
        Dictionary<string, object> chromeOptions = [];
        if (!string.IsNullOrEmpty(this.BrowserExecutableLocation))
        {
            chromeOptions["binary"] = this.BrowserExecutableLocation;
        }

        List<string> defaultArguments = this.IsBrowserHeadless ? ["--disable-dev-shm-usage"] : [];
        if (ChromeLauncher.IsSandboxUnavailable)
        {
            defaultArguments.Add("--no-sandbox");
        }

        List<string> chromeCommandLineArgs = [.. this.LaunchSettings.FilterDefaultArguments(defaultArguments)];

        // chrome-headless-shell is always headless.
        if (this.IsBrowserHeadless && !this.LaunchSettings.UseHeadlessShell)
        {
            chromeCommandLineArgs.Add("--headless=new");
            chromeCommandLineArgs.Add("--disable-gpu");
        }

        if (this.LaunchSettings.UserDataDirectory is not null)
        {
            chromeCommandLineArgs.Add($"--user-data-dir={this.LaunchSettings.UserDataDirectory}");
        }

        chromeCommandLineArgs.AddRange(this.LaunchSettings.Arguments);
        if (chromeCommandLineArgs.Count > 0)
        {
            chromeOptions["args"] = chromeCommandLineArgs;
        }

        // CONSIDER: This is a very naive and simple set of capabilities.
        // A future implementation could create a more fully-featured
        // generation of capabilities.
        Dictionary<string, object> capabilities = new()
        {
            ["browserName"] = "chrome",
            ["webSocketUrl"] = true,
            ["goog:chromeOptions"] = chromeOptions,
        };

        return capabilities;
    }
}
