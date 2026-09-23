// <copyright file="DriverLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Handles locating and downloading browser drivers for testing.
/// </summary>
public class DriverLocator
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Driver Locator";

    private readonly ObservableEventInvocable<LogMessageEventArgs> invocableLogMessageObservableEvent = new("driverLocator.logMessage");
    private readonly BrowserLocatorSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriverLocator"/> class.
    /// </summary>
    /// <param name="settings">The <see cref="BrowserLocatorSettings"/> for the driver locator.</param>
    internal DriverLocator(BrowserLocatorSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Gets the directory where downloaded drivers should be cached. By default,
    /// the cache directory is a "webdriverbidi-net" subdirectory of a hidden ".cache"
    /// directory located in the user's profile directory.
    /// </summary>
    public string CacheDirectory => this.settings.CacheDirectory;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the driver locator.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.invocableLogMessageObservableEvent;

    /// <summary>
    /// Finds the driver executable using default settings (Stable channel, Latest version, AutoLocateAndDownload).
    /// </summary>
    /// <param name="browser">The browser for which to locate the driver.</param>
    /// <returns>The path to the driver executable.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    public static Task<string?> FindDriverAsync(BrowserKind browser)
    {
        return FindDriverAsync(browser, BrowserReleaseChannel.Stable);
    }

    /// <summary>
    /// Finds the driver executable with the specified release channel using default settings (Latest version, AutoLocateAndDownload).
    /// </summary>
    /// <param name="browser">The browser for which to locate the driver.</param>
    /// <param name="channel">The release channel of the driver.</param>
    /// <returns>The path to the driver executable.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    public static Task<string?> FindDriverAsync(BrowserKind browser, BrowserReleaseChannel channel)
    {
        return FindDriverAsync(browser, channel, BrowserVersion.Latest);
    }

    /// <summary>
    /// Finds the driver executable with the specified release channel and version using default settings (AutoLocateAndDownload).
    /// </summary>
    /// <param name="browser">The browser for which to locate the driver.</param>
    /// <param name="channel">The release channel of the driver.</param>
    /// <param name="version">The version of the driver to locate (typically matches browser version).</param>
    /// <returns>The path to the driver executable.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    public static Task<string?> FindDriverAsync(BrowserKind browser, BrowserReleaseChannel channel, BrowserVersion version)
    {
        return FindDriverAsync(browser, channel, version, FileLocationBehavior.AutoLocateAndDownload);
    }

    /// <summary>
    /// Finds the driver executable with full control over all location settings.
    /// </summary>
    /// <param name="browser">The browser for which to locate the driver.</param>
    /// <param name="channel">The release channel of the driver.</param>
    /// <param name="version">The version of the driver to locate (typically matches browser version).</param>
    /// <param name="locationBehavior">The strategy for locating the driver.</param>
    /// <param name="customPath">The custom path to the driver executable (only used when locationBehavior is UseCustomLocation).</param>
    /// <param name="downloadOptions">The options controlling where the driver is cached and downloaded from, or <see langword="null"/> for the defaults.</param>
    /// <returns>The path to the driver executable, or null if not found.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    /// <exception cref="ArgumentException">Thrown when customPath is required but not provided.</exception>
    public static async Task<string?> FindDriverAsync(
        BrowserKind browser,
        BrowserReleaseChannel channel,
        BrowserVersion version,
        FileLocationBehavior locationBehavior,
        string? customPath = null,
        BrowserDownloadOptions? downloadOptions = null)
    {
        downloadOptions ??= new BrowserDownloadOptions();
        BrowserLocatorSettings settings = browser switch
        {
            BrowserKind.Chrome => BrowserLocator.CreateChromeSettings(channel, version, locationBehavior, customPath, downloadOptions),
            BrowserKind.Firefox => BrowserLocator.CreateFirefoxSettings(channel, version, locationBehavior, customPath, downloadOptions),
            BrowserKind.Edge => throw new NotImplementedException(
                "Microsoft Edge driver support is not yet implemented. Currently supported browsers: Chrome, Firefox. " +
                "Edge support is planned for a future release."),
            BrowserKind.Safari => throw new NotImplementedException(
                "Apple Safari driver support is not yet implemented. Currently supported browsers: Chrome, Firefox. " +
                "Safari support is planned for a future release pending maturity of Safari's BiDi implementation."),
            _ => throw new ArgumentException($"Unknown browser: {browser}", nameof(browser)),
        };

        settings.IncludeDriver = true;
        DriverLocator locator = new(settings);
        return await locator.LocateDriverAsync(null).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates the driver executable, downloading it if necessary.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <returns>The path to the driver executable, or null if driver is not included.</returns>
    internal async Task<string?> LocateDriverAsync(string? browserVersion)
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

        InstallCache cache = new(Path.Combine(this.CacheDirectory, "drivers", this.settings.DriverName), this.settings.DownloadOptions);
        using IDisposable lockHandle = await cache.LockAsync().ConfigureAwait(false);
        string relativeExecutablePath = this.settings.DriverExecutableName;
        string? requiredVersion = this.settings.GetRequiredDriverVersion(browserVersion);
        if (requiredVersion is not null)
        {
            if (cache.TryGetInstalledExecutable(requiredVersion, relativeExecutablePath, out string? installedPath))
            {
                await this.LogAsync($"Using cached {this.settings.DriverName} {requiredVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
                return installedPath;
            }

            DriverDownloadInfo requiredDownloadInfo = await this.settings.GetMatchingDriverDownloadInfo(browserVersion).ConfigureAwait(false);
            return await this.InstallDriverAsync(cache, requiredDownloadInfo).ConfigureAwait(false);
        }

        string request = this.settings.DriverVersionRequest;
        string? cachedPath = null;
        bool isResolvedVersionInstalled = cache.TryGetResolvedVersion(request, out string? resolvedVersion, out bool isFresh)
            && cache.TryGetInstalledExecutable(resolvedVersion, relativeExecutablePath, out cachedPath);
        if (isResolvedVersionInstalled && isFresh)
        {
            await this.LogAsync($"Using cached {this.settings.DriverName} {resolvedVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return cachedPath;
        }

        DriverDownloadInfo downloadInfo;
        try
        {
            downloadInfo = await this.settings.GetMatchingDriverDownloadInfo(browserVersion).ConfigureAwait(false);
        }
        catch (Exception ex) when (isResolvedVersionInstalled && BrowserLocator.IsUnreachableServiceException(ex))
        {
            await this.LogAsync($"Could not check for a newer {this.settings.DriverName} ({ex.Message}); using cached version {resolvedVersion}.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return cachedPath;
        }

        string executablePath = cache.TryGetInstalledExecutable(downloadInfo.Version, relativeExecutablePath, out string? currentPath)
            ? currentPath
            : await this.InstallDriverAsync(cache, downloadInfo).ConfigureAwait(false);
        cache.SaveResolvedVersion(request, downloadInfo.Version);
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
        await this.invocableLogMessageObservableEvent.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    private async Task<string> InstallDriverAsync(InstallCache cache, DriverDownloadInfo downloadInfo)
    {
        await this.LogAsync($"Downloading {downloadInfo.DriverName} {downloadInfo.Version}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.DriverExecutableName;
        string executablePath = await cache.InstallAsync(downloadInfo.Version, relativeExecutablePath, async installDirectory =>
        {
            // The installer is named for the download URL's file name, without any query string.
            string installerPath = Path.Combine(installDirectory, Path.GetFileName(new Uri(downloadInfo.DownloadUrl).LocalPath));
            FileDownloader downloader = new();
            downloader.OnDownloadProgress.AddObserver(this.LogFileDownloadProgressAsync);
            await downloader.DownloadFileAsync(DownloadHttpClient.GetClient(this.settings.DownloadOptions), downloadInfo.DownloadUrl, installerPath).ConfigureAwait(false);
            await this.settings.DriverExtractor.ExtractFileContentsAsync(installerPath, installDirectory).ConfigureAwait(false);

            // Driver executables might be in a subdirectory of the archive.
            string expectedPath = Path.Combine(installDirectory, relativeExecutablePath);
            if (!File.Exists(expectedPath))
            {
                string[] foundDrivers = Directory.GetFiles(installDirectory, relativeExecutablePath, SearchOption.AllDirectories);
                if (foundDrivers.Length == 0)
                {
                    throw new FileNotFoundException($"{downloadInfo.DriverName} executable not found after extraction at: {relativeExecutablePath}");
                }

                File.Move(foundDrivers[0], expectedPath);
            }
        }).ConfigureAwait(false);

        await this.LogAsync($"{downloadInfo.DriverName} ready at: {executablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        return executablePath;
    }

    private async Task LogFileDownloadProgressAsync(FileDownloadProgressEventArgs args)
    {
        await this.LogAsync($"  Download progress: {args.PercentComplete}%", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }
}
