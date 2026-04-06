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
public class ChromeBrowserLocatorSettings : BrowserLocatorSettings
{
    private const string ChannelDownloadInfoUrl = "https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json";
    private const string AllVersionsDownloadInfoUrl = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";

    private readonly ChromeChannel channelValue;
    private readonly string versionValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="version">The specific released version of the Chrome browser.</param>
    public ChromeBrowserLocatorSettings(string version)
        : this(ChromeChannel.Stable, $"Chrome {version}")
    {
        this.versionValue = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Chrome browser.</param>
    public ChromeBrowserLocatorSettings(ChromeChannel channel)
        : this(channel, $"Chrome {channel}", true)
    {
    }

    private ChromeBrowserLocatorSettings(ChromeChannel channel, string browserDisplayName, bool isDefaultChannelVersion = false)
    {
        this.channelValue = channel;
        this.BrowserName = "chrome";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = browserDisplayName;
        this.IsLatestChannelVersion = isDefaultChannelVersion;
        this.EnvironmentVariableName = "CHROME_EXECUTABLE";
        this.InstallerFileName = $"{this.BrowserName}-{this.Channel}.zip";
        this.ExpectedExecutablePath = this.InitializeExpectedExecutablePath();
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome").
    /// </summary>
    public override string BrowserName { get; } = "chrome";

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

        BinaryVersionInfo binaryVersionInfo = string.IsNullOrEmpty(this.versionValue)
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

    private string GetBinaryDownloadInfoUrl()
    {
        if (string.IsNullOrEmpty(this.versionValue))
        {
            return ChannelDownloadInfoUrl;
        }

        return AllVersionsDownloadInfoUrl;
    }

    private BinaryVersionInfo GetChannelBinaryVersionInfo(string json)
    {
        ChromeChannelBinaryDownloadInfo? downloadInfo =
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonContext.Default.ChromeChannelBinaryDownloadInfo) ??
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
            JsonSerializer.Deserialize(json, ChromeBrowserLocatorSettingsJsonContext.Default.ChromeAllVersionsBinaryDownloadInfo)
            ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {ChannelDownloadInfoUrl}.");
        foreach (BinaryVersionInfo versionInfo in downloadInfo.Versions)
        {
            if (versionInfo.Version.Equals(this.versionValue, StringComparison.OrdinalIgnoreCase))
            {
                return versionInfo;
            }
        }

        throw new InvalidOperationException($"Failed to find download information for Chrome version {this.versionValue}.");
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

    private string InitializeExpectedExecutablePath()
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
internal partial class ChromeBrowserLocatorSettingsJsonContext : JsonSerializerContext
{
}
