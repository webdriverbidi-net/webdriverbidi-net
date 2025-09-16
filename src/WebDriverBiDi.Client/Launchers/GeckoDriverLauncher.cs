// <copyright file="GeckoDriverLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Runtime.InteropServices;

/// <summary>
/// Object for launching a Firefox browser to connect to using a WebDriverBiDi session
/// using a local instance of the geckodriver browser driver executable.
/// </summary>
public class GeckoDriverLauncher : ClassicDriverExecutableBrowserLauncher
{
    private const string DefaultFirefoxLauncherFileName = "geckodriver";

    /// <summary>
    /// Initializes a new instance of the <see cref="GeckoDriverLauncher" /> class.
    /// </summary>
    /// <param name="launcherPath">The path of the directory containing the launcher executable.</param>
    /// <param name="browserExecutableLocation">
    /// The location of the Firefox browser executable the launcher will launch.
    /// Defaults to an empty string, indicating to launch Firefox from its default location.
    /// </param>
    public GeckoDriverLauncher(string launcherPath, string browserExecutableLocation = "")
        : this(launcherPath, FirefoxLauncherFileName(), 0, browserExecutableLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeckoDriverLauncher" /> class.
    /// </summary>
    /// <param name="launcherPath">The path of the directory containing the launcher executable.</param>
    /// <param name="executableName">The name of the launcher executable.</param>
    /// <param name="browserExecutableLocation">
    /// The location of the Firefox browser executable the launcher will launch.
    /// Defaults to an empty string, indicating to launch Firefox from its default location.
    /// </param>
    public GeckoDriverLauncher(string launcherPath, string executableName, string browserExecutableLocation = "")
        : this(launcherPath, executableName, 0, browserExecutableLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeckoDriverLauncher" /> class.
    /// </summary>
    /// <param name="launcherPath">The path of the directory containing the launcher executable.</param>
    /// <param name="executableName">The name of the launcher executable.</param>
    /// <param name="port">The port on which the launcher will listen.</param>
    /// <param name="browserExecutableLocation">
    /// The location of the Firefox browser executable the launcher will launch.
    /// Defaults to an empty string, indicating to launch Firefox from its default location.
    /// </param>
    public GeckoDriverLauncher(string launcherPath, string executableName, int port, string browserExecutableLocation = "")
        : base(launcherPath, executableName, port, browserExecutableLocation)
    {
    }

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
        Dictionary<string, object> firefoxOptions = new();
        if (!string.IsNullOrEmpty(this.BrowserExecutableLocation))
        {
            firefoxOptions["binary"] = this.BrowserExecutableLocation;
        }

        firefoxOptions["log"] = new Dictionary<string, object>()
        {
            { "level", "error" },
        };

        // TODO: Create a more fully-featured generation of capabilities.
        Dictionary<string, object> capabilities = new()
        {
            ["browserName"] = "firefox",
            ["webSocketUrl"] = true,
            ["moz:firefoxOptions"] = firefoxOptions,
        };

        return capabilities;
    }

    /// <summary>
    /// Returns the Firefox driver filename for the currently running platform.
    /// </summary>
    /// <returns>The file name of the Firefox driver service executable.</returns>
    private static string FirefoxLauncherFileName()
    {
        string fileName = DefaultFirefoxLauncherFileName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName += ".exe";
        }

        return fileName;
    }
}
