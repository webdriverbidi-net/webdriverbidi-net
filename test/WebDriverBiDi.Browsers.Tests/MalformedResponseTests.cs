// <copyright file="MalformedResponseTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

// Version information that is well-formed JSON but not what the service promises is reported
// clearly, naming what is missing, rather than failing somewhere later.
public class MalformedResponseTests
{
    private const string ChromeVersion = "130.0.6723.58";

    [Theory]
    [InlineData(ChannelDocumentPath, null, "Failed to deserialize")]
    [InlineData(MilestoneDocumentPath, 130, "Failed to deserialize")]
    [InlineData(AllVersionsDocumentPath, 0, "Failed to deserialize")]
    public async Task NullVersionDocumentIsReported(string documentPath, int? milestone, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        server.AddText(documentPath, "null");
        using TemporaryDirectory cache = new();
        BrowserVersion version = milestone switch
        {
            null => BrowserVersion.Latest,
            0 => BrowserVersion.Specific(ChromeVersion),
            int number => BrowserVersion.Milestone(number),
        };

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, version: version, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task UnknownFirefoxChannelIsRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, (BrowserReleaseChannel)99, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ChannelMissingFromVersionDocumentIsReported()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Canary", ChromeVersion);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Chrome channel Stable", exception.Message);
    }

    [Theory]
    [InlineData(false, "chrome-headless-shell download information")]
    [InlineData(true, "download URL for platform linux64")]
    public async Task BuildMissingFromVersionDocumentIsReported(bool listOtherPlatform, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        JsonObject downloads = [];
        if (listOtherPlatform)
        {
            downloads["chrome-headless-shell"] = new JsonArray(new JsonObject() { ["platform"] = "win64", ["url"] = "http://example/shell.zip" });
        }

        ServeChannelDocument(server, downloads);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), browserOptions: new ChromeLaunchOptions() { UseHeadlessShell = true }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(false, "chromedriver download information")]
    [InlineData(true, "chromedriver download URL for platform linux64")]
    public async Task DriverMissingFromVersionDocumentIsReported(bool listOtherPlatform, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        JsonObject downloads = [];
        if (listOtherPlatform)
        {
            downloads["chromedriver"] = new JsonArray(new JsonObject() { ["platform"] = "win64", ["url"] = "http://example/chromedriver.zip" });
        }

        ServeChannelDocument(server, downloads);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task FirefoxServiceThatDoesNotRedirectLeavesNoChecksumToVerify()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(TestDownloadOptions.FirefoxProductPath, [1, 2, 3]);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Cannot find the SHA256SUMS file", exception.Message);
    }

    [Fact]
    public async Task FirefoxNightlyBuildOfUnknownFormHasNoChecksumsFile()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor("/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.zip"));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Alpha, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Cannot find the checksums file", exception.Message);
    }

    [Fact]
    public async Task PinnedFirefoxVersionThatIsNotAVersionIsReported()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, version: BrowserVersion.Specific("esr"), downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Theory]
    [InlineData("null", "Failed to deserialize geckodriver release")]
    [InlineData("{\"tag_name\":\"v0.36.0\",\"assets\":[]}", "Failed to find geckodriver asset geckodriver-v0.36.0-linux64.tar.gz")]
    public async Task UnusableGeckoDriverReleaseIsReported(string release, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddText("/gecko/latest", release);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(BrowserReleaseChannel.DeveloperPreview, "dev", "Firefox Developer Edition.app/Contents/MacOS/firefox")]
    [InlineData(BrowserReleaseChannel.Alpha, "nightly", "Firefox Nightly.app/Contents/MacOS/firefox")]
    public async Task MacOSFirefoxChannelsInstallUnderTheirOwnApplicationNames(BrowserReleaseChannel channel, string channelDirectory, string executablePath)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor("/pub/devedition/releases/132.0b9/mac/en-US/Firefox%20132.0b9.dmg"));
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, $"firefox/{channelDirectory}", "132.0b9", executablePath);
        CacheSeeder.SeedResolvedVersion(cache, $"firefox/{channelDirectory}", "latest", "132.0b9", DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(OperatingSystemFamily.MacOS, Architecture.Arm64));

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, channel, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, null, null, "returned 403")]
    [InlineData((HttpStatusCode)429, "0", "not-a-time", "rate limit is exhausted. Supply")]
    [InlineData(HttpStatusCode.Forbidden, "5", null, "returned 403")]
    public async Task RefusedRequestIsReportedAccordingToRateLimitHeaders(HttpStatusCode statusCode, string? remaining, string? reset, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Dictionary<string, string> headers = [];
        if (remaining is not null)
        {
            headers["X-RateLimit-Remaining"] = remaining;
        }

        if (reset is not null)
        {
            headers["X-RateLimit-Reset"] = reset;
        }

        server.AddResponses("/gecko/latest", new ServedResponse(statusCode, Headers: headers));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task StorageHashWithoutMd5IsNotChecked()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        byte[] archive = TestArchives.Zip(ChromeExecutablePath("linux64"));
        server.AddResponses(ChromeArchivePath(ChromeVersion, "linux64"), new ServedResponse(HttpStatusCode.OK, archive, new Dictionary<string, string>() { ["x-goog-hash"] = "crc32c=AAAAAA==" }));
        using TemporaryDirectory cache = new();

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(File.Exists(path));
    }

    private static void ServeChannelDocument(DownloadServer server, JsonObject downloads)
    {
        JsonObject stable = new() { ["channel"] = "Stable", ["version"] = ChromeVersion, ["revision"] = "1", ["downloads"] = downloads };
        server.AddText(ChannelDocumentPath, new JsonObject() { ["channels"] = new JsonObject() { ["Stable"] = stable } }.ToJsonString());
    }
}
