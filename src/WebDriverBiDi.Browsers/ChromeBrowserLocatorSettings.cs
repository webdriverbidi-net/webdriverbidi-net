// <copyright file="ChromeBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the locator settings for the Chrome browser, including properties such as the browser name,
/// channel, version, environment variable name, expected executable path, and installer file name.
/// This class also includes methods to retrieve browser download information based on the specified
/// channel or version, and to determine the appropriate download URL for the Chrome browser based
/// on the current platform and architecture. The locator settings are used by the BrowserLocator to
/// locate and download the correct version of Chrome for testing with WebDriver BiDi.
/// </summary>
internal class ChromeBrowserLocatorSettings : BrowserLocatorSettings
{
    private const string ChannelDownloadInfoFileName = "last-known-good-versions-with-downloads.json";
    private const string AllVersionsDownloadInfoFileName = "known-good-versions-with-downloads.json";
    private const string MilestoneDownloadInfoFileName = "latest-versions-per-milestone-with-downloads.json";
    private const string HeadlessShellName = "chrome-headless-shell";

    private readonly ChromeChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Chrome browser.</param>
    /// <param name="locationBehavior">The location behavior for the Chrome browser.</param>
    /// <param name="downloadOptions">The options controlling where Chrome is cached and downloaded from.</param>
    /// <param name="expectedExecutablePath">The expected path to the Chrome executable.</param>
    /// <param name="version">The version of the Chrome browser to locate or download.</param>
    /// <param name="useHeadlessShell">A value indicating whether to download chrome-headless-shell rather than Chrome.</param>
    public ChromeBrowserLocatorSettings(ChromeChannel channel, FileLocationBehavior locationBehavior, BrowserDownloadOptions downloadOptions, string expectedExecutablePath = "", string version = LatestVersionString, bool useHeadlessShell = false)
        : base(downloadOptions)
    {
        if (useHeadlessShell && locationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            throw new ArgumentException("chrome-headless-shell is available only from Chrome for Testing, not as a system installation.", nameof(useHeadlessShell));
        }

        this.channelValue = channel;
        this.UseHeadlessShell = useHeadlessShell;
        this.BrowserName = useHeadlessShell ? HeadlessShellName : "chrome";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = useHeadlessShell ? $"Chrome Headless Shell {channel}" : $"Chrome {channel}";
        this.EnvironmentVariableName = "CHROME_EXECUTABLE";
        this.LocationBehavior = locationBehavior;
        this.InstallerFileName = $"{this.BrowserName}-{this.Channel}.zip";
        this.ExpectedExecutablePath = this.InitializeExpectedExecutablePath(expectedExecutablePath);
        if (this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload)
        {
            this.Version = version;
        }
        else
        {
            this.Version = this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation ? SystemVersionString : LatestVersionString;
        }
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome").
    /// </summary>
    public override string BrowserName { get; }

    /// <summary>
    /// Gets a value indicating whether chrome-headless-shell is located rather than Chrome.
    /// </summary>
    public bool UseHeadlessShell { get; }

    /// <summary>
    /// Gets the name of the driver executable (e.g., "chromedriver" or "chromedriver.exe").
    /// </summary>
    public override string DriverExecutableName => this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? "chromedriver.exe" : "chromedriver";

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "CHROMEDRIVER_EXECUTABLE";

    /// <summary>
    /// Gets the version request under which the resolved chromedriver version is cached, which
    /// is per channel, as each channel's latest chromedriver matches that channel's latest Chrome.
    /// </summary>
    public override string DriverVersionRequest => this.Milestone is int milestone ? $"{MilestoneVersionPrefix}{milestone}" : $"{LatestVersionString}-{this.Channel}";

    /// <summary>
    /// Gets a value indicating whether each chromedriver version drives only the Chrome version of the same number, which it does.
    /// </summary>
    public override bool DriverVersionFollowsBrowser => true;

    /// <summary>
    /// Gets a message explaining that Windows on Arm runs the x64 build of Chrome under emulation, as
    /// Chrome for Testing publishes no Arm build for Windows.
    /// </summary>
    public override string? PlatformSubstitutionNote => this.Platform.OperatingSystem == OperatingSystemFamily.Windows && this.Platform.Architecture == Architecture.Arm64
        ? "Chrome for Testing publishes no Windows Arm64 build; downloading the x64 build, which Windows runs under emulation."
        : null;

    private BrowserPlatform Platform => this.DownloadOptions.ResolvedPlatform;

    private Uri ChannelDownloadInfoUrl => new(this.DownloadOptions.ChromeForTestingEndpoint, ChannelDownloadInfoFileName);

    private Uri AllVersionsDownloadInfoUrl => new(this.DownloadOptions.ChromeForTestingEndpoint, AllVersionsDownloadInfoFileName);

    private Uri MilestoneDownloadInfoUrl => new(this.DownloadOptions.ChromeForTestingEndpoint, MilestoneDownloadInfoFileName);

    /// <summary>
    /// Gets the browser download information for the Chrome browser version or channel specified..
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo(CancellationToken cancellationToken)
    {
        string platformIdentifierString = this.GetRequiredPlatformIdentifierString();
        string json = await DownloadHttpClient.GetStringAsync(this.DownloadOptions, this.GetBinaryDownloadInfoUrl(), cancellationToken).ConfigureAwait(false);

        BinaryVersionInfo binaryVersionInfo;
        if (this.Milestone is int milestone)
        {
            binaryVersionInfo = this.GetMilestoneBinaryVersionInfo(json, milestone);
        }
        else
        {
            binaryVersionInfo = this.IsLatestChannelVersion
                ? this.GetChannelBinaryVersionInfo(json)
                : this.GetSpecificBinaryVersionInfo(json, this.Version);
        }

        BrowserDownloadInfo browserDownloadInfo = new()
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = binaryVersionInfo.Version,
        };

        if (!binaryVersionInfo.Downloads.TryGetValue(this.BrowserName, out List<FileDownloadInfo>? downloadsForPlatform))
        {
            throw new InvalidOperationException($"Failed to find {this.BrowserName} download information for version {binaryVersionInfo.Version}.");
        }

        foreach (FileDownloadInfo download in downloadsForPlatform)
        {
            if (download.Platform.Equals(platformIdentifierString, StringComparison.OrdinalIgnoreCase))
            {
                browserDownloadInfo.DownloadUrl = download.Url;
                return browserDownloadInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find {this.BrowserName} download URL for platform {platformIdentifierString} in version {binaryVersionInfo.Version}.");
    }

    /// <summary>
    /// Gets the chromedriver version that must be used, which is the version of the Chrome it drives.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <returns>The required driver version, or <see langword="null"/> if the latest driver for the channel is to be used.</returns>
    public override string? GetRequiredDriverVersion(string? browserVersion)
    {
        string? pinnedBrowserVersion = this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload && this.BrowserVersionRequest is null ? this.Version : null;
        return browserVersion ?? pinnedBrowserVersion;
    }

    /// <summary>
    /// Gets the driver download information for the chromedriver matching the Chrome version given
    /// by <see cref="GetRequiredDriverVersion"/>, or the latest chromedriver for the channel if none is required.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public override async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo(string? browserVersion, CancellationToken cancellationToken)
    {
        string platformIdentifierString = this.GetRequiredPlatformIdentifierString();
        string? requiredVersion = this.GetRequiredDriverVersion(browserVersion);
        BinaryVersionInfo binaryVersionInfo;
        if (requiredVersion is not null)
        {
            binaryVersionInfo = this.GetSpecificBinaryVersionInfo(await DownloadHttpClient.GetStringAsync(this.DownloadOptions, this.AllVersionsDownloadInfoUrl, cancellationToken).ConfigureAwait(false), requiredVersion);
        }
        else if (this.Milestone is int milestone)
        {
            binaryVersionInfo = this.GetMilestoneBinaryVersionInfo(await DownloadHttpClient.GetStringAsync(this.DownloadOptions, this.MilestoneDownloadInfoUrl, cancellationToken).ConfigureAwait(false), milestone);
        }
        else
        {
            binaryVersionInfo = this.GetChannelBinaryVersionInfo(await DownloadHttpClient.GetStringAsync(this.DownloadOptions, this.ChannelDownloadInfoUrl, cancellationToken).ConfigureAwait(false));
        }

        DriverDownloadInfo driverDownloadInfo = new()
        {
            DriverName = "chromedriver",
            Version = binaryVersionInfo.Version,
            BrowserVersion = binaryVersionInfo.Version,
            InstallerFileName = $"chromedriver-{this.Channel}.zip",
        };

        if (!binaryVersionInfo.Downloads.TryGetValue("chromedriver", out List<FileDownloadInfo>? downloadsForPlatform))
        {
            throw new InvalidOperationException($"Failed to find chromedriver download information for platform.");
        }

        foreach (FileDownloadInfo download in downloadsForPlatform)
        {
            if (download.Platform.Equals(platformIdentifierString, StringComparison.OrdinalIgnoreCase))
            {
                driverDownloadInfo.DownloadUrl = download.Url;
                return driverDownloadInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find chromedriver download URL for platform {platformIdentifierString}.");
    }

    private Uri GetBinaryDownloadInfoUrl()
    {
        if (this.Milestone is not null)
        {
            return this.MilestoneDownloadInfoUrl;
        }

        if (this.IsLatestChannelVersion)
        {
            return this.ChannelDownloadInfoUrl;
        }

        return this.AllVersionsDownloadInfoUrl;
    }

    private BinaryVersionInfo GetChannelBinaryVersionInfo(string json)
    {
        ChromeChannelBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeChannelBinaryDownloadInfo) ??
            throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {this.ChannelDownloadInfoUrl}.");
        string channel = this.channelValue.ToString();
        if (!downloadInfo.Channels.TryGetValue(channel, out BinaryVersionInfo? channelInfo))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome channel {channel}.");
        }

        return channelInfo;
    }

    private BinaryVersionInfo GetMilestoneBinaryVersionInfo(string json, int milestone)
    {
        ChromeMilestoneBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeMilestoneBinaryDownloadInfo)
            ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {this.MilestoneDownloadInfoUrl}.");
        if (!downloadInfo.Milestones.TryGetValue(milestone.ToString(System.Globalization.CultureInfo.InvariantCulture), out BinaryVersionInfo? milestoneInfo))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome milestone {milestone}.");
        }

        return milestoneInfo;
    }

