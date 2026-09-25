// <copyright file="BrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Base class for browser locator settings, which define the properties and methods
/// needed to locate and download a specific browser for testing.
/// </summary>
internal abstract class BrowserLocatorSettings
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
    /// The prefix of the version property's value when the most recent release of a milestone is requested.
    /// </summary>
    public const string MilestoneVersionPrefix = "milestone-";

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="downloadOptions">The options controlling where the browser and driver are cached and downloaded from.</param>
    protected BrowserLocatorSettings(BrowserDownloadOptions downloadOptions)
    {
        this.DownloadOptions = downloadOptions;
    }

    /// <summary>
    /// Gets the options controlling where the browser and driver are cached and downloaded from.
    /// </summary>
    public BrowserDownloadOptions DownloadOptions { get; }

    /// <summary>
    /// Gets the directory for the browser and driver cache.
    /// </summary>
    public string CacheDirectory => this.DownloadOptions.CacheDirectory;

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
    /// Gets the requested milestone, or <see langword="null"/> if a milestone was not requested.
    /// </summary>
    public int? Milestone => this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload
        && this.Version.StartsWith(MilestoneVersionPrefix, StringComparison.Ordinal)
        && int.TryParse(this.Version.Substring(MilestoneVersionPrefix.Length), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int milestone)
        ? milestone
        : null;

    /// <summary>
    /// Gets the version request whose resolved version is cached and periodically rechecked ("latest"
    /// or a milestone), or <see langword="null"/> when a specific version is requested or the browser
    /// is not downloaded.
    /// </summary>
    public string? BrowserVersionRequest => this.IsLatestChannelVersion || this.Milestone is not null ? this.Version : null;

    /// <summary>
    /// Gets a message explaining a substitute download for the platform, or <see langword="null"/> if
    /// the download is built for the platform.
    /// </summary>
    public virtual string? PlatformSubstitutionNote => null;

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
    /// Gets or sets the expected path to the driver executable.
    /// Only used when <see cref="IncludeDriver"/> is true and <see cref="DriverLocationBehavior"/> is <see cref="FileLocationBehavior.UseCustomLocation"/>.
    /// </summary>
    public string DriverExecutableLocation { get; internal set; } = string.Empty;

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
    /// Gets the name of the driver (e.g., "chromedriver"), which names its cache directory.
    /// </summary>
    public string DriverName => Path.GetFileNameWithoutExtension(this.DriverExecutableName);

    /// <summary>
    /// Gets the version request under which the resolved driver version is cached when
    /// <see cref="GetRequiredDriverVersion"/> returns <see langword="null"/>.
    /// </summary>
    public virtual string DriverVersionRequest => LatestVersionString;

    /// <summary>
    /// Gets a value indicating whether each driver version drives only the browser version of the same number.
    /// </summary>
    public virtual bool DriverVersionFollowsBrowser => false;

    /// <summary>
    /// Gets a value indicating whether, when no driver of the required version is published, the closest compatible
    /// version is used (see <see cref="CompatibleVersion"/>).
    /// </summary>
    public virtual bool AcceptsCompatibleDriverVersion => false;

    /// <summary>
    /// Gets the description of the browser location behavior, which is used for logging and user-facing messages.
    /// </summary>
    public virtual string BrowserLocationBehaviorDescription
    {
        get
        {
            string location = this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation
                ? "system-installed"
                : this.LocationBehavior == FileLocationBehavior.UseCustomLocation ? "custom" : "cached";
            return $"{location} {this.BrowserDisplayName}";
        }
    }

    /// <summary>
    /// Gets the browser download information.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    /// <exception cref="NotSupportedException">Thrown for a browser that is never downloaded.</exception>
    public virtual Task<BrowserDownloadInfo> GetBrowserDownloadInfo(CancellationToken cancellationToken)
    {
        throw new NotSupportedException($"{this.BrowserDisplayName} is not downloaded.");
    }

    /// <summary>
    /// Gets the SHA-256 hash the browser's publisher lists for a download.
    /// </summary>
    /// <param name="downloadInfo">The download.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The hash in hexadecimal, or <see langword="null"/> if the publisher lists none.</returns>
    /// <exception cref="DownloadVerificationException">Thrown when the publisher lists hashes, but none for the download.</exception>
    public virtual Task<string?> GetBrowserSha256Async(BrowserDownloadInfo downloadInfo, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Reads the version of a browser that is not downloaded, when its driver must match it.
    /// </summary>
    /// <param name="executablePath">The path of the browser executable.</param>
    /// <param name="cancellationToken">A token that cancels reading the version.</param>
    /// <returns>The version, or <see langword="null"/> if it is not read or cannot be.</returns>
    public virtual Task<string?> GetInstalledBrowserVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Gets the driver version that must be used, if it is known without a network request.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <returns>The required driver version, or <see langword="null"/> if the driver version must be resolved.</returns>
    public virtual string? GetRequiredDriverVersion(string? browserVersion)
    {
        return null;
    }

    /// <summary>
    /// Gets the driver download information for a driver that is compatible with this browser.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    /// <exception cref="NotSupportedException">Thrown for a browser whose driver is never downloaded.</exception>
    public virtual Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo(string? browserVersion, CancellationToken cancellationToken)
    {
        throw new NotSupportedException($"The driver for {this.BrowserDisplayName} is not downloaded.");
    }

    /// <summary>
    /// Gets the path, within the 64-bit or else the 32-bit Program Files directory, at which a
    /// Windows application is installed.
    /// </summary>
    /// <param name="relativePath">The path of the executable relative to the Program Files directory.</param>
    /// <returns>The path of the installed executable, or its 64-bit path if it is installed in neither.</returns>
    [ExcludeFromCodeCoverage] // Depends on which of the directories the machine has the application in.
    protected static string GetWindowsProgramFilesPath(string relativePath)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), relativePath);
        string x86Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), relativePath);
        return !File.Exists(path) && File.Exists(x86Path) ? x86Path : path;
    }
}
