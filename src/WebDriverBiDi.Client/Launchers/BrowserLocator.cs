// <copyright file="BrowserLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Base class for locating and downloading browsers for testing.
/// </summary>
public class BrowserLocator
{
    /// <summary>
    /// Gets the base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    protected static readonly string BaseCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    // 1 MB buffer for downloads
    private const int BufferSize = 1024 * 1024;

    private static readonly string CacheInfoFile = Path.Combine(BaseCacheDir, "cache-info.json");

    private readonly BrowserLocatorSettings settings;

    private BrowserLocator(BrowserLocatorSettings settings)
    {
        this.settings = settings;
    }

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
            Console.WriteLine($"Using '{this.settings.EnvironmentVariableName}': {envPath}");
            return envPath;
        }

        BrowserCacheInfo cacheInfo = this.LoadCacheInfo();
        InstalledBrowserInfo? browserInfo = cacheInfo.GetCachedBrowser(this.settings);

        string browserVersion;
        string directDownloadUrl = string.Empty;
        bool ignoreVersionMatch = false;
        if (browserInfo is not null && !browserInfo.IsCachedVersionInfoStale())
        {
            browserVersion = browserInfo.Version;
        }
        else
        {
            BrowserDownloadInfo downloadInfo = await this.settings.GetBrowserDownloadInfo().ConfigureAwait(false);
            browserVersion = downloadInfo.Version;
            directDownloadUrl = downloadInfo.DownloadUrl;
            ignoreVersionMatch = downloadInfo.IgnoreVersionMatch;
            browserInfo = new InstalledBrowserInfo
            {
                BrowserName = this.settings.BrowserName,
                Channel = this.settings.Channel,
                Version = browserVersion,
                DirectDownloadUrl = downloadInfo.DownloadUrl,
            };

            cacheInfo.AddBrowserToCache(browserInfo, this.settings.IsLatestChannelVersion);
        }

        string cachedInstallDirectory = Path.Combine(BaseCacheDir, this.settings.BrowserName, this.settings.Channel, browserVersion);
        string executablePath = Path.Combine(cachedInstallDirectory, this.settings.ExpectedExecutablePath);
        if (File.Exists(executablePath))
        {
            if (ignoreVersionMatch)
            {
                Directory.Delete(cachedInstallDirectory, true);
            }
            else
            {
                Console.WriteLine($"Using cached {this.settings.BrowserDisplayName}.");
                return executablePath;
            }
        }

        if (!Directory.Exists(cachedInstallDirectory))
        {
            Directory.CreateDirectory(cachedInstallDirectory);
        }

        Console.WriteLine($"Downloading {this.settings.BrowserDisplayName}...");
        string downloadedInstallerPath = Path.Combine(cachedInstallDirectory, this.settings.InstallerFileName);
        using HttpClient client = new();
        await this.DownloadFileAsync(client, directDownloadUrl, downloadedInstallerPath).ConfigureAwait(false);
        await this.settings.Extractor.ExtractBrowserAsync(downloadedInstallerPath, cachedInstallDirectory).ConfigureAwait(false);

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"{this.settings.BrowserDisplayName} executable not found after extraction at: {executablePath}");
        }

        browserInfo.LastDownload = DateTime.UtcNow;
        this.SaveCacheInfo(cacheInfo);
        Console.WriteLine($"{this.settings.BrowserDisplayName} ready at: {executablePath}");
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
                    Console.WriteLine($"  Download progress: {percent}%");
                    lastPercent = percent;
                }
            }
        }
    }

    private BrowserCacheInfo LoadCacheInfo()
    {
        if (File.Exists(CacheInfoFile))
        {
            try
            {
                string text = File.ReadAllText(CacheInfoFile).Trim();
                BrowserCacheInfo? cacheInfo = JsonSerializer.Deserialize<BrowserCacheInfo>(text);
                if (cacheInfo is not null)
                {
                    return cacheInfo;
                }
            }
            catch
            {
                // If parsing fails, treat as stale cache and overwrite on next download.
            }
        }

        return new BrowserCacheInfo();
    }

    private void SaveCacheInfo(BrowserCacheInfo cacheInfo)
    {
        string json = JsonSerializer.Serialize(cacheInfo, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(CacheInfoFile, json);
    }

    private class BrowserCacheInfo
    {
        [JsonPropertyName("installedBrowsers")]
        [JsonInclude]
        public Dictionary<string, BrowserInfo> InstalledBrowsers { get; set; } = [];

        public InstalledBrowserInfo? GetCachedBrowser(BrowserLocatorSettings settings)
        {
            if (this.InstalledBrowsers.TryGetValue(settings.BrowserName, out BrowserInfo? browserInfo))
            {
                if (browserInfo.Channels.TryGetValue(settings.Channel, out BrowserChannelInfo? browserChannel))
                {
                    string version = settings.Version;
                    foreach (InstalledBrowserInfo info in browserChannel.Versions)
                    {
                        if (info.Version == version)
                        {
                            return info;
                        }
                    }
                }
            }

            return null;
        }

        public void AddBrowserToCache(InstalledBrowserInfo browserInfo, bool isDefaultChannelVersion = false)
        {
            if (!this.InstalledBrowsers.TryGetValue(browserInfo.BrowserName, out BrowserInfo? browserCacheInfo))
            {
                browserCacheInfo = new BrowserInfo
                {
                    Channels = [],
                };
                this.InstalledBrowsers[browserInfo.BrowserName] = browserCacheInfo;
            }

            if (!browserCacheInfo.Channels.TryGetValue(browserInfo.Channel, out BrowserChannelInfo? channelCacheInfo))
            {
                channelCacheInfo = new BrowserChannelInfo
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

    private class BrowserInfo
    {
        [JsonPropertyName("channels")]
        [JsonInclude]
        public Dictionary<string, BrowserChannelInfo> Channels { get; set; } = [];
    }

    private class BrowserChannelInfo
    {
        [JsonPropertyName("defaultVersion")]
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultVersion { get; set; }

        [JsonPropertyName("versions")]
        [JsonInclude]
        public List<InstalledBrowserInfo> Versions { get; set; } = [];
    }

    private class InstalledBrowserInfo
    {
        [JsonPropertyName("lastDownload")]
        [JsonInclude]
        public DateTime LastDownload { get; set; } = DateTime.MinValue;

        [JsonPropertyName("browserName")]
        [JsonInclude]
        public string BrowserName { get; set; } = string.Empty;

        [JsonPropertyName("channel")]
        [JsonInclude]
        public string Channel { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("directDownloadUrl")]
        [JsonInclude]
        public string DirectDownloadUrl { get; set; } = string.Empty;

        public bool IsCachedVersionInfoValid()
        {
            bool cacheExpired = DateTime.UtcNow - this.LastDownload <= TimeSpan.FromHours(24);
            return cacheExpired;
        }

        public bool IsCachedVersionInfoStale()
        {
            bool cacheExpired = DateTime.UtcNow - this.LastDownload > TimeSpan.FromHours(24);
            return cacheExpired;
        }
    }
}
