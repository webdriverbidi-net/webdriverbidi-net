// <copyright file="DriverConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Provides configuration options for WebDriver Classic driver executables (chromedriver, geckodriver).
/// </summary>
/// <remarks>
/// TODO: This class is currently internal and not used. When driver configuration is needed,
/// make this class public and add a static Configure() factory method to BrowserLauncherBuilder
/// that accepts a DriverConfiguration instance:
/// <code>
/// var driverConfig = DriverConfiguration.Configure()
///     .UseSystemPath()
///     .WithVersion("120.0.0");
/// var launcher = BrowserLauncher.Configure(Browser.Chrome)
///     .LaunchUsingDriver(driverConfig)
///     .Build();
/// </code>
/// </remarks>
internal class DriverConfiguration
{
    private readonly BrowserLocatorSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriverConfiguration"/> class.
    /// </summary>
    /// <param name="settings">The browser locator settings that will be configured.</param>
    internal DriverConfiguration(BrowserLocatorSettings settings)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Specifies to locate the driver executable on the system PATH.
    /// </summary>
    /// <returns>The current configuration instance for method chaining.</returns>
    public DriverConfiguration UseSystemPath()
    {
        this.settings.DriverLocationBehavior = FileLocationBehavior.UseSystemInstallLocation;
        return this;
    }

    /// <summary>
    /// Specifies the custom location of the driver executable.
    /// </summary>
    /// <param name="path">The full path to the driver executable.</param>
    /// <returns>The current configuration instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
    public DriverConfiguration AtLocation(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Driver location path cannot be null or empty.", nameof(path));
        }

        this.settings.DriverLocationBehavior = FileLocationBehavior.UseCustomLocation;
        this.settings.DriverExecutableLocation = path;
        return this;
    }

    /// <summary>
    /// Specifies to automatically locate the driver, downloading it if necessary.
    /// </summary>
    /// <returns>The current configuration instance for method chaining.</returns>
    public DriverConfiguration AtAutomaticallyDownloadedLocation()
    {
        this.settings.DriverLocationBehavior = FileLocationBehavior.AutoLocateAndDownload;

        // TODO: Support custom cache directory for driver
        // Currently BrowserLocatorSettings doesn't expose this separately for driver vs browser
        return this;
    }

    /// <summary>
    /// Specifies a particular version of the driver to use.
    /// Only applicable when using <see cref="AtAutomaticallyDownloadedLocation"/>.
    /// </summary>
    /// <param name="version">The driver version string.</param>
    /// <returns>The current configuration instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when version is null or empty.</exception>
    public DriverConfiguration WithVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Driver version cannot be null or empty.", nameof(version));
        }

        this.settings.DriverVersion = version;
        return this;
    }

    // TODO: Add these methods in future when we support driver arguments
    // public DriverConfiguration WithArguments(params string[] args)
    // public DriverConfiguration WithVerboseLogging()
    // public DriverConfiguration OnPort(int port)
}
