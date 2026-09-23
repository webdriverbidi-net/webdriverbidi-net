// <copyright file="BrowserDownloadOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;

/// <summary>
/// Options controlling where browsers and drivers are cached, which platform they are
/// located for, and where they are downloaded from.
/// </summary>
public class BrowserDownloadOptions
{
    private string cacheDirectory = LauncherEnvironment.GetVariable(LauncherEnvironment.BrowsersPathVariableName) ?? DefaultCacheDirectory;
    private TimeProvider timeProvider = TimeProvider.System;
    private TimeSpan lockTimeout = TimeSpan.FromMinutes(10);
    private Uri chromeForTestingEndpoint = new("https://googlechromelabs.github.io/chrome-for-testing/");
    private Uri firefoxProductEndpoint = new("https://download.mozilla.org/");
    private Uri firefoxReleaseArchiveEndpoint = new("https://download-installer.cdn.mozilla.net/pub/firefox/releases/");
    private Uri geckoDriverReleasesEndpoint = new("https://api.github.com/repos/mozilla/geckodriver/releases/");

    /// <summary>
    /// Gets the platform's cache directory for downloaded browsers and drivers: a "webdriverbidi-net"
    /// subdirectory of the local application data directory on Windows, of ~/Library/Caches on
    /// macOS, and of $XDG_CACHE_HOME (or ~/.cache) on Linux.
    /// </summary>
    public static string DefaultCacheDirectory
    {
        get
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string cacheRoot;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cacheRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                cacheRoot = Path.Combine(userProfile, "Library", "Caches");
            }
            else
            {
                // The XDG specification requires a relative value to be ignored.
                string? xdgCacheHome = LauncherEnvironment.GetVariable("XDG_CACHE_HOME");
                cacheRoot = xdgCacheHome is not null && Path.IsPathRooted(xdgCacheHome) ? xdgCacheHome : Path.Combine(userProfile, ".cache");
            }

            return Path.Combine(cacheRoot, "webdriverbidi-net");
        }
    }

    /// <summary>
    /// Gets the directory in which downloaded browsers and drivers are cached. Defaults to the
    /// value of the WEBDRIVERBIDI_BROWSERS_PATH environment variable if set, or to
    /// <see cref="DefaultCacheDirectory"/> otherwise.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when set to a null or empty value.</exception>
    public string CacheDirectory
    {
        get => this.cacheDirectory;
        init => this.cacheDirectory = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Cache directory cannot be null or empty.", nameof(this.CacheDirectory))
            : value;
    }

    /// <summary>
    /// Gets a value indicating whether to use only what is already in the cache, making no network
    /// requests: an installed version is used however old its version information is, and locating
    /// a browser or driver that is not installed fails. Defaults to <see langword="true"/> if the
    /// WEBDRIVERBIDI_SKIP_DOWNLOAD environment variable is set to "1" or "true".
    /// </summary>
    public bool SkipDownload { get; init; } = LauncherEnvironment.IsEnabled(LauncherEnvironment.SkipDownloadVariableName);

    /// <summary>
    /// Gets the platform for which browsers and drivers are located and downloaded,
    /// or <see langword="null"/> to use the platform of the current process.
    /// </summary>
    public BrowserPlatform? Platform { get; init; }

    /// <summary>
    /// Gets the <see cref="System.TimeProvider"/> used to decide whether cached version information is stale.
    /// </summary>
    public TimeProvider TimeProvider
    {
        get => this.timeProvider;
        init => this.timeProvider = value ?? throw new ArgumentNullException(nameof(this.TimeProvider));
    }

    /// <summary>
    /// Gets how long to wait for another process that is installing the same browser or driver
    /// into the cache directory. Defaults to 10 minutes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive or <see cref="Timeout.InfiniteTimeSpan"/>.</exception>
    public TimeSpan LockTimeout
    {
        get => this.lockTimeout;
        init => this.lockTimeout = value > TimeSpan.Zero || value == Timeout.InfiniteTimeSpan
            ? value
            : throw new ArgumentOutOfRangeException(nameof(this.LockTimeout), "Lock timeout must be positive or infinite.");
    }

    /// <summary>
    /// Gets the receiver of download progress reports, or <see langword="null"/> for none. Reports are
    /// made on the thread doing the download, so a <see cref="Progress{T}"/> posts them to the context
    /// in which it was created.
    /// </summary>
    public IProgress<BrowserDownloadProgress>? Progress { get; init; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpClient"/> used for downloads, or <see langword="null"/>
    /// to use a client owned by this library. A supplied client is not disposed by this library.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// Gets the base URL of the Chrome for Testing version information service.
    /// </summary>
    public Uri ChromeForTestingEndpoint
    {
        get => this.chromeForTestingEndpoint;
        init => this.chromeForTestingEndpoint = AsBaseUri(value, nameof(this.ChromeForTestingEndpoint));
    }

    /// <summary>
    /// Gets the base URL of the Mozilla product download service, which redirects to the latest release of a channel.
    /// </summary>
    public Uri FirefoxProductEndpoint
    {
        get => this.firefoxProductEndpoint;
        init => this.firefoxProductEndpoint = AsBaseUri(value, nameof(this.FirefoxProductEndpoint));
    }

    /// <summary>
    /// Gets the base URL of the Mozilla archive of specific Firefox releases.
    /// </summary>
    public Uri FirefoxReleaseArchiveEndpoint
    {
        get => this.firefoxReleaseArchiveEndpoint;
        init => this.firefoxReleaseArchiveEndpoint = AsBaseUri(value, nameof(this.FirefoxReleaseArchiveEndpoint));
    }

    /// <summary>
    /// Gets the base URL of the geckodriver releases API.
    /// </summary>
    public Uri GeckoDriverReleasesEndpoint
    {
        get => this.geckoDriverReleasesEndpoint;
        init => this.geckoDriverReleasesEndpoint = AsBaseUri(value, nameof(this.GeckoDriverReleasesEndpoint));
    }

    /// <summary>
    /// Gets the platform to use, resolving <see langword="null"/> to the current platform.
    /// </summary>
    internal BrowserPlatform ResolvedPlatform => this.Platform ?? BrowserPlatform.Current;

    // Relative URLs resolve against a base only below its last '/', so a base without a trailing
    // slash would silently drop its final path segment.
    private static Uri AsBaseUri(Uri value, string propertyName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(propertyName);
        }

        if (!value.IsAbsoluteUri)
        {
            throw new ArgumentException("Endpoint must be an absolute URL.", propertyName);
        }

        return value.AbsolutePath.EndsWith("/", StringComparison.Ordinal) ? value : new Uri(value.AbsoluteUri + "/");
    }
}
