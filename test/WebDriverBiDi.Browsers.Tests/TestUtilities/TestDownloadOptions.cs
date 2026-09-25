// <copyright file="TestDownloadOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Runtime.InteropServices;

/// <summary>
/// Creates <see cref="BrowserDownloadOptions"/> that cache into a test's temporary directory and
/// download from a <see cref="DownloadServer"/>.
/// </summary>
public static class TestDownloadOptions
{
    /// <summary>
    /// The base path of the fake Mozilla product download service.
    /// </summary>
    public const string FirefoxProductPath = "/mozilla/";

    /// <summary>
    /// The base path of the fake Mozilla archive.
    /// </summary>
    public const string FirefoxArchivePath = "/archive/";

    /// <summary>
    /// The base path of the fake geckodriver releases API.
    /// </summary>
    public const string GeckoDriverReleasesPath = "/gecko/";

    /// <summary>
    /// The base path of the fake msedgedriver download server.
    /// </summary>
    public const string EdgeDriverPath = "/edgedriver/";

    /// <summary>
    /// The platform used when a test does not name one.
    /// </summary>
    public static readonly BrowserPlatform DefaultPlatform = new(OperatingSystemFamily.Linux, Architecture.X64);

    /// <summary>
    /// Creates download options for a test.
    /// </summary>
    /// <param name="server">The server standing in for the download services.</param>
    /// <param name="cacheDirectory">The test's cache directory.</param>
    /// <param name="platform">The platform, or <see langword="null"/> for <see cref="DefaultPlatform"/>.</param>
    /// <param name="timeProvider">The time provider, or <see langword="null"/> for the system clock.</param>
    /// <param name="skipDownload">A value indicating whether to use only what is already in the cache.</param>
    /// <returns>The download options.</returns>
    public static BrowserDownloadOptions Create(DownloadServer server, TemporaryDirectory cacheDirectory, BrowserPlatform? platform = null, TimeProvider? timeProvider = null, bool skipDownload = false)
    {
        return new BrowserDownloadOptions()
        {
            CacheDirectory = cacheDirectory.Path,
            Platform = platform ?? DefaultPlatform,
            TimeProvider = timeProvider ?? TimeProvider.System,
            SkipDownload = skipDownload,
            ManifestUrl = null,
            ChromeForTestingEndpoint = server.UrlFor(ChromeForTestingService.BasePath),
            FirefoxProductEndpoint = server.UrlFor(FirefoxProductPath),
            FirefoxArchiveEndpoint = server.UrlFor(FirefoxArchivePath),
            GeckoDriverReleasesEndpoint = server.UrlFor(GeckoDriverReleasesPath),
            EdgeDriverEndpoint = server.UrlFor(EdgeDriverPath),
        };
    }

    /// <summary>
    /// Converts a '/'-separated relative path to one using the current platform's separator.
    /// </summary>
    /// <param name="relativePath">The '/'-separated path.</param>
    /// <returns>The converted path.</returns>
    public static string ToLocalPath(string relativePath)
    {
        return Path.Combine(relativePath.Split('/'));
    }
}
