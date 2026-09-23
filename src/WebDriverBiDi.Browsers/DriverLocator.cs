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
    /// Finds the driver executable, downloading it if necessary.
    /// </summary>
    /// <param name="browser">The browser for which to locate the driver.</param>
    /// <param name="channel">The release channel of the browser the driver drives.</param>
    /// <param name="version">The version of the browser the driver drives, or <see langword="null"/> for <see cref="BrowserVersion.Latest"/>.</param>
    /// <param name="locationBehavior">The strategy for locating the browser.</param>
    /// <param name="customPath">The custom path to the browser executable (only used when locationBehavior is UseCustomLocation).</param>
    /// <param name="downloadOptions">The options controlling where the driver is cached and downloaded from, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels locating the driver.</param>
    /// <returns>The path to the driver executable, or null if not found.</returns>
    /// <exception cref="NotSupportedException">Thrown when the specified browser's driver cannot be located by this method, or not on this platform.</exception>
    /// <exception cref="ArgumentException">Thrown when customPath is required but not provided.</exception>
    /// <exception cref="BrowserDownloadException">Thrown when the driver cannot be located or downloaded.</exception>
    public static async Task<string?> FindDriverAsync(
        BrowserKind browser,
        BrowserReleaseChannel channel = BrowserReleaseChannel.Stable,
        BrowserVersion? version = null,
        FileLocationBehavior locationBehavior = FileLocationBehavior.AutoLocateAndDownload,
        string? customPath = null,
        BrowserDownloadOptions? downloadOptions = null,
        CancellationToken cancellationToken = default)
    {
        downloadOptions ??= new BrowserDownloadOptions();
        version ??= BrowserVersion.Latest;
        BrowserLocatorSettings settings = browser switch
        {
            BrowserKind.Chrome => BrowserLocator.CreateChromeSettings(channel, version, locationBehavior, customPath, downloadOptions),
            BrowserKind.Firefox => BrowserLocator.CreateFirefoxSettings(channel, version, locationBehavior, customPath, downloadOptions),
            BrowserKind.Safari => BrowserLocator.CreateSafariSettings(channel, version, locationBehavior, customPath, downloadOptions),
            _ => throw new NotSupportedException($"The driver for {browser} cannot be located; this method supports Chrome, Firefox, and Safari."),
        };

        settings.IncludeDriver = true;
        DriverLocator locator = new(settings);
        return await locator.LocateDriverAsync(null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates the driver executable, downloading it if necessary.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <param name="cancellationToken">A token that cancels locating the driver.</param>
    /// <returns>The path to the driver executable, or null if driver is not included.</returns>
    internal async Task<string?> LocateDriverAsync(string? browserVersion, CancellationToken cancellationToken)
    {
        if (!this.settings.IncludeDriver)
        {
            return null;
        }

        // Check environment variable first
        string? envDriverPath = LauncherEnvironment.GetVariable(this.settings.DriverEnvironmentVariableName);
        if (envDriverPath is not null)
        {
            await this.LogAsync($"Using environment variable '{this.settings.DriverEnvironmentVariableName}': {envDriverPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return envDriverPath;
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

        try
        {
            return await this.LocateCachedDriverAsync(browserVersion, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not WebDriverBiDiException && !(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            throw new BrowserDownloadException($"Unable to locate or download {this.settings.DriverName}: {ex.Message}", ex);
        }
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

    private async Task<string> LocateCachedDriverAsync(string? browserVersion, CancellationToken cancellationToken)
    {
        InstallCache cache = new(Path.Combine(this.CacheDirectory, "drivers", this.settings.DriverName), this.settings.DownloadOptions);
        using IDisposable lockHandle = await cache.LockAsync(cancellationToken).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.DriverExecutableName;
        string? requiredVersion = this.settings.GetRequiredDriverVersion(browserVersion);
        if (requiredVersion is not null)
        {
            if (cache.TryGetInstalledExecutable(requiredVersion, relativeExecutablePath, out string? installedPath))
            {
                await this.LogAsync($"Using cached {this.settings.DriverName} {requiredVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
                return installedPath;
            }

            this.ThrowIfDownloadSkipped($"{this.settings.DriverName} {requiredVersion}");
            DriverDownloadInfo requiredDownloadInfo = await this.settings.GetMatchingDriverDownloadInfo(browserVersion, cancellationToken).ConfigureAwait(false);
            return await this.InstallDriverAsync(cache, requiredDownloadInfo, cancellationToken).ConfigureAwait(false);
        }

        string request = this.settings.DriverVersionRequest;
        string? cachedPath = null;
        bool isResolvedVersionInstalled = cache.TryGetResolvedVersion(request, out string? resolvedVersion, out bool isFresh)
            && cache.TryGetInstalledExecutable(resolvedVersion, relativeExecutablePath, out cachedPath);
        if (isResolvedVersionInstalled && (isFresh || this.settings.DownloadOptions.SkipDownload))
        {
            await this.LogAsync($"Using cached {this.settings.DriverName} {resolvedVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return cachedPath!;
        }

        this.ThrowIfDownloadSkipped(this.settings.DriverName);
        DriverDownloadInfo downloadInfo;
        try
        {
            downloadInfo = await this.settings.GetMatchingDriverDownloadInfo(browserVersion, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (isResolvedVersionInstalled && BrowserLocator.IsUnreachableServiceException(ex) && !cancellationToken.IsCancellationRequested)
        {
            await this.LogAsync($"Could not check for a newer {this.settings.DriverName} ({ex.Message}); using cached version {resolvedVersion}.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return cachedPath!;
        }

        string executablePath = cache.TryGetInstalledExecutable(downloadInfo.Version, relativeExecutablePath, out string? currentPath)
            ? currentPath
            : await this.InstallDriverAsync(cache, downloadInfo, cancellationToken).ConfigureAwait(false);
        cache.SaveResolvedVersion(request, downloadInfo.Version);
        return executablePath;
    }

    private void ThrowIfDownloadSkipped(string description)
    {
        if (this.settings.DownloadOptions.SkipDownload)
        {
            throw new BrowserDownloadException($"{description} is not installed in {this.CacheDirectory}, and downloads are disabled by {nameof(BrowserDownloadOptions.SkipDownload)} (or {LauncherEnvironment.SkipDownloadVariableName}).");
        }
    }

    private async Task<string> InstallDriverAsync(InstallCache cache, DriverDownloadInfo downloadInfo, CancellationToken cancellationToken)
    {
        string name = $"{downloadInfo.DriverName} {downloadInfo.Version}";
        await this.LogAsync($"Downloading {name}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.DriverExecutableName;
        string executablePath = await cache.InstallAsync(downloadInfo.Version, relativeExecutablePath, async installDirectory =>
        {
            // The installer is named for the download URL's file name, without any query string.
            string installerPath = Path.Combine(installDirectory, Path.GetFileName(new Uri(downloadInfo.DownloadUrl).LocalPath));
            FileDownloader downloader = new(name, this.settings.DownloadOptions.Progress, message => this.LogAsync(message, WebDriverBiDiLogLevel.Info));
            await downloader.DownloadFileAsync(DownloadHttpClient.GetClient(this.settings.DownloadOptions), downloadInfo.DownloadUrl, installerPath, downloadInfo.Sha256, downloadInfo.Size, cancellationToken).ConfigureAwait(false);
            await this.settings.DriverExtractor.ExtractFileContentsAsync(installerPath, installDirectory, cancellationToken).ConfigureAwait(false);

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
}
