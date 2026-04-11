// <copyright file="BrowserLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Base class for locating and downloading browsers for testing.
/// </summary>
public class BrowserLocator
{
    /// <summary>
    /// Gets the the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Locator";

    /// <summary>
    /// Gets the default base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    private static readonly string DefaultCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    private readonly BrowserLocatorSettings settings;
    private readonly DriverLocator? driverLocator;
    private string cacheDirectory = DefaultCacheDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLocator"/> class.
    /// </summary>
    /// <param name="settings">The <see cref="BrowserLocatorSettings"/> for the browser locator used by the launcher.</param>
    /// <param name="driverLocator">Optional <see cref="DriverLocator"/> to use for locating driver executables. If null and settings.IncludeDriver is true, a new instance will be created.</param>
    public BrowserLocator(BrowserLocatorSettings settings, DriverLocator? driverLocator = null)
    {
        this.settings = settings;

        // If IncludeDriver is true and no driver locator provided, create one
        if (settings.IncludeDriver && driverLocator is null)
        {
            driverLocator = new DriverLocator(settings);
        }

        this.driverLocator = driverLocator;

        // Wire up driver locator logging to forward through browser locator's event
        if (this.driverLocator is not null)
        {
            this.driverLocator.CacheDirectory = this.cacheDirectory;
            this.driverLocator.OnLogMessage.AddObserver(async args =>
                await this.OnLogMessage.NotifyObserversAsync(args).ConfigureAwait(false));
        }
    }

    /// <summary>
    /// Gets or sets the directory where downloaded browsers should be cached. By default,
    /// the cache directory is a "webdriverbidi-net" subdirectory of a hidden ".cache"
    /// directory located in the user's profile directory.
    /// </summary>
    public string CacheDirectory
    {
        get => this.cacheDirectory;
        set
        {
            this.cacheDirectory = value;
            if (this.driverLocator is not null)
            {
                this.driverLocator.CacheDirectory = value;
            }
        }
    }

    /// <summary>
    /// Gets the name of the browser being located (e.g., "Chrome", "Firefox").
    /// </summary>
    public string BrowserName => this.settings.BrowserName;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser locator.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new("browserLocator.logMessage");

    /// <summary>
    /// Gets the location information for the browser executable and optionally the driver executable,
    /// downloading them if necessary. This method minimizes network calls by fetching both from a
    /// single API call when possible (Chrome) or coordinating two calls and caching them together (Firefox).
    /// </summary>
    /// <returns>A <see cref="BrowserExecutableInfo"/> containing the browser path and optionally the driver path.</returns>
    public async Task<BrowserExecutableInfo> LocateExecutablesAsync()
    {
        // Determine if we need to load the cache (only if either browser or driver uses AutoLocateAndDownload)
        bool needsCache = this.settings.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload ||
                          (this.settings.IncludeDriver && this.settings.DriverLocationBehavior == FileLocationBehavior.AutoLocateAndDownload);

        Cache? cacheInfo = needsCache ? Cache.Load(this.CacheDirectory) : null;

        // Locate browser (handles its own environment variable check)
        string browserPath = await this.LocateBrowserPathAsync(cacheInfo).ConfigureAwait(false);

        // Locate driver if requested using DriverLocator (handles its own environment variable check)
        string? driverPath = null;
        if (this.driverLocator is not null)
        {
            driverPath = await this.driverLocator.LocateDriverAsync(cacheInfo).ConfigureAwait(false);
        }

        // Save cache if it was loaded (it's only loaded when using AutoLocateAndDownload)
        cacheInfo?.Save();

        return new BrowserExecutableInfo(browserPath, driverPath);
    }

