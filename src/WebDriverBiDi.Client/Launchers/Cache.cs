// <copyright file="Cache.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the structure of the browser cache information stored in the cache file.
/// </summary>
internal class Cache
{
    private static readonly string CacheInfoFileName = "cache-info.json";

    private string cacheInfoFilePath = string.Empty;

    /// <summary>
    /// Gets or sets the dictionary of installed browsers, keyed by browser name.
    /// </summary>
    [JsonPropertyName("installedBrowsers")]
    [JsonInclude]
    public Dictionary<string, CachedBrowserInfo> InstalledBrowsers { get; set; } = [];

    /// <summary>
    /// Gets or sets the dictionary of installed drivers, keyed by driver name.
    /// </summary>
    [JsonPropertyName("installedDrivers")]
    [JsonInclude]
    public Dictionary<string, CachedDriverInfo> InstalledDrivers { get; set; } = [];

    /// <summary>
    /// Loads the browser cache information from the cache file, or returns an empty cache
    /// if the file does not exist or cannot be parsed.
    /// </summary>
    /// <param name="cacheDirectory">The directory where the installed browser cache is located.</param>
    /// <returns>The loaded browser cache information.</returns>
    public static Cache Load(string cacheDirectory)
    {
        Cache? cacheInfo = null;
        string cacheInfoFile = Path.Combine(cacheDirectory, CacheInfoFileName);
        if (File.Exists(cacheInfoFile))
        {
            try
            {
                string text = File.ReadAllText(cacheInfoFile).Trim();
                cacheInfo = JsonSerializer.Deserialize(text, BrowserLocatorJsonSerializerContext.Default.Cache);
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException)
            {
                // If parsing fails, treat as stale cache and overwrite on next download.
            }
        }

        cacheInfo ??= new Cache();
        cacheInfo.cacheInfoFilePath = cacheInfoFile;
        return cacheInfo;
    }

    /// <summary>
    /// Saves the current browser cache information to the cache file, creating or overwriting it as necessary.
    /// </summary>
    public void Save()
    {
        string json = JsonSerializer.Serialize(this, BrowserLocatorJsonSerializerContext.Default.Cache);
        File.WriteAllText(this.cacheInfoFilePath, json);
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
                // string version = settings.Version == BrowserLocatorSettings.LatestVersionString ? channelCache.DefaultVersion ?? string.Empty : settings.Version;
                string version = settings.IsLatestChannelVersion ? channelCache.DefaultVersion ?? string.Empty : settings.Version;
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

    /// <summary>
    /// Gets a value indicating whether the cache contains valid information for the specified driver.
    /// </summary>
    /// <param name="driverName">The name of the driver (e.g., "chromedriver", "geckodriver").</param>
    /// <param name="driverVersion">The version of the driver.</param>
    /// <param name="driverInfo">The cached driver information, or null if not found.</param>
    /// <returns><see langword="true"/> if cached information is found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetCachedDriver(string driverName, string driverVersion, [NotNullWhen(true)] out InstalledDriverInfo? driverInfo)
    {
        driverInfo = null;
        if (this.InstalledDrivers.TryGetValue(driverName, out CachedDriverInfo? driverCache))
        {
            foreach (InstalledDriverInfo info in driverCache.Versions)
            {
                if (info.Version == driverVersion)
                {
                    info.IsLoadedFromCache = true;
                    driverInfo = info;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Adds the specified driver information to the cache, creating entries for the driver if they do not already exist.
    /// </summary>
    /// <param name="driverInfo">The <see cref="InstalledDriverInfo"/> object to add to the cache.</param>
    public void AddDriverToCache(InstalledDriverInfo driverInfo)
    {
        if (!this.InstalledDrivers.TryGetValue(driverInfo.DriverName, out CachedDriverInfo? driverCacheInfo))
        {
            driverCacheInfo = new CachedDriverInfo
            {
                Versions = [],
            };
            this.InstalledDrivers[driverInfo.DriverName] = driverCacheInfo;
        }

        driverCacheInfo.Versions.Add(driverInfo);
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

    /// <summary>
    /// Represents the cached information about a specific driver.
    /// </summary>
    internal class CachedDriverInfo
    {
        /// <summary>
        /// Gets or sets the information for each version of the driver installed.
        /// </summary>
        [JsonPropertyName("versions")]
        [JsonInclude]
        public List<InstalledDriverInfo> Versions { get; set; } = [];
    }

    /// <summary>
    /// Represents the cached information about a specific installed driver version, including
    /// the last download time, version, matched browser version, and direct download URL.
    /// </summary>
    internal class InstalledDriverInfo
    {
        /// <summary>
        /// Gets or sets the last time the driver version was downloaded and installed.
        /// </summary>
        [JsonPropertyName("lastDownload")]
        [JsonInclude]
        public DateTime LastDownload { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the name of the driver (e.g., "chromedriver", "geckodriver").
        /// </summary>
        [JsonPropertyName("driverName")]
        [JsonInclude]
        public string DriverName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the driver.
        /// </summary>
        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the browser that this driver is compatible with.
        /// </summary>
        [JsonPropertyName("browserVersion")]
        [JsonInclude]
        public string BrowserVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the direct download URL for the driver version.
        /// </summary>
        [JsonPropertyName("directDownloadUrl")]
        [JsonInclude]
        public string DirectDownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InstalledDriverInfo"/> was loaded from the cache.
        /// </summary>
        [JsonIgnore]
        public bool IsLoadedFromCache { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether the cached driver information is expired based on the last download time.
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
[JsonSerializable(typeof(Cache))]
[JsonSerializable(typeof(Dictionary<string, Cache.CachedBrowserInfo>))]
[JsonSerializable(typeof(Dictionary<string, Cache.CachedBrowserChannelInfo>))]
[JsonSerializable(typeof(List<Cache.InstalledBrowserInfo>))]
[JsonSerializable(typeof(Dictionary<string, Cache.CachedDriverInfo>))]
[JsonSerializable(typeof(List<Cache.InstalledDriverInfo>))]
internal partial class BrowserLocatorJsonSerializerContext : JsonSerializerContext
{
}
