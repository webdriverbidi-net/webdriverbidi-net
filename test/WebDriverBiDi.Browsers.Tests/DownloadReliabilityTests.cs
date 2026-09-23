// <copyright file="DownloadReliabilityTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

// Failed requests are retried twice, after real delays totalling 1.5 seconds.
public class DownloadReliabilityTests
{
    private const string ChromeVersion = "130.0.6723.58";
    private const string FirefoxVersion = "140.0";
    private const string FirefoxArchivePath = "/archive/firefox/releases/140.0/linux-x86_64/en-US/firefox-140.0.tar.xz";
    private const string GeckoDriverReleasePath = "/gecko/latest";
    private const string GeckoDriverAssetPath = "/gecko/download/geckodriver-v0.36.0-linux64.tar.gz";

    [Fact]
    public async Task ServerErrorFromVersionServiceIsRetried()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = $"/pub/firefox/releases/131.0.2/linux-x86_64/en-US/firefox-131.0.2.tar.xz";
        server.AddResponses(
            TestDownloadOptions.FirefoxProductPath,
            new ServedResponse(HttpStatusCode.ServiceUnavailable),
            new ServedResponse(HttpStatusCode.Found, Headers: new Dictionary<string, string>() { ["Location"] = server.UrlFor(archivePath).AbsoluteUri }));
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/stable", "131.0.2", "firefox/firefox");

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
        Assert.Equal(2, server.RequestCount(TestDownloadOptions.FirefoxProductPath));
    }

    [Fact]
    public async Task PersistentServerErrorFailsAfterThreeAttempts()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses(ChannelDocumentPath, new ServedResponse(HttpStatusCode.InternalServerError));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains("500", exception.Message);
        Assert.Equal(3, server.RequestCount(ChannelDocumentPath));
    }

    [Fact]
    public async Task NotFoundIsReportedWithoutRetrying()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddNotFound(ChannelDocumentPath);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains($"{ChannelDocumentPath} was not found (404)", exception.Message);
        Assert.Equal(1, server.RequestCount(ChannelDocumentPath));
    }

    [Fact]
    public async Task ServerErrorFallsBackToInstalledVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses(ChannelDocumentPath, new ServedResponse(HttpStatusCode.BadGateway));
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, ChromeExecutablePath("linux64"));
        CacheSeeder.SeedResolvedVersion(cache, "chrome/stable", "latest", ChromeVersion, timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        string path = await FindChromeAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider));

        Assert.Equal(installedPath, path);
    }

    [Fact]
    public async Task RateLimitIsReportedWithoutRetrying()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses(GeckoDriverReleasePath, RateLimitResponse());
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindGeckoDriverAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains("rate limit is exhausted until 2026-09-23 18:00:00Z", exception.Message);
        Assert.Contains(nameof(BrowserDownloadOptions.HttpClient), exception.Message);
        Assert.Equal(1, server.RequestCount(GeckoDriverReleasePath));
    }

    [Fact]
    public async Task RateLimitFallsBackToInstalledDriver()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses(GeckoDriverReleasePath, RateLimitResponse());
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");
        CacheSeeder.SeedResolvedVersion(cache, "drivers/geckodriver", "latest", "0.36.0", timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        string? path = await FindGeckoDriverAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider));

        Assert.Equal(installedPath, path);
    }

    [Fact]
    public async Task InterruptedDownloadIsRetried()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        server.AddResponses(
            ChromeArchivePath(ChromeVersion, "linux64"),
            new ServedResponse(HttpStatusCode.InternalServerError),
            new ServedResponse(HttpStatusCode.OK, TestArchives.Zip(ChromeExecutablePath("linux64"))));
        using TemporaryDirectory cache = new();

        string path = await FindChromeAsync(TestDownloadOptions.Create(server, cache));

        Assert.True(File.Exists(path));
        Assert.Equal(2, server.RequestCount(ChromeArchivePath(ChromeVersion, "linux64")));
    }

    [Fact]
    public async Task DownloadMatchingStorageHashIsInstalled()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        byte[] archive = TestArchives.Zip(ChromeExecutablePath("linux64"));
        server.AddResponses(ChromeArchivePath(ChromeVersion, "linux64"), StorageResponse(archive, Md5Of(archive)));
        using TemporaryDirectory cache = new();

        string path = await FindChromeAsync(TestDownloadOptions.Create(server, cache));

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task DownloadNotMatchingStorageHashIsRetriedThenRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        byte[] archive = TestArchives.Zip(ChromeExecutablePath("linux64"));
        server.AddResponses(ChromeArchivePath(ChromeVersion, "linux64"), StorageResponse(archive[..^10], Md5Of(archive)));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains("MD5", exception.Message);
        Assert.Equal(3, server.RequestCount(ChromeArchivePath(ChromeVersion, "linux64")));
        Assert.Empty(Directory.GetDirectories(Path.Combine(cache.Path, "chrome", "stable")));
    }

    [Fact]
    public async Task FirefoxDownloadMatchingPublishedChecksumIsInstalled()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        byte[] archive = TestArchives.TarGz("firefox/firefox");
        server.AddFile(FirefoxArchivePath, archive);
        FirefoxLocatorTests.ServeChecksums(server, FirefoxArchivePath, Sha256Of(archive));
        using TemporaryDirectory cache = new();

        string path = await FindPinnedFirefoxAsync(TestDownloadOptions.Create(server, cache));

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task FirefoxDownloadNotMatchingPublishedChecksumIsRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(FirefoxArchivePath, TestArchives.TarGz("firefox/firefox"));
        FirefoxLocatorTests.ServeChecksums(server, FirefoxArchivePath, new string('a', 64));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindPinnedFirefoxAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains($"its publisher lists {new string('a', 64)}", exception.Message);
        Assert.Equal(1, server.RequestCount(FirefoxArchivePath));
        Assert.Empty(Directory.GetDirectories(Path.Combine(cache.Path, "firefox", "stable")));
    }

    [Fact]
    public async Task FirefoxDownloadWithoutPublishedChecksumsIsNotAttempted()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(FirefoxArchivePath, TestArchives.TarGz("firefox/firefox"));
        server.AddNotFound("/archive/firefox/releases/140.0/SHA256SUMS");
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindPinnedFirefoxAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains("SHA256SUMS was not found (404)", exception.Message);
        Assert.Equal(0, server.RequestCount(FirefoxArchivePath));
    }

    [Fact]
    public async Task FirefoxDownloadNotListedInChecksumsIsNotAttempted()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(FirefoxArchivePath, TestArchives.TarGz("firefox/firefox"));
        server.AddText("/archive/firefox/releases/140.0/SHA256SUMS", $"{new string('a', 64)}  linux-x86_64/en-US/firefox-140.0.tar.bz2\n");
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindPinnedFirefoxAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains("lists no SHA-256 hash for linux-x86_64/en-US/firefox-140.0.tar.xz", exception.Message);
        Assert.Equal(0, server.RequestCount(FirefoxArchivePath));
    }

    [Theory]
    [InlineData(true, false, "SHA-256")]
    [InlineData(false, true, "bytes")]
    public async Task GeckoDriverNotMatchingPublishedDigestOrSizeIsRejected(bool wrongDigest, bool wrongSize, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        byte[] archive = TestArchives.TarGz("geckodriver");
        server.AddFile(GeckoDriverAssetPath, archive);
        JsonObject asset = new()
        {
            ["name"] = "geckodriver-v0.36.0-linux64.tar.gz",
            ["browser_download_url"] = server.UrlFor(GeckoDriverAssetPath).AbsoluteUri,
            ["digest"] = wrongDigest ? $"sha256:{new string('a', 64)}" : null,
            ["size"] = wrongSize ? archive.Length + 1 : archive.Length,
        };
        server.AddText(GeckoDriverReleasePath, new JsonObject() { ["tag_name"] = "v0.36.0", ["assets"] = new JsonArray(asset) }.ToJsonString());
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindGeckoDriverAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Contains(expectedMessage, exception.Message);
    }

    // A supplied client that follows the product service's redirect itself leaves the target as the final request URL.
    [Fact]
    public async Task FirefoxVersionIsFoundThroughRedirectFollowingClient()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = "/pub/firefox/releases/131.0.2/linux-x86_64/en-US/firefox-131.0.2.tar.xz";
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archivePath));
        server.AddFile(archivePath, [1, 2, 3]);
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/stable", "131.0.2", "firefox/firefox");
        using HttpClient httpClient = new();
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            FirefoxProductEndpoint = defaults.FirefoxProductEndpoint,
            HttpClient = httpClient,
        };

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
        Assert.Equal(1, server.RequestCount(archivePath));
    }

    private static Task<string> FindChromeAsync(BrowserDownloadOptions options)
    {
        return BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static Task<string> FindPinnedFirefoxAsync(BrowserDownloadOptions options)
    {
        return BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, version: BrowserVersion.Specific(FirefoxVersion), downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static Task<string?> FindGeckoDriverAsync(BrowserDownloadOptions options)
    {
        return DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    // The reset time is 2026-09-23 18:00:00 UTC.
    private static ServedResponse RateLimitResponse()
    {
        return new ServedResponse(HttpStatusCode.Forbidden, Headers: new Dictionary<string, string>() { ["X-RateLimit-Remaining"] = "0", ["X-RateLimit-Reset"] = "1790186400" });
    }

    private static ServedResponse StorageResponse(byte[] body, string md5)
    {
        return new ServedResponse(HttpStatusCode.OK, body, new Dictionary<string, string>() { ["x-goog-hash"] = $"crc32c=AAAAAA==,md5={md5}" });
    }

    private static string Md5Of(byte[] content) => Convert.ToBase64String(MD5.HashData(content));

    private static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));
}
