// <copyright file="DriverLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Handles locating and downloading browser drivers for testing.
/// </summary>
public class DriverLocator
{
    /// <summary>
    /// Gets the the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Driver Locator";

    /// <summary>
    /// Gets the default base cache directory for downloaded drivers, which is a subdirectory of the user's profile directory.
    /// </summary>
    private static readonly string DefaultCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    private readonly BrowserLocatorSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriverLocator"/> class.
    /// </summary>
    /// <param name="settings">The <see cref="BrowserLocatorSettings"/> for the driver locator.</param>
    public DriverLocator(BrowserLocatorSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Gets or sets the directory where downloaded drivers should be cached. By default,
    /// the cache directory is a "webdriverbidi-net" subdirectory of a hidden ".cache"
    /// directory located in the user's profile directory.
    /// </summary>
    public string CacheDirectory { get; set; } = DefaultCacheDir;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the driver locator.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new("driverLocator.logMessage");

    /// <summary>
    /// Locates the driver executable, downloading it if necessary.
    /// </summary>
    /// <param name="cacheInfo">The cache information, or null if cache is not being used.</param>
    /// <returns>The path to the driver executable, or null if driver is not included.</returns>
    internal async Task<string?> LocateDriverAsync(Cache? cacheInfo)
    {
        if (!this.settings.IncludeDriver)
        {
            return null;
        }

        // Check environment variable first
        string? envDriverPath = Environment.GetEnvironmentVariable(this.settings.DriverEnvironmentVariableName);
        if (!string.IsNullOrEmpty(envDriverPath))
        {
            await this.LogAsync($"Using environment variable '{this.settings.DriverEnvironmentVariableName}': {envDriverPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return envDriverPath;
        }

        if (this.settings.DriverLocationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            await this.LogAsync($"Using system-installed {this.settings.DriverExecutableName} from PATH", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return this.settings.DriverExecutableName;
        }

        if (this.settings.DriverLocationBehavior == FileLocationBehavior.UseCustomLocation)
        {
            if (string.IsNullOrEmpty(this.settings.DriverExecutableLocation))
            {
                throw new ArgumentException("DriverExecutableLocation must be set when DriverLocationBehavior is UseCustomLocation.", nameof(this.settings.DriverExecutableLocation));
            }

            await this.LogAsync($"Using custom {this.settings.DriverExecutableName} at: {this.settings.DriverExecutableLocation}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return this.settings.DriverExecutableLocation;
        }

        // If we fall through to this point, the behavior must be AutoLocateAndDownload.
        if (cacheInfo is null)
        {
            throw new InvalidOperationException("Cache should have been loaded for AutoLocateAndDownload behavior.");
        }

        DriverDownloadInfo driverDownloadInfo = await this.settings.GetMatchingDriverDownloadInfo().ConfigureAwait(false);
        Cache.InstalledDriverInfo driverInfo = await this.GetInstalledDriverInfoFromCacheAsync(cacheInfo, driverDownloadInfo).ConfigureAwait(false);
        string driverCacheDirectory = Path.Combine(this.CacheDirectory, "drivers", driverDownloadInfo.DriverName, driverDownloadInfo.Version);
        string executablePath = Path.Combine(driverCacheDirectory, this.settings.DriverExecutableName);

        string cacheLogMessage = $"Using cached {driverDownloadInfo.DriverName}.";
        if (!File.Exists(executablePath))
        {
            await this.DownloadAndExtractDriverAsync(driverInfo, driverCacheDirectory, executablePath).ConfigureAwait(false);
            cacheLogMessage = $"{driverDownloadInfo.DriverName} ready at: {executablePath}";
        }

        await this.LogAsync(cacheLogMessage, WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        return executablePath;
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    internal async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    private async Task<Cache.InstalledDriverInfo> GetInstalledDriverInfoFromCacheAsync(Cache cacheInfo, DriverDownloadInfo driverDownloadInfo)
    {
        Cache.InstalledDriverInfo driverInfo;
        if (cacheInfo.TryGetCachedDriver(driverDownloadInfo.DriverName, driverDownloadInfo.Version, out Cache.InstalledDriverInfo? cachedDriverInfo))
        {
            driverInfo = cachedDriverInfo;
        }
        else
        {
            driverInfo = new Cache.InstalledDriverInfo
            {
                DriverName = driverDownloadInfo.DriverName,
                Version = driverDownloadInfo.Version,
                BrowserVersion = driverDownloadInfo.BrowserVersion,
                DirectDownloadUrl = driverDownloadInfo.DownloadUrl,
            };
            cacheInfo.AddDriverToCache(driverInfo);
        }

        if (driverInfo.IsCachedVersionInfoExpired())
        {
            driverInfo.Version = driverDownloadInfo.Version;
            driverInfo.DirectDownloadUrl = driverDownloadInfo.DownloadUrl;
            driverInfo.BrowserVersion = driverDownloadInfo.BrowserVersion;
        }

        return driverInfo;
    }

    private async Task DownloadAndExtractDriverAsync(Cache.InstalledDriverInfo driverInfo, string cachedInstallDirectory, string expectedExtractedExecutablePath)
    {
        if (!Directory.Exists(cachedInstallDirectory))
        {
            Directory.CreateDirectory(cachedInstallDirectory);
        }

        await this.LogAsync($"Downloading {driverInfo.DriverName}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);

        // Parse the download URL to extract the filename (without query string)
        Uri downloadUri = new(driverInfo.DirectDownloadUrl);
        string installerFileName = Path.GetFileName(downloadUri.LocalPath);

        string downloadedInstallerPath = Path.Combine(cachedInstallDirectory, installerFileName);

        FileDownloader downloader = new();
        downloader.OnDownloadProgress.AddObserver(this.LogFileDownloadProgressAsync);

        using HttpClient client = new();
        await downloader.DownloadFileAsync(client, driverInfo.DirectDownloadUrl, downloadedInstallerPath).ConfigureAwait(false);
        await this.settings.DriverExtractor.ExtractFileContentsAsync(downloadedInstallerPath, cachedInstallDirectory).ConfigureAwait(false);

        // Driver executables might be in a subdirectory, try to find them
        if (!File.Exists(expectedExtractedExecutablePath))
        {
            string[] foundDrivers = Directory.GetFiles(cachedInstallDirectory, this.settings.DriverExecutableName, SearchOption.AllDirectories);
            if (foundDrivers.Length > 0)
            {
                // Move the driver executable to the expected location
                File.Move(foundDrivers[0], expectedExtractedExecutablePath);
            }
        }

        if (!File.Exists(expectedExtractedExecutablePath))
        {
            throw new FileNotFoundException($"{driverInfo.DriverName} executable not found after extraction at: {expectedExtractedExecutablePath}");
        }

        driverInfo.LastDownload = DateTime.UtcNow;
    }

    private async Task LogFileDownloadProgressAsync(FileDownloadProgressEventArgs args)
    {
        await this.LogAsync($"  Download progress: {args.PercentComplete}%", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }
}