    private BinaryVersionInfo GetSpecificBinaryVersionInfo(string json, string version)
    {
        ChromeAllVersionsBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeAllVersionsBinaryDownloadInfo)
            ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {this.AllVersionsDownloadInfoUrl}.");
        foreach (BinaryVersionInfo versionInfo in downloadInfo.Versions)
        {
            if (versionInfo.Version.Equals(version, StringComparison.OrdinalIgnoreCase))
            {
                return versionInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find download information for Chrome version '{version}'.");
    }

    private string? GetPlatformIdentifierString()
    {
        return (this.Platform.OperatingSystem, this.Platform.Architecture) switch
        {
            (OperatingSystemFamily.MacOS, Architecture.Arm64) => "mac-arm64",
            (OperatingSystemFamily.MacOS, Architecture.X64) => "mac-x64",
            (OperatingSystemFamily.Windows, Architecture.X64 or Architecture.Arm64) => "win64",
            (OperatingSystemFamily.Windows, Architecture.X86) => "win32",
            (OperatingSystemFamily.Linux, Architecture.X64) => "linux64",
            (OperatingSystemFamily.Linux, Architecture.Arm64) => "linux-arm64",
            _ => null,
        };
    }

    private string GetRequiredPlatformIdentifierString()
    {
        return this.GetPlatformIdentifierString() ?? throw new PlatformNotSupportedException($"Chrome for Testing publishes no builds for {this.Platform}.");
    }

    private string InitializeExpectedExecutablePath(string expectedExecutablePath)
    {
        if (this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            return this.GetDefaultSystemInstalledLocation();
        }

        if (this.LocationBehavior == FileLocationBehavior.UseCustomLocation)
        {
            return expectedExecutablePath;
        }

        return this.GetCachedRelativeLocation();
    }

    private string GetCachedRelativeLocation()
    {
        // An unsupported platform fails when download information is requested, before this path is used.
        string directory = $"{this.BrowserName}-{this.GetPlatformIdentifierString()}";
        if (this.UseHeadlessShell)
        {
            return Path.Combine(directory, this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? $"{HeadlessShellName}.exe" : HeadlessShellName);
        }

        return this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => Path.Combine(directory, "Google Chrome for Testing.app", "Contents", "MacOS", "Google Chrome for Testing"),
            OperatingSystemFamily.Windows => Path.Combine(directory, "chrome.exe"),
            _ => Path.Combine(directory, "chrome"),
        };
    }

