// <copyright file="GeckoDriverLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Object for launching a Firefox browser to connect to using a WebDriverBiDi session
/// using a local instance of the geckodriver browser driver executable.
/// </summary>
public class GeckoDriverLauncher : ClassicDriverExecutableBrowserLauncher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeckoDriverLauncher" /> class using Firefox browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="settings">The Firefox browser locator settings to use for locating the browser and driver executables.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    /// <exception cref="ArgumentException">Thrown when settings.IncludeDriver is false.</exception>
    internal GeckoDriverLauncher(FirefoxBrowserLocatorSettings settings)
        : base(settings, 0)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public override bool IsBrowserCloseAllowed => false;

    /// <summary>
    /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
    /// </summary>
    protected override TimeSpan TerminationTimeout => TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
    /// it gracefully before forcing a termination.
    /// </summary>
    protected override bool HasShutdownApi => false;

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected override Dictionary<string, object> CreateBrowserLaunchCapabilities()
    {
        Dictionary<string, object> firefoxOptions = [];
        if (!string.IsNullOrEmpty(this.BrowserExecutableLocation))
        {
            firefoxOptions["binary"] = this.BrowserExecutableLocation;
        }

        firefoxOptions["log"] = new Dictionary<string, object>()
        {
            { "level", "error" },
        };

        if (this.IsBrowserHeadless)
        {
            firefoxOptions["args"] = new List<string>() { "--headless" };
        }

        // CONSIDER: This is a very naive and simple set of capabilities.
        // A future implementation could create a more fully-featured
        // generation of capabilities.
        Dictionary<string, object> capabilities = new()
        {
            ["browserName"] = "firefox",
            ["webSocketUrl"] = true,
            ["moz:firefoxOptions"] = firefoxOptions,
        };

        return capabilities;
    }
}
