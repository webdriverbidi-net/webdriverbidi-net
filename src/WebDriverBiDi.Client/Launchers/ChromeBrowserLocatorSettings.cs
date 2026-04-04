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
            ? await this.GetChannelBinaryVersionInfo(json)
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

    private async Task<BinaryVersionInfo> GetChannelBinaryVersionInfo(string json)
    {
        ChromeChannelBinaryDownloadInfo? downloadInfo = JsonSerializer.Deserialize<ChromeChannelBinaryDownloadInfo>(json) ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {ChannelDownloadInfoUrl}.");
        string channel = this.channelValue.ToString();
        if (!downloadInfo.Channels.TryGetValue(channel, out BinaryVersionInfo? channelInfo))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome channel {channel}.");
        }

        return channelInfo;
    }

    private BinaryVersionInfo GetSpecificBinaryVersionInfo(string json)
    {
        ChromeAllVersionsBinaryDownloadInfo? downloadInfo = JsonSerializer.Deserialize<ChromeAllVersionsBinaryDownloadInfo>(json) ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {ChannelDownloadInfoUrl}.");
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

    private record FileDownloadInfo
    {
        [JsonConstructor]
        public FileDownloadInfo(string platform, string url)
        {
            this.Platform = platform;
            this.Url = url;
        }

        [JsonPropertyName("platform")]
        [JsonInclude]
        public string Platform { get; set; }

        [JsonPropertyName("url")]
        [JsonInclude]
        public string Url { get; set; }
    }

    private record ChromeAllVersionsBinaryDownloadInfo
    {
        [JsonConstructor]
        public ChromeAllVersionsBinaryDownloadInfo()
        {
        }

        [JsonPropertyName("timestamp")]
        [JsonInclude]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("versions")]
        [JsonInclude]
        public List<BinaryVersionInfo> Versions { get; set; } = [];
    }

    private record BinaryVersionInfo
    {
        [JsonConstructor]
        public BinaryVersionInfo()
        {
        }

        [JsonPropertyName("channel")]
        [JsonInclude]
        public string Channel { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("revision")]
        [JsonInclude]
        public string Revision { get; set; } = string.Empty;

        [JsonPropertyName("downloads")]
        [JsonInclude]
        public Dictionary<string, List<FileDownloadInfo>> Downloads { get; set; } = [];
    }

    private record ChromeChannelBinaryDownloadInfo
    {
        [JsonConstructor]
        public ChromeChannelBinaryDownloadInfo()
        {
        }

        [JsonPropertyName("timestamp")]
        [JsonInclude]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("channels")]
        [JsonInclude]
        public Dictionary<string, BinaryVersionInfo> Channels { get; set; } = [];
    }
}
