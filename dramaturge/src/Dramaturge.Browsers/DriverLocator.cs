// <copyright file="DriverLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using WebDriverBiDi;

/// <summary>
/// Handles locating and downloading browser drivers for testing.
/// </summary>
public class DriverLocator
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Driver Locator";

    // Prefixes the version request that records which driver matches a required version.
    private const string MatchRequestPrefix = "match-";

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
    /// the cache directory is a "dramaturge" subdirectory of a hidden ".cache"
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
        BrowserLocatorSettings settings = BrowserLocator.CreateSettings(browser, channel, version, locationBehavior, customPath, downloadOptions);
        settings.IncludeDriver = true;
        if (settings.LocationBehavior != FileLocationBehavior.AutoLocateAndDownload)
        {
            // A driver that must match an installed browser takes the version read when the browser is located.
            BrowserExecutableInfo executables = await new BrowserLocator(settings).LocateExecutablesAsync(cancellationToken).ConfigureAwait(false);
            return executables.DriverPath;
        }

        DriverLocator locator = new(settings);
        return await locator.LocateDriverAsync(null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the build of a driver that <see cref="FindDriverAsync"/> would download, without downloading it. The
    /// driver for a browser that is not downloaded, such as msedgedriver, matches the version of the installed browser.
    /// </summary>
    /// <param name="browser">The browser whose driver to resolve: Chrome, Firefox, or Edge.</param>
    /// <param name="channel">The release channel of the browser the driver drives.</param>
    /// <param name="version">The version of the browser the driver drives, or <see langword="null"/> for <see cref="BrowserVersion.Latest"/>.</param>
    /// <param name="locationBehavior">The strategy for locating the browser.</param>
    /// <param name="customPath">The custom path to the browser executable (only used when locationBehavior is UseCustomLocation).</param>
    /// <param name="downloadOptions">The options controlling where the driver is cached and downloaded from, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels resolving the build.</param>
    /// <returns>The build.</returns>
    /// <exception cref="NotSupportedException">Thrown when the driver is never downloaded, as safaridriver is not.</exception>
    /// <exception cref="ArgumentException">Thrown when the channel or version cannot be used with the browser, or customPath is required but not provided.</exception>
    /// <exception cref="BrowserDownloadException">Thrown when the build cannot be resolved.</exception>
    public static async Task<ResolvedDownload> ResolveDownloadAsync(
        BrowserKind browser,
        BrowserReleaseChannel channel = BrowserReleaseChannel.Stable,
        BrowserVersion? version = null,
        FileLocationBehavior locationBehavior = FileLocationBehavior.AutoLocateAndDownload,
        string? customPath = null,
        BrowserDownloadOptions? downloadOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (browser == BrowserKind.Safari)
        {
            throw new NotSupportedException("safaridriver is part of macOS, and is never downloaded.");
        }

        downloadOptions ??= new BrowserDownloadOptions();
        BrowserLocatorSettings settings = BrowserLocator.CreateSettings(browser, channel, version ?? BrowserVersion.Latest, locationBehavior, customPath, downloadOptions);
        settings.IncludeDriver = true;
        DriverLocator locator = new(settings);
        DriverDownloadInfo downloadInfo;
        try
        {
            string? browserVersion = settings.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload
                ? null
                : await new BrowserLocator(settings).LocateBrowserVersionAsync(cancellationToken).ConfigureAwait(false);
            downloadInfo = await locator.GetDriverDownloadInfoAsync((IDriverDownloadSource)settings, browserVersion, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not WebDriverBiDiException && !(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            throw new BrowserDownloadException($"Unable to resolve {settings.DriverName}: {ex.Message}", ex);
        }

        InstallCache cache = new(Path.Combine(locator.CacheDirectory, "drivers", settings.DriverName), downloadOptions);
        bool isCached = cache.TryGetInstalledExecutable(downloadInfo.Version, settings.DriverExecutableName, out _);
        return new ResolvedDownload(settings.DriverName, downloadInfo.Version, new Uri(downloadInfo.DownloadUrl), isCached);
    }

    /// <summary>
    /// Locates the driver executable, downloading it if necessary.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <param name="cancellationToken">A token that cancels locating the driver.</param>
    /// <returns>The path to the driver executable.</returns>
    internal async Task<string?> LocateDriverAsync(string? browserVersion, CancellationToken cancellationToken)
    {
        // Check environment variable first
        string? envDriverPath = LauncherEnvironment.GetVariable(this.settings.DriverEnvironmentVariableName);
        if (envDriverPath is not null)
        {
            await this.LogAsync($"Using environment variable '{this.settings.DriverEnvironmentVariableName}': {envDriverPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return envDriverPath;
        }

        if (this.settings is not IDriverDownloadSource downloadSource)
        {
            await this.LogAsync($"Using {this.settings.DriverExecutableName} at: {this.settings.DriverExecutableLocation}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return this.settings.DriverExecutableLocation;
        }

        try
        {
            return await this.LocateCachedDriverAsync(downloadSource, browserVersion, cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Gets the download of the driver, for the given version of the browser it drives.
    /// </summary>
    /// <param name="downloadSource">The settings, as the source of the driver's download.</param>
    /// <param name="browserVersion">The version of the browser, or <see langword="null"/> if it is not known.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The driver download information.</returns>
    internal async Task<DriverDownloadInfo> GetDriverDownloadInfoAsync(IDriverDownloadSource downloadSource, string? browserVersion, CancellationToken cancellationToken)
    {
        if (this.settings.DownloadOptions.ManifestUrl is null)
        {
            return await downloadSource.GetMatchingDriverDownloadInfo(browserVersion, cancellationToken).ConfigureAwait(false);
        }

        DownloadManifest manifest = await DownloadManifest.LoadAsync(this.settings.DownloadOptions, cancellationToken).ConfigureAwait(false);
        return manifest.ResolveDriver(this.settings, browserVersion);
    }

    private async Task<string> LocateCachedDriverAsync(IDriverDownloadSource downloadSource, string? browserVersion, CancellationToken cancellationToken)
    {
        InstallCache cache = new(Path.Combine(this.CacheDirectory, "drivers", this.settings.DriverName), this.settings.DownloadOptions);
        using IDisposable lockHandle = await cache.LockAsync(cancellationToken).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.DriverExecutableName;
        string? requiredVersion = this.settings.GetRequiredDriverVersion(browserVersion);
        if (requiredVersion is not null)
        {
            return await this.LocateMatchingDriverAsync(downloadSource, cache, requiredVersion, browserVersion, cancellationToken).ConfigureAwait(false);
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
            downloadInfo = await this.GetDriverDownloadInfoAsync(downloadSource, browserVersion, cancellationToken).ConfigureAwait(false);
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

    // A driver of another version may be the match for the required one, and is then recorded as its match, rechecked
    // as a resolved version is, so that it is found without a network request.
    private async Task<string> LocateMatchingDriverAsync(IDriverDownloadSource downloadSource, InstallCache cache, string requiredVersion, string? browserVersion, CancellationToken cancellationToken)
    {
        string relativeExecutablePath = this.settings.DriverExecutableName;
        if (cache.TryGetInstalledExecutable(requiredVersion, relativeExecutablePath, out string? installedPath))
        {
            await this.LogAsync($"Using cached {this.settings.DriverName} {requiredVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return installedPath;
        }

        string matchRequest = $"{MatchRequestPrefix}{requiredVersion}";
        string? matchedPath = null;
        bool isMatchInstalled = cache.TryGetResolvedVersion(matchRequest, out string? matchedVersion, out bool isFresh)
            && cache.TryGetInstalledExecutable(matchedVersion, relativeExecutablePath, out matchedPath);
        if (isMatchInstalled && (isFresh || this.settings.DownloadOptions.SkipDownload))
        {
            await this.LogAsync($"Using cached {this.settings.DriverName} {matchedVersion}, which matches {requiredVersion}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return matchedPath!;
        }

        this.ThrowIfDownloadSkipped($"{this.settings.DriverName} {requiredVersion}");
        DriverDownloadInfo downloadInfo;
        try
        {
            downloadInfo = await this.GetDriverDownloadInfoAsync(downloadSource, browserVersion, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (isMatchInstalled && BrowserLocator.IsUnreachableServiceException(ex) && !cancellationToken.IsCancellationRequested)
        {
            await this.LogAsync($"Could not check for a closer {this.settings.DriverName} ({ex.Message}); using cached version {matchedVersion}.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return matchedPath!;
        }

        string executablePath = cache.TryGetInstalledExecutable(downloadInfo.Version, relativeExecutablePath, out string? currentPath)
            ? currentPath
            : await this.InstallDriverAsync(cache, downloadInfo, cancellationToken).ConfigureAwait(false);
        if (downloadInfo.Version != requiredVersion)
        {
            await this.LogAsync($"No {this.settings.DriverName} {requiredVersion} is published; using {downloadInfo.Version}.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            cache.SaveResolvedVersion(matchRequest, downloadInfo.Version);
        }

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
