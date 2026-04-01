// <copyright file="ChromeLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Base class for locating and downloading Chrome browsers for testing.
/// </summary>
public class ChromeLocator : BrowserLocator
{
    private const string DownloadInfoFile = "last-known-good-versions-with-downloads.json";
    private static readonly ChromeCanaryLocator CanaryLocator = new();
    private static readonly ChromeStableLocator StableLocator = new();

    /// <summary>
    /// Gets a locator for Chrome canary channel binaries.
    /// </summary>
    public ChromeLocator Canary { get; } = CanaryLocator;

    /// <summary>
    /// Gets a locator for Chrome stable channel binaries.
     ///
    /// </summary>
    public ChromeLocator Stable { get; } = StableLocator;

    /// <summary>
    /// Gets the name of the environment variable that can be used to specify a custom path to the Chrome executable.
    /// If this environment variable is set, the locator will return its value directly instead of downloading Chrome.
    /// </summary>
    protected override string EnvironmentVariableName { get; } = "CHROME_EXECUTABLE";

    /// <summary>
    /// Gets the user-friendly name of the browser being located, used for logging and error messages.
    /// </summary>
    protected override string BrowserName => "Chrome";

    /// <summary>
    /// Gets the cache directory specific to this browser, which is a subdirectory of the base cache directory.
    /// </summary>
    protected override string CacheDir => BaseCacheDir;

    /// <summary>
    /// Gets the direct download URL for the Chrome installer for the current platform.
    /// </summary>
    /// <returns>The direct download URL for the Chrome installer for the current platform.</returns>
    protected override async Task<string> GetDownloadUrl()
    {
        string chromeBinaryDownloadInfoFile = Path.Combine(BaseCacheDir, DownloadInfoFile);
        if (this.IsCacheStale() || !File.Exists(chromeBinaryDownloadInfoFile))
        {
            using HttpClient httpClient = new();
            string chromeBinaryDownloadInfoUrl = "https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json";
            await this.DownloadFileAsync(httpClient, chromeBinaryDownloadInfoUrl, chromeBinaryDownloadInfoFile).ConfigureAwait(false);
        }

        string json = File.ReadAllText(chromeBinaryDownloadInfoFile);
        ChromeBinaryDownloadInfo? downloadInfo = JsonSerializer.Deserialize<ChromeBinaryDownloadInfo>(json) ?? throw new InvalidOperationException($"Failed to deserialize Chrome binary download information from {chromeBinaryDownloadInfoFile}.");
        string channel = this switch
        {
            ChromeCanaryLocator => "Canary",
            ChromeStableLocator => "Stable",
            _ => throw new InvalidOperationException($"Unknown Chrome channel for locator of type {this.GetType().Name}."),
        };
        if (!downloadInfo.Channels.TryGetValue(channel, out ChannelInfo? channelInfo))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome channel {channel}.");
        }

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
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
        }

        if (!channelInfo.Downloads.TryGetValue("chrome", out List<FileDownloadInfo>? downloadsForPlatform))
        {
            throw new InvalidOperationException($"Failed to find download information for Chrome platform.");
        }

        foreach (FileDownloadInfo download in downloadsForPlatform)
        {
            if (download.Platform.Equals(platformKey, StringComparison.OrdinalIgnoreCase))
            {
                return download.Url;
            }
        }

        throw new InvalidOperationException($"Failed to find download URL for Chrome platform {platformKey}.");
    }

    /// <summary>
    /// Performs the download and extraction of the browser for the current platform.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task DownloadAndExtractAsync()
    {
        Directory.CreateDirectory(this.CacheDir);

        string url = await this.GetDownloadUrl();
        using HttpClient client = new();
        client.Timeout = TimeSpan.FromMinutes(10);

        string zipPath = Path.Combine(this.CacheDir, this.GetInstallerFileName());
        await this.DownloadFileAsync(client, url, zipPath);

        try
        {
            string chromeDir = Path.Combine(this.CacheDir, "chrome");
            if (Directory.Exists(chromeDir))
            {
                Directory.Delete(chromeDir, true);
            }

            ZipFile.ExtractToDirectory(zipPath, this.CacheDir);
        }
        finally
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
        }
    }

    /// <summary>
    /// Gets the expected path to the Chrome executable after extraction, based on the platform.
    /// </summary>
    /// <returns>The expected path to the Chrome executable after extraction, based on the platform.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
    protected override string GetExpectedExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string subdirectory = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "chrome-mac-arm64" : "chrome-mac-x64";
            return Path.Combine(this.CacheDir, subdirectory, "Google Chrome for Testing.app", "Contents", "MacOS", "Google Chrome for Testing");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(this.CacheDir, "chrome-linux64", "chrome");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string subdirectory = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "chrome-win64" : "chrome-win32";
            return Path.Combine(this.CacheDir, subdirectory, "chrome.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
    }

    /// <summary>
    /// Gets the file name used for the downloaded installer for the current platform.
    /// </summary>
    /// <returns>The file name for the downloaded installer.</returns>
    protected override string GetInstallerFileName()
    {
        throw new NotImplementedException();
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

    private record ChannelInfo
    {
        [JsonConstructor]
        public ChannelInfo(string channel, string version, string revision, Dictionary<string, List<FileDownloadInfo>> downloads)
        {
            this.Channel = channel;
            this.Version = version;
            this.Revision = revision;
            this.Downloads = downloads;
        }

        [JsonPropertyName("channel")]
        [JsonInclude]
        public string Channel { get; set; }

        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; }

        [JsonPropertyName("revision")]
        [JsonInclude]
        public string Revision { get; set; }

        [JsonPropertyName("downloads")]
        [JsonInclude]
        public Dictionary<string, List<FileDownloadInfo>> Downloads { get; set; }
    }

    private record ChromeBinaryDownloadInfo
    {
        [JsonConstructor]
        public ChromeBinaryDownloadInfo(string timestamp, Dictionary<string, ChannelInfo> channels)
        {
            this.Timestamp = timestamp;
            this.Channels = channels;
        }

        [JsonPropertyName("timestamp")]
        [JsonInclude]
        public string Timestamp { get; set; }

        [JsonPropertyName("channels")]
        [JsonInclude]
        public Dictionary<string, ChannelInfo> Channels { get; set; }
    }
}