    private string GetDefaultSystemInstalledLocation()
    {
        if (this.Platform.OperatingSystem == OperatingSystemFamily.MacOS)
        {
            string applicationBundleName = this.channelValue switch
            {
                ChromeChannel.Dev => "Google Chrome Dev",
                ChromeChannel.Beta => "Google Chrome Beta",
                ChromeChannel.Canary => "Google Chrome Canary",
                _ => "Google Chrome",
            };
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                $"{applicationBundleName}.app",
                "Contents",
                "MacOS",
                applicationBundleName);
        }

        if (this.Platform.OperatingSystem == OperatingSystemFamily.Linux)
        {
            // Note carefully, the symlinked executable for Chrome Developer and Canary channels
            // on Linux is the same.
            string executableName = this.channelValue switch
            {
                ChromeChannel.Beta => "google-chrome-beta",
                ChromeChannel.Dev => "google-chrome-unstable",
                ChromeChannel.Canary => "google-chrome-unstable",
                _ => "google-chrome",
            };
            return Path.Combine(
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))!,
                "usr",
                "bin",
                executableName);
        }

        string applicationSubdirectory = this.channelValue switch
        {
            ChromeChannel.Dev => "Chrome Dev",
            ChromeChannel.Beta => "Chrome Beta",
            ChromeChannel.Canary => "Chrome SxS",
            _ => "Chrome",
        };
        string relativePath = Path.Combine("Google", applicationSubdirectory, "Application", "chrome.exe");
        return this.channelValue == ChromeChannel.Beta || this.channelValue == ChromeChannel.Canary
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), relativePath)
            : GetWindowsProgramFilesPath(relativePath);
    }

    /// <summary>
    /// Represents the results of a query to the Chrome for Testing service for all released
    /// versions of Chrome binaries.
    /// </summary>
    internal record ChromeAllVersionsBinaryDownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromeAllVersionsBinaryDownloadInfo"/> class.
        /// </summary>
        [JsonConstructor]
        public ChromeAllVersionsBinaryDownloadInfo()
        {
        }

        /// <summary>
        /// Gets or sets the timestamp of when the Chrome binary download information was last updated.
        /// </summary>
        [JsonPropertyName("timestamp")]
        [JsonInclude]
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of binary version information for all versions of Chrome included
        /// in the Chrome for Testing service response.
        /// </summary>
        [JsonPropertyName("versions")]
        [JsonInclude]
        public List<BinaryVersionInfo> Versions { get; set; } = [];
    }

    /// <summary>
    /// Represents the results of a query to the Chrome for Testing service for the latest versions
    /// of Chrome binaries in a specific channel.
    /// </summary>
    internal record ChromeChannelBinaryDownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromeChannelBinaryDownloadInfo"/> class.
        /// </summary>
        [JsonConstructor]
        public ChromeChannelBinaryDownloadInfo()
        {
        }

        /// <summary>
        /// Gets or sets the timestamp of when the Chrome binary download information was last updated.
        /// </summary>
        [JsonPropertyName("timestamp")]
        [JsonInclude]
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the information about which binaries are available for each of the Chrome distribution channels.
        /// </summary>
        [JsonPropertyName("channels")]
        [JsonInclude]
        public Dictionary<string, BinaryVersionInfo> Channels { get; set; } = [];
    }

    /// <summary>
    /// Represents the results of a query to the Chrome for Testing service for the latest version
    /// of each Chrome milestone.
    /// </summary>
    internal record ChromeMilestoneBinaryDownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromeMilestoneBinaryDownloadInfo"/> class.
        /// </summary>
        [JsonConstructor]
        public ChromeMilestoneBinaryDownloadInfo()
        {
        }

        /// <summary>
        /// Gets or sets the information about the latest binaries of each milestone, keyed by milestone.
        /// </summary>
        [JsonPropertyName("milestones")]
        [JsonInclude]
        public Dictionary<string, BinaryVersionInfo> Milestones { get; set; } = [];
    }

    /// <summary>
    /// Represents the information about a specific Chrome binary, as identified by its version and revision.
    /// </summary>
    internal record BinaryVersionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryVersionInfo"/> class.
        /// </summary>
        [JsonConstructor]
        public BinaryVersionInfo()
        {
        }

        /// <summary>
        /// Gets or sets the name of the Chrome distribution channel on which the binary is available.
        /// </summary>
        [JsonPropertyName("channel")]
        [JsonInclude]
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of this Chrome binary.
        /// </summary>
        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the revision identifier for this version of the Chrome binary.
        /// </summary>
        [JsonPropertyName("revision")]
        [JsonInclude]
        public string Revision { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of file download information for this version of Chrome
        /// for each of the binaries included in the Chrome for Testing service response.
        /// </summary>
        [JsonPropertyName("downloads")]
        [JsonInclude]
        public Dictionary<string, List<FileDownloadInfo>> Downloads { get; set; } = [];
    }

    /// <summary>
    /// Represents information about a file download of a Chrome file for a specific platform,
    /// including the platform identifier and the download URL.
    /// </summary>
    internal record FileDownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadInfo"/> class with the specified platform and download URL.
        /// </summary>
        /// <param name="platform">The platform identifier.</param>
        /// <param name="url">The download URL.</param>
        [JsonConstructor]
        public FileDownloadInfo(string platform, string url)
        {
            this.Platform = platform;
            this.Url = url;
        }

        /// <summary>
        /// Gets or sets the platform identifier for this download (e.g., "win64", "mac-arm64", "linux64").
        /// </summary>
        [JsonPropertyName("platform")]
        [JsonInclude]
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the direct download URL for this Chrome file for the specified platform.
        /// </summary>
        [JsonPropertyName("url")]
        [JsonInclude]
        public string Url { get; set; }
    }
}

#pragma warning disable SA1402 // File may only contain a single type
/// <summary>
/// A source generation context for JSON serialization of browser locator cache information.
/// This is used to enable serialization and deserialization of the browser cache information
/// when used in AOT environments.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ChromeBrowserLocatorSettings.ChromeAllVersionsBinaryDownloadInfo))]
[JsonSerializable(typeof(ChromeBrowserLocatorSettings.ChromeChannelBinaryDownloadInfo))]
[JsonSerializable(typeof(ChromeBrowserLocatorSettings.ChromeMilestoneBinaryDownloadInfo))]
[JsonSerializable(typeof(Dictionary<string, List<ChromeBrowserLocatorSettings.FileDownloadInfo>>))]
[JsonSerializable(typeof(Dictionary<string, ChromeBrowserLocatorSettings.BinaryVersionInfo>))]
internal partial class ChromeBrowserLocatorSettingsJsonSerializerContext : JsonSerializerContext
{
}
