// <copyright file="BrowserLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Provides methods for locating and downloading browsers for testing.
/// </summary>
public class BrowserLocator
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Locator";

    private readonly ObservableEventInvocable<LogMessageEventArgs> invocableLogMessageObservableEvent = new("browserLocator.logMessage");
    private readonly BrowserLocatorSettings settings;
    private readonly DriverLocator? driverLocator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLocator"/> class.
    /// </summary>
    /// <param name="settings">The <see cref="BrowserLocatorSettings"/> for the browser locator used by the launcher.</param>
    /// <param name="driverLocator">Optional <see cref="DriverLocator"/> to use for locating driver executables. If null and settings.IncludeDriver is true, a new instance will be created.</param>
    internal BrowserLocator(BrowserLocatorSettings settings, DriverLocator? driverLocator = null)
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
            this.driverLocator.OnLogMessage.AddObserver(this.invocableLogMessageObservableEvent.InvokeNotifyObserversAsync);
        }
    }

    /// <summary>
    /// Getsthe directory where downloaded browsers should be cached. By default,
    /// the cache directory is a "webdriverbidi-net" subdirectory of a hidden ".cache"
    /// directory located in the user's profile directory.
    /// </summary>
    public string CacheDirectory
    {
        get => this.settings.CacheDirectory;
    }

    /// <summary>
    /// Gets the name of the browser being located (e.g., "Chrome", "Firefox").
    /// </summary>
    public string BrowserName => this.settings.BrowserName;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser locator.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.invocableLogMessageObservableEvent;

    /// <summary>
    /// Finds the browser executable, downloading it if necessary.
    /// </summary>
    /// <param name="browser">The browser to locate.</param>
    /// <param name="channel">The release channel of the browser.</param>
    /// <param name="version">The version of the browser to locate, or <see langword="null"/> for <see cref="BrowserVersion.Latest"/>.</param>
    /// <param name="locationBehavior">The strategy for locating the browser.</param>
    /// <param name="customPath">The custom path to the browser executable (only used when locationBehavior is UseCustomLocation).</param>
    /// <param name="downloadOptions">The options controlling where the browser is cached and downloaded from, or <see langword="null"/> for the defaults.</param>
    /// <param name="browserOptions">Browser-specific options that affect which executable is located, such as <see cref="ChromeLaunchOptions.UseHeadlessShell"/>, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">A token that cancels locating the browser.</param>
    /// <returns>The path to the browser executable.</returns>
    /// <exception cref="NotSupportedException">Thrown when the specified browser cannot be located by this method, or not on this platform.</exception>
    /// <exception cref="ArgumentException">Thrown when customPath is required but not provided, or the channel, version, or browser options cannot be used with the browser.</exception>
    /// <exception cref="BrowserDownloadException">Thrown when the browser cannot be located or downloaded.</exception>
    public static async Task<string> FindBrowserAsync(
        BrowserKind browser,
        BrowserReleaseChannel channel = BrowserReleaseChannel.Stable,
        BrowserVersion? version = null,
        FileLocationBehavior locationBehavior = FileLocationBehavior.AutoLocateAndDownload,
        string? customPath = null,
        BrowserDownloadOptions? downloadOptions = null,
        BrowserLaunchOptions? browserOptions = null,
        CancellationToken cancellationToken = default)
    {
        downloadOptions ??= new BrowserDownloadOptions();
        version ??= BrowserVersion.Latest;
        if (browserOptions is not null && browserOptions.Browser != browser)
        {
            throw new ArgumentException($"{browserOptions.GetType().Name} cannot be used to locate {browser}.", nameof(browserOptions));
        }

        BrowserLocatorSettings settings = browser switch
        {
            BrowserKind.Chrome => CreateChromeSettings(channel, version, locationBehavior, customPath, downloadOptions, browserOptions is ChromeLaunchOptions { UseHeadlessShell: true }),
            BrowserKind.Firefox => CreateFirefoxSettings(channel, version, locationBehavior, customPath, downloadOptions),
            BrowserKind.Safari => CreateSafariSettings(channel, version, locationBehavior, customPath, downloadOptions),
            _ => throw new NotSupportedException($"{browser} cannot be located; this method supports Chrome, Firefox, and Safari."),
        };

        BrowserLocator locator = new(settings);
        return await locator.LocateBrowserAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the location information for the browser executable and optionally the driver executable,
    /// downloading them if necessary. This method minimizes network calls by fetching both from a
    /// single API call when possible (Chrome) or coordinating two calls and caching them together (Firefox).
    /// </summary>
    /// <param name="cancellationToken">A token that cancels locating the executables.</param>
    /// <returns>A <see cref="BrowserExecutableInfo"/> containing the browser path and optionally the driver path.</returns>
    /// <exception cref="BrowserDownloadException">Thrown when the browser or driver cannot be located or downloaded.</exception>
    public async Task<BrowserExecutableInfo> LocateExecutablesAsync(CancellationToken cancellationToken = default)
    {
        LocatedBrowser browser = await this.LocateBrowserPathAsync(cancellationToken).ConfigureAwait(false);
        string? driverPath = null;
        if (this.driverLocator is not null)
        {
            driverPath = await this.driverLocator.LocateDriverAsync(browser.Version, cancellationToken).ConfigureAwait(false);
        }

        return new BrowserExecutableInfo(browser.Path, driverPath);
    }

    /// <summary>
    /// Gets the path to the browser executable, downloading it if necessary.
    /// If the browser-specific environment variable is set, returns that path directly.
    /// This is a convenience method that calls <see cref="LocateExecutablesAsync"/> and returns only the browser path.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels locating the browser.</param>
    /// <returns>The path to the browser executable.</returns>
    /// <exception cref="BrowserDownloadException">Thrown when the browser cannot be located or downloaded.</exception>
    public async Task<string> LocateBrowserAsync(CancellationToken cancellationToken = default)
    {
        LocatedBrowser browser = await this.LocateBrowserPathAsync(cancellationToken).ConfigureAwait(false);
        return browser.Path;
    }

    /// <summary>
    /// Creates browser locator settings for Chrome.
    /// </summary>
    /// <param name="channel">The release channel.</param>
    /// <param name="version">The browser version.</param>
    /// <param name="locationBehavior">The location behavior strategy.</param>
    /// <param name="customPath">Optional custom path to the browser executable.</param>
    /// <param name="downloadOptions">The options controlling where the browser is cached and downloaded from.</param>
    /// <param name="useHeadlessShell">A value indicating whether to locate chrome-headless-shell rather than Chrome.</param>
    /// <returns>Configured Chrome browser locator settings.</returns>
    /// <exception cref="ArgumentException">Thrown when customPath is required but not provided.</exception>
    internal static BrowserLocatorSettings CreateChromeSettings(
        BrowserReleaseChannel channel,
        BrowserVersion version,
        FileLocationBehavior locationBehavior,
        string? customPath,
        BrowserDownloadOptions downloadOptions,
        bool useHeadlessShell = false)
    {
        ChromeChannel chromeChannel = channel switch
        {
            BrowserReleaseChannel.Stable => ChromeChannel.Stable,
            BrowserReleaseChannel.Beta => ChromeChannel.Beta,
            BrowserReleaseChannel.DeveloperPreview => ChromeChannel.Dev,
            BrowserReleaseChannel.Alpha => ChromeChannel.Canary,
            _ => throw new ArgumentException($"Invalid browser release channel for Chrome: {channel}", nameof(channel)),
        };

        string versionString = version.Value;
        string browserLocation = customPath ?? string.Empty;

        if (locationBehavior == FileLocationBehavior.UseCustomLocation && string.IsNullOrWhiteSpace(customPath))
        {
            throw new ArgumentException("customPath must be provided when locationBehavior is UseCustomLocation.", nameof(customPath));
        }

        return new ChromeBrowserLocatorSettings(chromeChannel, locationBehavior, downloadOptions, browserLocation, versionString, useHeadlessShell);
    }

    /// <summary>
    /// Creates browser locator settings for Firefox.
    /// </summary>
    /// <param name="channel">The release channel.</param>
    /// <param name="version">The browser version.</param>
    /// <param name="locationBehavior">The location behavior strategy.</param>
    /// <param name="customPath">Optional custom path to the browser executable.</param>
    /// <param name="downloadOptions">The options controlling where the browser is cached and downloaded from.</param>
    /// <returns>Configured Firefox browser locator settings.</returns>
    /// <exception cref="ArgumentException">Thrown when customPath is required but not provided.</exception>
    internal static BrowserLocatorSettings CreateFirefoxSettings(
        BrowserReleaseChannel channel,
        BrowserVersion version,
        FileLocationBehavior locationBehavior,
        string? customPath,
        BrowserDownloadOptions downloadOptions)
    {
        FirefoxChannel firefoxChannel = channel switch
        {
            BrowserReleaseChannel.Stable => FirefoxChannel.Stable,
            BrowserReleaseChannel.Beta => FirefoxChannel.Beta,
            BrowserReleaseChannel.DeveloperPreview => FirefoxChannel.Dev,
            BrowserReleaseChannel.Alpha => FirefoxChannel.Nightly,
            BrowserReleaseChannel.ExtendedSupport => FirefoxChannel.Esr,
            _ => throw new ArgumentException($"Invalid browser release channel for Firefox: {channel}", nameof(channel)),
        };

        string versionString = version.Value;
        string browserLocation = customPath ?? string.Empty;

        if (locationBehavior == FileLocationBehavior.UseCustomLocation && string.IsNullOrWhiteSpace(customPath))
        {
            throw new ArgumentException("customPath must be provided when locationBehavior is UseCustomLocation.", nameof(customPath));
        }

        return new FirefoxBrowserLocatorSettings(firefoxChannel, locationBehavior, downloadOptions, browserLocation, versionString);
    }

    /// <summary>
    /// Creates browser locator settings for Safari, which is never downloaded: the system
    /// installation, or the executable at a custom location, is used.
    /// </summary>
    /// <param name="channel">The release channel.</param>
    /// <param name="version">The browser version, which must be the latest or the system-installed version.</param>
    /// <param name="locationBehavior">The location behavior strategy.</param>
    /// <param name="customPath">Optional custom path to the browser executable.</param>
    /// <param name="downloadOptions">The download options, whose platform must be macOS.</param>
    /// <returns>Configured Safari browser locator settings.</returns>
    /// <exception cref="NotSupportedException">Thrown when the platform is not macOS.</exception>
    /// <exception cref="ArgumentException">Thrown when the channel or version cannot be used with Safari, or customPath is required but not provided.</exception>
    internal static BrowserLocatorSettings CreateSafariSettings(
        BrowserReleaseChannel channel,
        BrowserVersion version,
        FileLocationBehavior locationBehavior,
        string? customPath,
        BrowserDownloadOptions downloadOptions)
    {
        if (downloadOptions.ResolvedPlatform.OperatingSystem != OperatingSystemFamily.MacOS)
        {
            throw new NotSupportedException($"Safari is available only on macOS, not {downloadOptions.ResolvedPlatform}.");
        }

        SafariChannel safariChannel = channel switch
        {
            BrowserReleaseChannel.Stable => SafariChannel.Stable,
            BrowserReleaseChannel.DeveloperPreview => SafariChannel.TechnologyPreview,
            _ => throw new ArgumentException($"Invalid browser release channel for Safari: {channel}", nameof(channel)),
        };

        if (version != BrowserVersion.Latest && version != BrowserVersion.SystemInstalled)
        {
            throw new ArgumentException("Safari cannot be downloaded, so only its installed version can be located.", nameof(version));
        }

        if (locationBehavior == FileLocationBehavior.UseCustomLocation && string.IsNullOrWhiteSpace(customPath))
        {
            throw new ArgumentException("customPath must be provided when locationBehavior is UseCustomLocation.", nameof(customPath));
        }

        return new SafariBrowserLocatorSettings(safariChannel, downloadOptions, locationBehavior == FileLocationBehavior.UseCustomLocation ? customPath : null);
    }

    /// <summary>
    /// Gets a value indicating whether an exception from a version information request means the
    /// service could not be reached, failed, or refused the request for now, as opposed to it
    /// responding with something unusable.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns><see langword="true"/> if the service could not be reached; otherwise, <see langword="false"/>.</returns>
    internal static bool IsUnreachableServiceException(Exception exception)
    {
        return exception is HttpRequestException
            || exception is OperationCanceledException
            || exception is DownloadServiceException { IsServerError: true }
            || exception is DownloadServiceException { IsRateLimited: true };
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.invocableLogMessageObservableEvent.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    private async Task<LocatedBrowser> LocateBrowserPathAsync(CancellationToken cancellationToken)
    {
        // Check environment variable first
        string? envBrowserPath = LauncherEnvironment.GetVariable(this.settings.EnvironmentVariableName);
        if (envBrowserPath is not null)
        {
            await this.LogAsync($"Using environment variable '{this.settings.EnvironmentVariableName}': {envBrowserPath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            return new LocatedBrowser(envBrowserPath, null);
        }

        if (this.settings.LocationBehavior != FileLocationBehavior.AutoLocateAndDownload)
        {
            if (!this.settings.IncludeDriver)
            {
                // If we are using a browser driver, this has already been logged.
                await this.LogAsync($"Using {this.settings.BrowserLocationBehaviorDescription} browser at: {this.settings.ExpectedExecutablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            }

            return new LocatedBrowser(this.settings.ExpectedExecutablePath, null);
        }

        try
        {
            return await this.LocateCachedBrowserAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not WebDriverBiDiException && !(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            throw new BrowserDownloadException($"Unable to locate or download {this.settings.BrowserDisplayName}: {ex.Message}", ex);
        }
    }

    private async Task<LocatedBrowser> LocateCachedBrowserAsync(CancellationToken cancellationToken)
    {
        InstallCache cache = new(Path.Combine(this.CacheDirectory, this.settings.BrowserName, this.settings.Channel), this.settings.DownloadOptions);
        using IDisposable lockHandle = await cache.LockAsync(cancellationToken).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.ExpectedExecutablePath;
        string? versionRequest = this.settings.BrowserVersionRequest;
        if (versionRequest is null)
        {
            // A specific version never changes, so once installed it needs no network request.
            if (cache.TryGetInstalledExecutable(this.settings.Version, relativeExecutablePath, out string? installedPath))
            {
                await this.LogUsingCachedBrowserAsync().ConfigureAwait(false);
                return new LocatedBrowser(installedPath, this.settings.Version);
            }

            this.ThrowIfDownloadSkipped($"{this.settings.BrowserDisplayName} {this.settings.Version}");
            BrowserDownloadInfo pinnedDownloadInfo = await this.settings.GetBrowserDownloadInfo(cancellationToken).ConfigureAwait(false);
            return new LocatedBrowser(await this.InstallBrowserAsync(cache, pinnedDownloadInfo, cancellationToken).ConfigureAwait(false), pinnedDownloadInfo.Version);
        }

        string? cachedPath = null;
        bool isLatestInstalled = cache.TryGetResolvedVersion(versionRequest, out string? latestVersion, out bool isFresh)
            && cache.TryGetInstalledExecutable(latestVersion, relativeExecutablePath, out cachedPath);
        if (isLatestInstalled && (isFresh || this.settings.DownloadOptions.SkipDownload))
        {
            await this.LogUsingCachedBrowserAsync().ConfigureAwait(false);
            return new LocatedBrowser(cachedPath!, latestVersion);
        }

        this.ThrowIfDownloadSkipped(this.settings.BrowserDisplayName);
        BrowserDownloadInfo downloadInfo;
        try
        {
            downloadInfo = await this.settings.GetBrowserDownloadInfo(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (isLatestInstalled && IsUnreachableServiceException(ex) && !cancellationToken.IsCancellationRequested)
        {
            await this.LogAsync($"Could not check for a newer {this.settings.BrowserDisplayName} ({ex.Message}); using cached version {latestVersion}.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return new LocatedBrowser(cachedPath!, latestVersion);
        }

        // A channel whose builds share a version number (Firefox Nightly) is reinstalled whenever it is rechecked.
        if (!downloadInfo.IgnoreVersionMatch && cache.TryGetInstalledExecutable(downloadInfo.Version, relativeExecutablePath, out string? currentPath))
        {
            cache.SaveResolvedVersion(versionRequest, downloadInfo.Version);
            await this.LogUsingCachedBrowserAsync().ConfigureAwait(false);
            return new LocatedBrowser(currentPath, downloadInfo.Version);
        }

        string executablePath;
        try
        {
            executablePath = await this.InstallBrowserAsync(cache, downloadInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex) when (cache.TryGetInstalledExecutable(downloadInfo.Version, relativeExecutablePath, out string? inUsePath))
        {
            // Not recorded as resolved, so that the replacement is attempted again next time.
            await this.LogAsync($"Could not replace {this.settings.BrowserDisplayName} {downloadInfo.Version}, which may be in use ({ex.Message}); using the existing installation.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return new LocatedBrowser(inUsePath, downloadInfo.Version);
        }

        cache.SaveResolvedVersion(versionRequest, downloadInfo.Version);
        return new LocatedBrowser(executablePath, downloadInfo.Version);
    }

    private void ThrowIfDownloadSkipped(string description)
    {
        if (this.settings.DownloadOptions.SkipDownload)
        {
            throw new BrowserDownloadException($"{description} is not installed in {this.CacheDirectory}, and downloads are disabled by {nameof(BrowserDownloadOptions.SkipDownload)} (or {LauncherEnvironment.SkipDownloadVariableName}).");
        }
    }

    private async Task LogUsingCachedBrowserAsync()
    {
        if (!this.settings.IncludeDriver)
        {
            // If we are using a browser driver, this has already been logged.
            await this.LogAsync($"Using {this.settings.BrowserLocationBehaviorDescription} browser.", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        }
    }

    private async Task<string> InstallBrowserAsync(InstallCache cache, BrowserDownloadInfo downloadInfo, CancellationToken cancellationToken)
    {
        string name = $"{this.settings.BrowserDisplayName} {downloadInfo.Version}";
        if (this.settings.PlatformSubstitutionNote is string note)
        {
            await this.LogAsync(note, WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        }

        string? sha256 = await this.settings.GetBrowserSha256Async(downloadInfo, cancellationToken).ConfigureAwait(false);
        await this.LogAsync($"Downloading {name}...", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        string relativeExecutablePath = this.settings.ExpectedExecutablePath;
        string executablePath = await cache.InstallAsync(downloadInfo.Version, relativeExecutablePath, async installDirectory =>
        {
            string installerPath = Path.Combine(installDirectory, this.settings.InstallerFileName);
            FileDownloader downloader = new(name, this.settings.DownloadOptions.Progress, message => this.LogAsync(message, WebDriverBiDiLogLevel.Info));
            await downloader.DownloadFileAsync(DownloadHttpClient.GetClient(this.settings.DownloadOptions), downloadInfo.DownloadUrl, installerPath, sha256, null, cancellationToken).ConfigureAwait(false);
            await this.settings.BrowserExtractor.ExtractFileContentsAsync(installerPath, installDirectory, cancellationToken).ConfigureAwait(false);
            if (!File.Exists(Path.Combine(installDirectory, relativeExecutablePath)))
            {
                throw new FileNotFoundException($"{this.settings.BrowserDisplayName} executable not found after extraction at: {relativeExecutablePath}");
            }
        }).ConfigureAwait(false);

        if (!this.settings.IncludeDriver)
        {
            // If we are using a browser driver, this has already been logged.
            await this.LogAsync($"Downloaded {this.settings.BrowserDisplayName} ready at: {executablePath}", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        }

        return executablePath;
    }

    private sealed record LocatedBrowser(string Path, string? Version);
}
