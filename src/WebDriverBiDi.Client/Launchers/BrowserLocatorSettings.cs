// <copyright file="BrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Base class for browser locator settings, which define the properties and methods
/// needed to locate and download a specific browser for testing.
/// </summary>
public abstract class BrowserLocatorSettings
{
    /// <summary>
    /// Marker value for the version property indicating to use the latest published
    /// version of the browser for the specified channel.
    /// </summary>
    public const string LatestVersionString = "latest";

    /// <summary>
    /// Marker value for the version property indicating to use the system-installed version of the browser.
    /// </summary>
    public const string SystemVersionString = "system";

    /// <summary>
    /// Gets the base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    protected static readonly string BaseCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome", "firefox").
    /// </summary>
    public abstract string BrowserName { get; }

    /// <summary>
    /// Gets or sets the channel of the browser (e.g., "stable", "beta", "dev", "nightly").
    /// </summary>
    public string Channel { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the browser, which is used for logging and user-facing messages.
    /// </summary>
    public string BrowserDisplayName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    public string Version { get; protected set; } = LatestVersionString;

    /// <summary>
    /// Gets or sets the name of the environment variable that can be used to override the browser executable path.
    /// </summary>
    public string EnvironmentVariableName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected path to the browser executable relative to the installation directory.
    /// </summary>
    public string ExpectedExecutablePath { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the installer file.
    /// </summary>
    public string InstallerFileName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the version specified in the locator settings
    /// is the most recently published version for the channel.
    /// </summary>
    public bool IsLatestChannelVersion => this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload && this.Version == LatestVersionString;

    /// <summary>
    /// Gets or sets the location behavior for the browser, which determines how the browser is located and downloaded.
    /// </summary>
    public FileLocationBehavior LocationBehavior { get; set; } = FileLocationBehavior.AutoLocateAndDownload;

    /// <summary>
    /// Gets or sets a value indicating whether to include driver executable location when locating the browser.
    /// When true, the browser locator will also locate and download the matching driver executable.
    /// </summary>
    public bool IncludeDriver { get; set; } = false;

    /// <summary>
    /// Gets or sets the location behavior for the driver executable.
    /// Only used when <see cref="IncludeDriver"/> is true.
    /// </summary>
    public FileLocationBehavior DriverLocationBehavior { get; set; } = FileLocationBehavior.AutoLocateAndDownload;

    /// <summary>
    /// Gets the expected path to the driver executable.
    /// Only used when <see cref="IncludeDriver"/> is true and <see cref="DriverLocationBehavior"/> is <see cref="FileLocationBehavior.UseCustomLocation"/>.
    /// </summary>
    public string DriverExecutableLocation { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the driver to use. If null, the driver version will be determined
    /// based on the browser version or channel. Only used when <see cref="IncludeDriver"/> is true.
    /// </summary>
    public string? DriverVersion { get; set; }

    /// <summary>
    /// Gets or sets the file extractor used to extract the browser from the installer.
    /// Defaults to a ZipFileBrowserExtractor, but can be overridden for browsers that use
    /// different installer formats (e.g., DMG, self-extracting EXE, tarball).
    /// </summary>
    public FileExtractor BrowserExtractor { get; protected set; } = new ZipFileExtractor();

    /// <summary>
    /// Gets or sets the file extractor used to extract the driver from the installer.
    /// Defaults to a ZipFileBrowserExtractor, but can be overridden for browsers that use
    /// different installer formats (e.g., DMG, self-extracting EXE, tarball).
    /// </summary>
    public FileExtractor DriverExtractor { get; protected set; } = new ZipFileExtractor();

    /// <summary>
    /// Gets the name of the driver executable (e.g., "chromedriver", "geckodriver", "chromedriver.exe").
    /// </summary>
    public abstract string DriverExecutableName { get; }

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public abstract string DriverEnvironmentVariableName { get; }

    /// <summary>
    /// Gets the description of the browser location behavior, which is used for logging and user-facing messages.
    /// </summary>
    public virtual string BrowserLocationBehaviorDescription
    {
        get
        {
            return this.LocationBehavior switch
            {
                FileLocationBehavior.UseSystemInstallLocation => $"system-installed {this.BrowserDisplayName}",
                FileLocationBehavior.UseCustomLocation => $"custom {this.BrowserDisplayName}",
                FileLocationBehavior.AutoLocateAndDownload => $"cached {this.BrowserDisplayName}",
                _ => "unknown location behavior",
            };
        }
    }

    /// <summary>
    /// Gets the browser download information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public abstract Task<BrowserDownloadInfo> GetBrowserDownloadInfo();

    /// <summary>
    /// Gets the driver download information for a driver that is compatible with this browser.
    /// Uses the <see cref="DriverVersion"/> property to determine which driver version to download,
    /// or determines it automatically based on the browser version if <see cref="DriverVersion"/> is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public abstract Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo();
}
