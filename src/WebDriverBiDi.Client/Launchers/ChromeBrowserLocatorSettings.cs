// <copyright file="ChromeBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

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
    private const string ChannelDownloadInfoUrl = "https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json";
    private const string AllVersionsDownloadInfoUrl = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";

    private readonly ChromeChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Chrome browser.</param>
    /// <param name="locationBehavior">The location behavior for the Chrome browser.</param>
    /// <param name="expectedExecutablePath">The expected path to the Chrome executable.</param>
    /// <param name="version">The version of the Chrome browser to locate or download.</param>
    public ChromeBrowserLocatorSettings(ChromeChannel channel, FileLocationBehavior locationBehavior, string expectedExecutablePath = "", string version = LatestVersionString)
    {
        this.channelValue = channel;
        this.BrowserName = "chrome";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = $"Chrome {channel}";
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
    public override string BrowserName { get; } = "chrome";

    /// <summary>
    /// Gets the name of the driver executable (e.g., "chromedriver" or "chromedriver.exe").
    /// </summary>
    public override string DriverExecutableName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "chromedriver.exe" : "chromedriver";

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "CHROMEDRIVER_EXECUTABLE";

    /// <summary>
    /// Gets the browser download information for the Chrome browser version or channel specified..
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo()
    {
        using HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        string chromeBinaryDownloadInfoUrl = this.GetBinaryDownloadInfoUrl();
        HttpResponseMessage response = await httpClient.GetAsync(chromeBinaryDownloadInfoUrl);
        string json = await response.Content.ReadAsStringAsync();

        BinaryVersionInfo binaryVersionInfo = this.IsLatestChannelVersion
            ? this.GetChannelBinaryVersionInfo(json)
            : this.GetSpecificBinaryVersionInfo(json);

        BrowserDownloadInfo browserDownloadInfo = new()
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = binaryVersionInfo.Version,
        };

        string platformIdentifierString = this.GetPlatformIdentifierString();
        if (!binaryVersionInfo.Downloads.TryGetValue(this.BrowserName, out List<FileDownloadInfo>? downloadsForPlatform))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome platform.");
        }

        foreach (FileDownloadInfo download in downloadsForPlatform)
        {
            if (download.Platform.Equals(platformIdentifierString, StringComparison.OrdinalIgnoreCase))
            {
                browserDownloadInfo.DownloadUrl = download.Url;
                return browserDownloadInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find download URL for Chrome platform {platformIdentifierString}.");
    }

    /// <summary>
    /// Gets the driver download information for the chromedriver that matches the Chrome browser version or channel specified.
    /// Uses the <see cref="BrowserLocatorSettings.DriverVersion"/> property to determine which driver version to download,
    /// or determines it automatically based on the browser version if <see cref="BrowserLocatorSettings.DriverVersion"/> is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public override async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo()
    {
        using HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        BinaryVersionInfo binaryVersionInfo;

        // Determine which driver version to download
        if (!string.IsNullOrEmpty(this.DriverVersion))
        {
            // Explicit driver version specified
            string driverBinaryUrl = this.DriverVersion == LatestVersionString ? ChannelDownloadInfoUrl : AllVersionsDownloadInfoUrl;
            HttpResponseMessage response = await httpClient.GetAsync(driverBinaryUrl);
            string json = await response.Content.ReadAsStringAsync();

            if (this.DriverVersion == LatestVersionString)
            {
                // Get latest driver for the browser's channel
                binaryVersionInfo = this.GetChannelBinaryVersionInfo(json);
            }
            else
            {
                // Specific driver version requested
                ChromeAllVersionsBinaryDownloadInfo? downloadInfo =
                    JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeAllVersionsBinaryDownloadInfo)
                    ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {AllVersionsDownloadInfoUrl}.");

                binaryVersionInfo = downloadInfo.Versions.FirstOrDefault(v => v.Version.Equals(this.DriverVersion, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Failed to find download information for chromedriver version {this.DriverVersion}.");
            }
        }
        else if (this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload)
        {
            // Browser is being auto-downloaded, match driver to browser version
            string chromeBinaryDownloadInfoUrl = this.GetBinaryDownloadInfoUrl();
            HttpResponseMessage response = await httpClient.GetAsync(chromeBinaryDownloadInfoUrl);
            string json = await response.Content.ReadAsStringAsync();

            binaryVersionInfo = this.IsLatestChannelVersion
                ? this.GetChannelBinaryVersionInfo(json)
                : this.GetSpecificBinaryVersionInfo(json);
        }
        else
        {
            // Browser is system/custom - download latest driver for the browser's channel
            HttpResponseMessage response = await httpClient.GetAsync(ChannelDownloadInfoUrl);
            string json = await response.Content.ReadAsStringAsync();
            binaryVersionInfo = this.GetChannelBinaryVersionInfo(json);
        }

        DriverDownloadInfo driverDownloadInfo = new()
        {
            DriverName = "chromedriver",
            Version = binaryVersionInfo.Version,
            BrowserVersion = binaryVersionInfo.Version,
            InstallerFileName = $"chromedriver-{this.Channel}.zip",
        };

        string platformIdentifierString = this.GetPlatformIdentifierString();
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

    private string GetBinaryDownloadInfoUrl()
    {
        if (this.IsLatestChannelVersion)
        {
            return ChannelDownloadInfoUrl;
        }

        return AllVersionsDownloadInfoUrl;
    }

    private BinaryVersionInfo GetChannelBinaryVersionInfo(string json)
    {
        ChromeChannelBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeChannelBinaryDownloadInfo) ??
            throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {ChannelDownloadInfoUrl}.");
        string channel = this.channelValue.ToString();
        if (!downloadInfo.Channels.TryGetValue(channel, out BinaryVersionInfo? channelInfo))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome channel {channel}.");
        }

        return channelInfo;
    }

    private BinaryVersionInfo GetSpecificBinaryVersionInfo(string json)
    {
        ChromeAllVersionsBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonSerializerContext.Default.ChromeAllVersionsBinaryDownloadInfo)
            ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {ChannelDownloadInfoUrl}.");
        foreach (BinaryVersionInfo versionInfo in downloadInfo.Versions)
        {
            if (versionInfo.Version.Equals(this.Version, StringComparison.OrdinalIgnoreCase))
            {
                return versionInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find download information for Chrome version '{this.Version}'.");
    }

    private string GetPlatformIdentifierString()
    {
        string platformKey;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            platformKey = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "mac-arm64" : "mac-x64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            platformKey = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "win64" : "win32";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            platformKey = "linux64";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }

        return platformKey;
    }

    private string InitializeExpectedExecutablePath(string expectedExecutablePath)
    {
        if (this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            return this.GetDefaultSystemInstalledLocation();
        }

        if (this.LocationBehavior == FileLocationBehavior.UseCustomLocation)
        {
            if (string.IsNullOrEmpty(expectedExecutablePath))
            {
                throw new ArgumentException("Executable path must be provided when using custom location behavior.", nameof(expectedExecutablePath));
            }

            return expectedExecutablePath;
        }

        return this.GetCachedRelativeLocation();
    }

    private string GetCachedRelativeLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string subdirectory = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "chrome-mac-arm64" : "chrome-mac-x64";
            return Path.Combine(subdirectory, "Google Chrome for Testing.app", "Contents", "MacOS", "Google Chrome for Testing");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine("chrome-linux64", "chrome");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string subdirectory = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "chrome-win64" : "chrome-win32";
            return Path.Combine(subdirectory, "chrome.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
    }

    private string GetDefaultSystemInstalledLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
                "usr",
                "bin",
                executableName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string baseDirectory = this.channelValue == ChromeChannel.Beta || this.channelValue == ChromeChannel.Canary
                ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string applicationSubdirectory = this.channelValue switch
            {
                ChromeChannel.Dev => "Chrome Dev",
                ChromeChannel.Beta => "Chrome Beta",
                ChromeChannel.Canary => "Chrome SxS",
                _ => "Chrome",
            };
            return Path.Combine(baseDirectory, "Google", applicationSubdirectory, "Application", "chrome.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} system install location.");
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
[JsonSerializable(typeof(Dictionary<string, List<ChromeBrowserLocatorSettings.FileDownloadInfo>>))]
[JsonSerializable(typeof(Dictionary<string, ChromeBrowserLocatorSettings.BinaryVersionInfo>))]
internal partial class ChromeBrowserLocatorSettingsJsonSerializerContext : JsonSerializerContext
{
}
