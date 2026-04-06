// <copyright file="BrowserLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Base class for locating and downloading browsers for testing.
/// </summary>
public class BrowserLocator
{
    /// <summary>
    /// Gets the the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Locator";

    // 1 MB buffer for downloads
    private const int BufferSize = 1024 * 1024;

    /// <summary>
    /// Gets the default base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    private static readonly string DefaultCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    private readonly BrowserLocatorSettings settings;

    private BrowserLocator(BrowserLocatorSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Gets or sets the directory where downloaded browsers should be cached. By default,
    /// the cache directory is a "webdriverbidi-net" subdirectory of a hidden ".cache"
    /// directory located in the user's profile directory.
    /// </summary>
    public string CacheDirectory { get; set; } = DefaultCacheDir;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser locator.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new("browserLocator.logMessage");

    /// <summary>
    /// Creates a new instance of the <see cref="BrowserLocator"/> class for the stable channel of the specified browser type.
    /// </summary>
    /// <param name="browserType">The type of browser to locate.</param>
    /// <returns>A new instance of the <see cref="BrowserLocator"/> class.</returns>
    public static BrowserLocator Create(BrowserType browserType)
    {
        return browserType switch
        {
            BrowserType.Chrome => new BrowserLocator(new ChromeBrowserLocatorSettings(ChromeChannel.Stable)),
            BrowserType.ChromePipe => new BrowserLocator(new ChromeBrowserLocatorSettings(ChromeChannel.Stable)),
            BrowserType.Firefox => new BrowserLocator(new FirefoxBrowserLocatorSettings(FirefoxChannel.Stable)),
            _ => throw new ArgumentException($"Unsupported browser type: {browserType}"),
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BrowserLocator"/> class for the specified browser type and version.
    /// </summary>
    /// <param name="browserType">The type of browser to locate.</param>
    /// <param name="version">The version of the browser to locate.</param>
    /// <returns>A new instance of the <see cref="BrowserLocator"/> class.</returns>
    /// <exception cref="ArgumentException">Thrown when an unsupported browser type is specified, or when no version is specified.</exception>
    public static BrowserLocator Create(BrowserType browserType, string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("Version must be specified when creating a BrowserLocator with a specific version.", nameof(version));
        }

        return browserType switch
        {
            BrowserType.Chrome => new BrowserLocator(new ChromeBrowserLocatorSettings(version)),
            BrowserType.ChromePipe => new BrowserLocator(new ChromeBrowserLocatorSettings(version)),
            BrowserType.Firefox => new BrowserLocator(new FirefoxBrowserLocatorSettings(version)),
            _ => throw new ArgumentException($"Unsupported browser type: {browserType}"),
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BrowserLocator"/> class for Chrome browsers
    /// using the specified channel. This will return the current active version for the
    /// specified channel.
    /// </summary>
    /// <param name="channel">The channel of the Chrome browser to locate.</param>
    /// <returns>A new instance of the <see cref="BrowserLocator"/> class.</returns>
    public static BrowserLocator Create(ChromeChannel channel)
    {
        return new BrowserLocator(new ChromeBrowserLocatorSettings(channel));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BrowserLocator"/> class for Firefox browsers
    /// using the specified channel. This will return the current active version for the
    /// specified channel.
    /// </summary>
    /// <param name="channel">The channel of the Firefox browser to locate.</param>
    /// <returns>A new instance of the <see cref="BrowserLocator"/> class.</returns>
    public static BrowserLocator Create(FirefoxChannel channel)
    {
        return new BrowserLocator(new FirefoxBrowserLocatorSettings(channel));
    }

    /// <summary>
    /// Gets the path to the browser executable, downloading it if necessary.
    /// If the browser-specific environment variable is set, returns that path directly.
    /// </summary>
    /// <returns>The path to the browser executable.</returns>
    public async Task<string> LocateBrowserExecutablePathAsync()
    {
        string? envPath = Environment.GetEnvironmentVariable(this.settings.EnvironmentVariableName);
        if (!string.IsNullOrEmpty(envPath))
        {
            await this.LogAsync($"Using '{this.settings.EnvironmentVariableName}': {envPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return envPath;
        }

        BrowserCache cacheInfo = BrowserCache.Load(this.CacheDirectory);
        InstalledBrowserInfo browserInfo = await this.GetInstalledBrowserInfoFromCacheAsync(cacheInfo).ConfigureAwait(false);

        string cachedInstallDirectory = Path.Combine(this.CacheDirectory, this.settings.BrowserName, this.settings.Channel, browserInfo.Version);
        string executablePath = Path.Combine(cachedInstallDirectory, this.settings.ExpectedExecutablePath);
        if (File.Exists(executablePath))
        {
            // There are times we need to force a new download, even when we have a
            // cached installation of the specified version of the browser. For example,
            // Firefox Nightly does not bump the version number each day, so we need to
            // download a new version with the same version number once per day, even if
            // we already have a cached version. In these circumstances, delete the
            // existing cached version first.
            if (browserInfo.ForceNewDownload)
            {
                Directory.Delete(cachedInstallDirectory, true);
            }
            else
            {
                await this.LogAsync($"Using cached {this.settings.BrowserDisplayName}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
                return executablePath;
            }
        }

        await this.DownloadAndExtractBrowserAsync(browserInfo, cachedInstallDirectory, executablePath).ConfigureAwait(false);
        cacheInfo.Save();
        await this.LogAsync($"{this.settings.BrowserDisplayName} ready at: {executablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        return executablePath;
    }

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination path.
    /// </summary>
    /// <param name="client">The HTTP client to use for the download.</param>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destPath">The path where the downloaded file should be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DownloadFileAsync(HttpClient client, string url, string destPath)
    {
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

        byte[] buffer = new byte[BufferSize];
        long totalRead = 0;
        int bytesRead;
        int lastPercent = -1;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalRead += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                int percent = (int)(totalRead * 100 / totalBytes.Value);
                if (percent != lastPercent && percent % 10 == 0)
                {
                    await this.LogAsync($"  Download progress: {percent}%", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
                    lastPercent = percent;
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    private async Task<InstalledBrowserInfo> GetInstalledBrowserInfoFromCacheAsync(BrowserCache cacheInfo)
    {
        InstalledBrowserInfo browserInfo;
        if (cacheInfo.TryGetCachedBrowser(this.settings, out InstalledBrowserInfo? cachedBrowserInfo))
        {
            browserInfo = cachedBrowserInfo;
        }
        else
        {
            browserInfo = new InstalledBrowserInfo
            {
                BrowserName = this.settings.BrowserName,
                Channel = this.settings.Channel,
            };
        }

        if (browserInfo.IsCachedVersionInfoExpired())
        {
            BrowserDownloadInfo downloadInfo = await this.settings.GetBrowserDownloadInfo().ConfigureAwait(false);
            browserInfo.Version = downloadInfo.Version;
            browserInfo.DirectDownloadUrl = downloadInfo.DownloadUrl;
            browserInfo.ForceNewDownload = downloadInfo.IgnoreVersionMatch;
        }

        if (!browserInfo.IsLoadedFromCache)
        {
            cacheInfo.AddBrowserToCache(browserInfo, this.settings.IsLatestChannelVersion);
        }

        return browserInfo;
    }

    private async Task DownloadAndExtractBrowserAsync(InstalledBrowserInfo browserInfo, string cachedInstallDirectory, string expectedExtractedExecutablePath)
    {
        if (!Directory.Exists(cachedInstallDirectory))
        {
            Directory.CreateDirectory(cachedInstallDirectory);
        }

        await this.LogAsync($"Downloading {this.settings.BrowserDisplayName}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        string downloadedInstallerPath = Path.Combine(cachedInstallDirectory, this.settings.InstallerFileName);
        using HttpClient client = new();
        await this.DownloadFileAsync(client, browserInfo.DirectDownloadUrl, downloadedInstallerPath).ConfigureAwait(false);
        await this.settings.Extractor.ExtractBrowserAsync(downloadedInstallerPath, cachedInstallDirectory).ConfigureAwait(false);

        if (!File.Exists(expectedExtractedExecutablePath))
        {
            throw new FileNotFoundException($"{this.settings.BrowserDisplayName} executable not found after extraction at: {expectedExtractedExecutablePath}");
        }

        browserInfo.LastDownload = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the structure of the browser cache information stored in the cache file.
    /// </summary>
    internal class BrowserCache
    {
        private static readonly string CacheInfoFileName = "cache-info.json";

        /// <summary>
        /// Gets or sets the dictionary of installed browsers, keyed by browser name.
        /// </summary>
        [JsonPropertyName("installedBrowsers")]
        [JsonInclude]
        public Dictionary<string, CachedBrowserInfo> InstalledBrowsers { get; set; } = [];

        private string CacheInfoFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Loads the browser cache information from the cache file, or returns an empty cache
        /// if the file does not exist or cannot be parsed.
        /// </summary>
        /// <param name="cacheDirectory">The directory where the installed browser cache is located.</param>
        /// <returns>The loaded browser cache information.</returns>
        public static BrowserCache Load(string cacheDirectory)
        {
            BrowserCache? cacheInfo = null;
            string cacheInfoFile = Path.Combine(cacheDirectory, CacheInfoFileName);
            if (File.Exists(cacheInfoFile))
            {
                try
                {
                    string text = File.ReadAllText(cacheInfoFile).Trim();
                    cacheInfo = JsonSerializer.Deserialize(text, BrowserLocatorJsonContext.Default.BrowserCache);
                }
                catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException)
                {
                    // If parsing fails, treat as stale cache and overwrite on next download.
                }
            }

            cacheInfo ??= new BrowserCache();
            cacheInfo.CacheInfoFilePath = cacheInfoFile;
            return cacheInfo;
        }

        /// <summary>
        /// Saves the current browser cache information to the cache file, creating or overwriting it as necessary.
        /// </summary>
        public void Save()
        {
            string json = JsonSerializer.Serialize(this, BrowserLocatorJsonContext.Default.BrowserCache);
            File.WriteAllText(this.CacheInfoFilePath, json);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains valid information for the specified browser locator settings.
        /// </summary>
        /// <param name="settings">>The <see cref="BrowserLocatorSettings"/> object to match for installed browsers.</param>
        /// <param name="browserInfo">The cached browser information, or null if not found.</param>
        /// <returns><see langword="true"/> if cached information is found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetCachedBrowser(BrowserLocatorSettings settings, [NotNullWhen(true)] out InstalledBrowserInfo? browserInfo)
        {
            browserInfo = null;
            if (this.InstalledBrowsers.TryGetValue(settings.BrowserName, out CachedBrowserInfo? browserInfoCache))
            {
                if (browserInfoCache.Channels.TryGetValue(settings.Channel, out CachedBrowserChannelInfo? channelCache))
                {
                    string version = settings.Version == BrowserLocatorSettings.LatestVersionString ? channelCache.DefaultVersion ?? string.Empty : settings.Version;
                    foreach (InstalledBrowserInfo info in channelCache.Versions)
                    {
                        if (info.Version == version)
                        {
                            info.IsLoadedFromCache = true;
                            browserInfo = info;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the specified browser information to the cache, creating entries for the browser
        /// and channel if they do not already exist.
        /// </summary>
        /// <param name="browserInfo">The <see cref="InstalledBrowserInfo"/> object to add to the cache.</param>
        /// <param name="isDefaultChannelVersion">A value indicating whether the browser information represents the most recent version for its channel.</param>
        public void AddBrowserToCache(InstalledBrowserInfo browserInfo, bool isDefaultChannelVersion = false)
        {
            if (!this.InstalledBrowsers.TryGetValue(browserInfo.BrowserName, out CachedBrowserInfo? browserCacheInfo))
            {
                browserCacheInfo = new CachedBrowserInfo
                {
                    Channels = [],
                };
                this.InstalledBrowsers[browserInfo.BrowserName] = browserCacheInfo;
            }

            if (!browserCacheInfo.Channels.TryGetValue(browserInfo.Channel, out CachedBrowserChannelInfo? channelCacheInfo))
            {
                channelCacheInfo = new CachedBrowserChannelInfo
                {
                    Versions = [],
                };
                browserCacheInfo.Channels[browserInfo.Channel] = channelCacheInfo;
            }

            if (isDefaultChannelVersion)
            {
                channelCacheInfo.DefaultVersion = browserInfo.Version;
            }

            channelCacheInfo.Versions.Add(browserInfo);
        }
    }

    /// <summary>
    /// Represents the cached information about a specific browser.
    /// </summary>
    internal class CachedBrowserInfo
    {
        /// <summary>
        /// Gets or sets the dictionary of channels for the browser, keyed by channel name.
        /// </summary>
        [JsonPropertyName("channels")]
        [JsonInclude]
        public Dictionary<string, CachedBrowserChannelInfo> Channels { get; set; } = [];
    }

    /// <summary>
    /// Represents the cached information about a specific browser channel.
    /// </summary>
    internal class CachedBrowserChannelInfo
    {
        /// <summary>
        /// Gets or sets the most recent version for the channel.
        /// </summary>
        [JsonPropertyName("defaultVersion")]
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultVersion { get; set; }

        /// <summary>
        /// Gets or sets the information for each version of the browser installed for the channel.
        /// </summary>
        [JsonPropertyName("versions")]
        [JsonInclude]
        public List<InstalledBrowserInfo> Versions { get; set; } = [];
    }

    /// <summary>
    /// Represents the cached information about a specific installed browser version, including
    /// the last download time, version, and direct download URL.
    /// </summary>
    internal class InstalledBrowserInfo
    {
        /// <summary>
        /// Gets or sets the last time the browser version was downloaded and installed.
        /// </summary>
        [JsonPropertyName("lastDownload")]
        [JsonInclude]
        public DateTime LastDownload { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the name of the browser (e.g., "chrome", "firefox").
        /// </summary>
        [JsonPropertyName("browserName")]
        [JsonInclude]
        public string BrowserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the channel of the browser (e.g., "stable", "beta", "canary", "nightly").
        /// </summary>
        [JsonPropertyName("channel")]
        [JsonInclude]
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the browser.
        /// </summary>
        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the direct download URL for the browser version.
        /// </summary>
        [JsonPropertyName("directDownloadUrl")]
        [JsonInclude]
        public string DirectDownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to force a new download of the browser version,
        /// even if it is already installed in the cache.
        /// </summary>
        [JsonIgnore]
        public bool ForceNewDownload { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InstalledBrowserInfo"/> was loaded from the cache.
        /// </summary>
        [JsonIgnore]
        public bool IsLoadedFromCache { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether the cached browser information is expired based on the last download time.
        /// </summary>
        /// <returns><see langword="true"/> if the cached information is expired; otherwise, <see langword="false"/>.</returns>
        public bool IsCachedVersionInfoExpired()
        {
            bool isCacheExpired = DateTime.UtcNow - this.LastDownload > TimeSpan.FromHours(24);
            return isCacheExpired;
        }
    }
}

#pragma warning disable SA1402 // File may only contain a single type
/// <summary>
/// A source generation context for JSON serialization of browser locator cache information.
/// This is used to enable serialization and deserialization of the browser cache information
/// when used in AOT environments.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BrowserLocator.BrowserCache))]
[JsonSerializable(typeof(Dictionary<string, BrowserLocator.CachedBrowserInfo>))]
[JsonSerializable(typeof(Dictionary<string, BrowserLocator.CachedBrowserChannelInfo>))]
[JsonSerializable(typeof(List<BrowserLocator.InstalledBrowserInfo>))]
internal partial class BrowserLocatorJsonContext : JsonSerializerContext
{
}
