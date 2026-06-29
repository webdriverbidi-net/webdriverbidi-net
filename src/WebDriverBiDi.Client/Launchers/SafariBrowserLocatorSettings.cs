// <copyright file="SafariBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

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
    private readonly SafariChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafariBrowserLocatorSettings"/> class.
    /// </summary>
    public SafariBrowserLocatorSettings()
        : this(SafariChannel.Stable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafariBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Safari browser.</param>
    internal SafariBrowserLocatorSettings(SafariChannel channel)
    {
        this.channelValue = channel;
        this.BrowserName = "safari";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = $"Safari{(channel == SafariChannel.TechnologyPreview ? "Technology Preview" : string.Empty)}";
        this.EnvironmentVariableName = "SAFARI_EXECUTABLE";
        this.LocationBehavior = FileLocationBehavior.UseSystemInstallLocation;
        this.InstallerFileName = string.Empty;
        this.ExpectedExecutablePath = this.GetDefaultSystemInstalledLocation();
        this.IncludeDriver = true;
        this.DriverLocationBehavior = FileLocationBehavior.UseSystemInstallLocation;
        this.DriverExecutableLocation = this.GetDriverLocation();
        this.Version = SystemVersionString;
        this.InitializeExtractors();
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

    /// <summary>
    /// Gets the browser download information for the Safari browser version or channel specified.
    /// For Safari, the browser cannot be downloaded independently, so this hard-codes no download
    /// information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo()
    {
        BrowserDownloadInfo downloadInfo = new()
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = this.Version,
        };

        return downloadInfo;
    }

    /// <summary>
    /// Gets the driver download information for the safaridriver that is compatible with the Safari browser.
    /// For Safari, this is hard-coded, as the user can only use the system-installed version of Safari and
    /// its accompanying driver.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public override async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo()
    {
        DriverDownloadInfo driverDownloadInfo = new()
        {
            DriverName = "safaridriver",
            Version = string.Empty,
            BrowserVersion = this.Version,
            DownloadUrl = string.Empty,
            InstallerFileName = string.Empty,
        };

        return driverDownloadInfo;
    }

    private void InitializeExtractors()
    {
        this.BrowserExtractor = new DiskImageFileExtractor();
        this.DriverExtractor = new DiskImageFileExtractor();
    }

    private string GetDriverLocation()
    {
        return this.channelValue == SafariChannel.Stable ? Path.Combine("usr", "bin") : this.GetInstallLocation();
    }

    private string GetDefaultSystemInstalledLocation()
    {
            return Path.Combine(this.GetInstallLocation(), this.GetAppBundleName());
    }

    private string GetAppBundleName()
    {
        return this.channelValue switch
        {
            SafariChannel.Stable => "Safari",
            SafariChannel.TechnologyPreview => "Safari Technology Preview",
            _ => "Safari",
        };
    }

    private string GetInstallLocation()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            $"{this.GetAppBundleName()}.app",
            "Contents",
            "MacOS");
    }
}
