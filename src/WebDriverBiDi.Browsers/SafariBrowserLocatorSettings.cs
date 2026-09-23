// <copyright file="SafariBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Defines the locator settings for the Safari browser, including properties such as the browser name,
/// channel, version, environment variable name, expected executable path, and installer file name.
/// This class does not provide includes methods to allow browser download information based on the
/// specified channel or version, as Safari is only supported when used with the executables installed
/// by the system package manager. The locator settings are used by the BrowserLocator to
/// locate the binaries of Safari specified for testing with WebDriver BiDi.
/// </summary>
internal class SafariBrowserLocatorSettings : BrowserLocatorSettings
{
    private const string ApplicationsDirectory = "/Applications";

    private readonly SafariChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafariBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Safari browser.</param>
    /// <param name="downloadOptions">The download options, or <see langword="null"/> for the defaults; Safari is never downloaded.</param>
    /// <param name="customPath">The path to the Safari executable, or <see langword="null"/> for the system installation.</param>
    internal SafariBrowserLocatorSettings(SafariChannel channel, BrowserDownloadOptions? downloadOptions = null, string? customPath = null)
        : base(downloadOptions ?? new BrowserDownloadOptions())
    {
        this.channelValue = channel;
        this.BrowserName = "safari";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = channel == SafariChannel.TechnologyPreview ? "Safari Technology Preview" : "Safari";
        this.EnvironmentVariableName = "SAFARI_EXECUTABLE";
        this.LocationBehavior = customPath is null ? FileLocationBehavior.UseSystemInstallLocation : FileLocationBehavior.UseCustomLocation;
        this.InstallerFileName = string.Empty;
        this.ExpectedExecutablePath = customPath ?? this.GetDefaultSystemInstalledLocation();
        this.IncludeDriver = true;
        this.DriverLocationBehavior = FileLocationBehavior.UseCustomLocation;
        this.DriverExecutableLocation = this.GetDriverLocation();
        this.Version = SystemVersionString;
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "safari").
    /// </summary>
    public override string BrowserName { get; } = "safari";

    /// <summary>
    /// Gets the name of the driver executable (e.g., "safaridriver").
    /// </summary>
    public override string DriverExecutableName => "safaridriver";

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "SAFARIDRIVER_EXECUTABLE";

    /// <summary>
    /// Gets a value indicating whether the Safari browser is the technology preview edition.
    /// </summary>
    public bool IsTechnologyPreview => this.channelValue == SafariChannel.TechnologyPreview;

    private string GetDriverLocation()
    {
        return this.channelValue == SafariChannel.Stable ? "/usr/bin/safaridriver" : $"{this.GetInstallLocation()}/safaridriver";
    }

    private string GetDefaultSystemInstalledLocation()
    {
        return $"{this.GetInstallLocation()}/{this.GetAppBundleName()}";
    }

    private string GetAppBundleName()
    {
        return this.channelValue == SafariChannel.TechnologyPreview ? "Safari Technology Preview" : "Safari";
    }

    private string GetInstallLocation()
    {
        // Safari exists only on macOS, so these are macOS paths wherever the settings are created.
        return $"{ApplicationsDirectory}/{this.GetAppBundleName()}.app/Contents/MacOS";
    }
}
