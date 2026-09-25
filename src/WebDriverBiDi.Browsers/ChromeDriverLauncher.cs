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
    /// <param name="settings">The browser locator settings to use for locating the browser and driver executables.</param>
    internal ChromeDriverLauncher(BrowserLocatorSettings settings)
        : base(settings, 0)
    {
    }

    /// <inheritdoc/>
    internal override IReadOnlyCollection<string> LaunchCapabilityNames => ["browserName", "webSocketUrl", this.OptionsCapabilityName];

    /// <summary>
    /// Gets the value of the browserName capability.
    /// </summary>
    private protected virtual string BrowserNameCapabilityValue => "chrome";

    /// <summary>
    /// Gets the name of the capability that carries the browser's launch options.
    /// </summary>
    private protected virtual string OptionsCapabilityName => "goog:chromeOptions";

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected override Dictionary<string, object?> CreateBrowserLaunchCapabilities()
    {
        Dictionary<string, object> chromeOptions = new() { ["binary"] = this.BrowserExecutableLocation };

        List<string> defaultArguments = this.IsBrowserHeadless ? ["--disable-dev-shm-usage"] : [];
        defaultArguments.AddRange(ChromeLauncher.SandboxArguments);

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
        Dictionary<string, object?> capabilities = new()
        {
            ["browserName"] = this.BrowserNameCapabilityValue,
            ["webSocketUrl"] = true,
            [this.OptionsCapabilityName] = chromeOptions,
        };

        return capabilities;
    }
}
