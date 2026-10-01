// <copyright file="DownloadReliabilityTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Runtime.Versioning;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Time.Testing;
using Dramaturge.Browsers.TestUtilities;
using static Dramaturge.Browsers.TestUtilities.ChromeForTestingService;

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

    // Only a service that cannot be reached, fails, or is rate limited leaves the stale version in use; a refusal is an error.
    [Fact]
    public async Task ClientErrorDoesNotFallBackToInstalledVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddResponses(ChannelDocumentPath, new ServedResponse(HttpStatusCode.NotFound));
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, ChromeExecutablePath("linux64"));
        CacheSeeder.SeedResolvedVersion(cache, "chrome/stable", "latest", ChromeVersion, timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider)));

        Assert.Contains("404", exception.Message);
    }

    [Fact]
    public async Task UnusableResponseDoesNotFallBackToInstalledVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddText(ChannelDocumentPath, "not a version document");
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, ChromeExecutablePath("linux64"));
        CacheSeeder.SeedResolvedVersion(cache, "chrome/stable", "latest", ChromeVersion, timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider)));
    }

    [Fact]
    public async Task LargeDownloadReportsProgressAsItArrives()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        byte[] archive = TestArchives.LargeZip(20 * 1024 * 1024, ChromeExecutablePath("linux64"));
        Serve(server, "Stable", ChromeVersion, archive);
        using TemporaryDirectory cache = new();
        RecordingProgress progress = new();
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            Progress = progress,
        };

        await FindChromeAsync(options);

        Assert.True(progress.Reports.Count > 3, $"Expected several progress reports, got {progress.Reports.Count}.");
        Assert.Equal(archive.Length, progress.Reports[^1].BytesReceived);
        Assert.All(progress.Reports, report => Assert.Equal(archive.Length, report.TotalBytes));
    }

    [Fact]
    [UnsupportedOSPlatform("windows")]
    public async Task InstalledNightlyIsKeptWhenItCannotBeReplaced()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows() || Environment.UserName == "root", "Needs directory permissions that the user cannot bypass.");
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = "/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.tar.xz";
        byte[] archive = TestArchives.TarGz("firefox/firefox");
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archivePath));
        server.AddFile(archivePath, archive);
        FirefoxLocatorTests.ServeChecksums(server, archivePath, Sha256Of(archive));
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/nightly", "133.0a1", "firefox/firefox");
        CacheSeeder.SeedResolvedVersion(cache, "firefox/nightly", "latest", "133.0a1", timeProvider.GetUtcNow() - TimeSpan.FromHours(25));
        string channelDirectory = Path.Combine(cache.Path, "firefox", "nightly");
        File.WriteAllText(Path.Combine(channelDirectory, ".lock"), string.Empty);
        File.SetUnixFileMode(channelDirectory, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        try
        {
            string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Alpha, downloadOptions: TestDownloadOptions.Create(server, cache, timeProvider: timeProvider), cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(installedPath, path);
        }
        finally
        {
            File.SetUnixFileMode(channelDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
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

    // Nightly builds share a version number, so a rechecked Nightly replaces the installed build of that version.
    [Fact]
    public async Task RecheckedNightlyReplacesInstalledBuildOfSameVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = "/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.tar.xz";
        byte[] archive = TestArchives.TarGz("firefox/firefox");
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archivePath));
        server.AddFile(archivePath, archive);
        FirefoxLocatorTests.ServeChecksums(server, archivePath, Sha256Of(archive));
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/nightly", "133.0a1", "firefox/firefox");
        string staleMarker = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(installedPath)!)!, "stale-build");
        File.WriteAllText(staleMarker, string.Empty);
        CacheSeeder.SeedResolvedVersion(cache, "firefox/nightly", "latest", "133.0a1", timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Alpha, downloadOptions: TestDownloadOptions.Create(server, cache, timeProvider: timeProvider), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
        Assert.Equal(1, server.RequestCount(archivePath));
        Assert.False(File.Exists(staleMarker));
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

    [Fact]
    public async Task DownloadInterruptedMidStreamIsRetried()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        byte[] archive = TestArchives.Zip(ChromeExecutablePath("linux64"));
        using ArchiveReplacingHandler handler = new(ChromeArchivePath(ChromeVersion, "linux64"), attempt => attempt == 1
            ? new StreamContent(new FailingStream(archive[..20]))
            : new ByteArrayContent(archive));
        using HttpClient httpClient = new(handler);

        string path = await FindChromeAsync(CreateOptionsWithClient(server, cache, httpClient));

        Assert.True(File.Exists(path));
        Assert.Equal(2, handler.Attempts);
    }

    [Fact]
    public async Task DownloadOfUnstatedLengthReportsProgressAtEnd()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        byte[] archive = TestArchives.Zip(ChromeExecutablePath("linux64"));
        using ArchiveReplacingHandler handler = new(ChromeArchivePath(ChromeVersion, "linux64"), _ => new StreamContent(new UnseekableStream(archive)));
        using HttpClient httpClient = new(handler);
        List<BrowserDownloadProgress> reports = [];
        BrowserDownloadOptions defaults = CreateOptionsWithClient(server, cache, httpClient);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            ManifestUrl = null,
            HttpClient = httpClient,
            Progress = new SynchronousProgress(reports.Add),
        };

        await FindChromeAsync(options);

        Assert.Null(reports[^1].TotalBytes);
        Assert.Equal(archive.Length, reports[^1].BytesReceived);
    }

    [Fact]
    public async Task DownloadEndingBeforeItsLengthIsRetriedThenRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        using ArchiveReplacingHandler handler = new(ChromeArchivePath(ChromeVersion, "linux64"), _ =>
        {
            ByteArrayContent content = new(new byte[10]);
            content.Headers.ContentLength = 100;
            return content;
        });
        using HttpClient httpClient = new(handler);

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(CreateOptionsWithClient(server, cache, httpClient)));

        Assert.Contains("ended after 10 of 100 bytes", exception.Message);
        Assert.Equal(3, handler.Attempts);
    }

    [Fact]
    public async Task StaleDriverResolvingToInstalledVersionIsNotDownloadedAgain()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        JsonObject asset = new() { ["name"] = "geckodriver-v0.36.0-linux64.tar.gz", ["browser_download_url"] = server.UrlFor(GeckoDriverAssetPath).AbsoluteUri };
        server.AddText(GeckoDriverReleasePath, new JsonObject() { ["tag_name"] = "v0.36.0", ["assets"] = new JsonArray(asset) }.ToJsonString());
        server.AddNotFound(GeckoDriverAssetPath);
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");
        CacheSeeder.SeedResolvedVersion(cache, "drivers/geckodriver", "latest", "0.35.0", timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        string? path = await FindGeckoDriverAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider));

        Assert.Equal(installedPath, path);
        Assert.Equal(0, server.RequestCount(GeckoDriverAssetPath));
    }

    private static BrowserDownloadOptions CreateOptionsWithClient(DownloadServer server, TemporaryDirectory cache, HttpClient httpClient)
    {
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        return new BrowserDownloadOptions()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            ManifestUrl = null,
            HttpClient = httpClient,
        };
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

    // Passes requests to the server, except that the archive at one path is replaced by generated content.
    private sealed class ArchiveReplacingHandler(string archivePath, Func<int, HttpContent> createContent) : DelegatingHandler(new HttpClientHandler())
    {
        private int attempts;

        public int Attempts => this.attempts;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath != archivePath)
            {
                return base.SendAsync(request, cancellationToken);
            }

            int attempt = Interlocked.Increment(ref this.attempts);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = createContent(attempt), RequestMessage = request });
        }
    }

    // A stream whose length is not known, so the response states none.
    private sealed class UnseekableStream(byte[] content) : MemoryStream(content)
    {
        public override bool CanSeek => false;
    }

    private sealed class SynchronousProgress(Action<BrowserDownloadProgress> report) : IProgress<BrowserDownloadProgress>
    {
        public void Report(BrowserDownloadProgress value) => report(value);
    }

    // Yields its content, then fails as a dropped connection does.
    private sealed class FailingStream(byte[] content) : MemoryStream(content)
    {
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.Position < this.Length ? base.ReadAsync(buffer, offset, count, cancellationToken) : throw new IOException("The connection was reset.");
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return this.Position < this.Length ? base.ReadAsync(buffer, cancellationToken) : throw new IOException("The connection was reset.");
        }
    }

    private static string Md5Of(byte[] content) => Convert.ToBase64String(MD5.HashData(content));

    private static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    // Records reports as they are made, rather than posting them as Progress<T> does.
    private sealed class RecordingProgress : IProgress<BrowserDownloadProgress>
    {
        private readonly List<BrowserDownloadProgress> reports = [];

        public IReadOnlyList<BrowserDownloadProgress> Reports
        {
            get
            {
                lock (this.reports)
                {
                    return [.. this.reports];
                }
            }
        }

        public void Report(BrowserDownloadProgress value)
        {
            lock (this.reports)
            {
                this.reports.Add(value);
            }
        }
    }
}