    /// <summary>
    /// Gets the path to the browser executable, downloading it if necessary.
    /// If the browser-specific environment variable is set, returns that path directly.
    /// This is a convenience method that calls <see cref="LocateExecutablesAsync"/> and returns only the browser path.
    /// </summary>
    /// <returns>The path to the browser executable.</returns>
    public async Task<string> LocateBrowserAsync()
    {
        // Temporarily disable driver inclusion for backward compatibility
        bool originalIncludeDriver = this.settings.IncludeDriver;
        this.settings.IncludeDriver = false;
        try
        {
            BrowserExecutableInfo info = await this.LocateExecutablesAsync().ConfigureAwait(false);
            return info.BrowserPath;
        }
        finally
        {
            this.settings.IncludeDriver = originalIncludeDriver;
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

    private async Task<string> LocateBrowserPathAsync(Cache? cacheInfo)
    {
        // Check environment variable first
        string? envBrowserPath = Environment.GetEnvironmentVariable(this.settings.EnvironmentVariableName);
        if (!string.IsNullOrEmpty(envBrowserPath))
        {
            await this.LogAsync($"Using environment variable '{this.settings.EnvironmentVariableName}': {envBrowserPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return envBrowserPath;
        }

        if (this.settings.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            if (!this.settings.IncludeDriver)
            {
                // If we are using a browser driver, this has already been logged.
                await this.LogAsync($"Using {this.settings.BrowserLocationBehaviorDescription} browser at: {this.settings.ExpectedExecutablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            }

            return this.settings.ExpectedExecutablePath;
        }

        if (this.settings.LocationBehavior == FileLocationBehavior.UseCustomLocation)
        {
            if (!this.settings.IncludeDriver)
            {
                // If we are using a browser driver, this has already been logged.
                await this.LogAsync($"Using {this.settings.BrowserLocationBehaviorDescription} browser at: {this.settings.ExpectedExecutablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            }

            return this.settings.ExpectedExecutablePath;
        }

        // If we fall through to this point, the behavior must be AutoLocateAndDownload.
        if (cacheInfo is null)
        {
            throw new InvalidOperationException("Cache should have been loaded for AutoLocateAndDownload behavior.");
        }

        BrowserDownloadInfo browserDownloadInfo = await this.settings.GetBrowserDownloadInfo().ConfigureAwait(false);
        Cache.InstalledBrowserInfo browserInfo = await this.GetInstalledBrowserInfoFromCacheAsync(cacheInfo, browserDownloadInfo).ConfigureAwait(false);
        string cachedInstallDirectory = Path.Combine(this.CacheDirectory, this.settings.BrowserName, this.settings.Channel, browserInfo.Version);
        string executablePath = Path.Combine(cachedInstallDirectory, this.settings.ExpectedExecutablePath);

        string cacheLogMessage = $"Using {this.settings.BrowserLocationBehaviorDescription} browser.";
        if (!File.Exists(executablePath) || browserInfo.ForceNewDownload)
        {
            if (browserInfo.ForceNewDownload && Directory.Exists(cachedInstallDirectory))
            {
                Directory.Delete(cachedInstallDirectory, true);
            }

            await this.DownloadAndExtractBrowserAsync(browserInfo, cachedInstallDirectory, executablePath).ConfigureAwait(false);
            cacheLogMessage = $"Downloaded {this.settings.BrowserDisplayName} ready at: {executablePath}";
        }

        if (!this.settings.IncludeDriver)
        {
            // If we are using a browser driver, this has already been logged.
            await this.LogAsync(cacheLogMessage, WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        }

        return executablePath;
    }

    private async Task<Cache.InstalledBrowserInfo> GetInstalledBrowserInfoFromCacheAsync(Cache cacheInfo, BrowserDownloadInfo browserDownloadInfo)
    {
        Cache.InstalledBrowserInfo browserInfo;
        if (cacheInfo.TryGetCachedBrowser(this.settings, out Cache.InstalledBrowserInfo? cachedBrowserInfo))
        {
            browserInfo = cachedBrowserInfo;
        }
        else
        {
            browserInfo = new Cache.InstalledBrowserInfo
            {
                BrowserName = this.settings.BrowserName,
                Channel = this.settings.Channel,
                Version = browserDownloadInfo.Version,
                DirectDownloadUrl = browserDownloadInfo.DownloadUrl,
                ForceNewDownload = browserDownloadInfo.IgnoreVersionMatch,
            };
            cacheInfo.AddBrowserToCache(browserInfo, this.settings.IsLatestChannelVersion);
        }

        if (browserInfo.IsCachedVersionInfoExpired())
        {
            browserInfo.Version = browserDownloadInfo.Version;
            browserInfo.DirectDownloadUrl = browserDownloadInfo.DownloadUrl;
            browserInfo.ForceNewDownload = browserDownloadInfo.IgnoreVersionMatch;
        }

        return browserInfo;
    }

    private async Task DownloadAndExtractBrowserAsync(Cache.InstalledBrowserInfo browserInfo, string cachedInstallDirectory, string expectedExtractedExecutablePath)
    {
        if (!Directory.Exists(cachedInstallDirectory))
        {
            Directory.CreateDirectory(cachedInstallDirectory);
        }

        await this.LogAsync($"Downloading {this.settings.BrowserDisplayName}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        string downloadedInstallerPath = Path.Combine(cachedInstallDirectory, this.settings.InstallerFileName);

        FileDownloader downloader = new();
        downloader.OnDownloadProgress.AddObserver(this.LogFileDownloadProgressAsync);

        using HttpClient client = new();
        await downloader.DownloadFileAsync(client, browserInfo.DirectDownloadUrl, downloadedInstallerPath).ConfigureAwait(false);
        await this.settings.BrowserExtractor.ExtractFileContentsAsync(downloadedInstallerPath, cachedInstallDirectory).ConfigureAwait(false);

        if (!File.Exists(expectedExtractedExecutablePath))
        {
            throw new FileNotFoundException($"{this.settings.BrowserDisplayName} executable not found after extraction at: {expectedExtractedExecutablePath}");
        }

        browserInfo.LastDownload = DateTime.UtcNow;
    }

    private async Task LogFileDownloadProgressAsync(FileDownloadProgressEventArgs args)
    {
        await this.LogAsync($"  Download progress: {args.PercentComplete}%", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }
}
