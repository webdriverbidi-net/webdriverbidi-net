// <copyright file="ChromeLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

public class ChromeLocatorTests
{
    private const string LatestVersion = "130.0.6723.58";
    private const string OlderVersion = "129.0.6668.100";

    public static TheoryData<OperatingSystemFamily, Architecture, string> Platforms => new()
    {
        { OperatingSystemFamily.Linux, Architecture.X64, "linux64" },
        { OperatingSystemFamily.MacOS, Architecture.Arm64, "mac-arm64" },
        { OperatingSystemFamily.MacOS, Architecture.X64, "mac-x64" },
        { OperatingSystemFamily.Windows, Architecture.X64, "win64" },
        { OperatingSystemFamily.Windows, Architecture.X86, "win32" },
    };

    [Theory]
    [MemberData(nameof(Platforms))]
    public async Task FindBrowserDownloadsLatestChannelReleaseForPlatform(OperatingSystemFamily operatingSystem, Architecture architecture, string platformIdentifier)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        Assert.Equal(Path.Combine(cache.Path, "chrome", "stable", LatestVersion, TestDownloadOptions.ToLocalPath(ChromeExecutablePath(platformIdentifier))), path);
        Assert.True(File.Exists(path));
        Assert.Equal(1, server.RequestCount(ChannelDocumentPath));
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, platformIdentifier)));
    }

    [Fact]
    public async Task FindBrowserUsesChannelNamedByReleaseChannel()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Canary", LatestVersion);
        using TemporaryDirectory cache = new();

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Alpha, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: TestDownloadOptions.Create(server, cache));

        Assert.StartsWith(Path.Combine(cache.Path, "chrome", "canary", LatestVersion), path);
    }

    [Fact]
    public async Task FindBrowserDownloadsSpecificVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion, OlderVersion);
        using TemporaryDirectory cache = new();

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Specific(OlderVersion), FileLocationBehavior.AutoLocateAndDownload, downloadOptions: TestDownloadOptions.Create(server, cache));

        Assert.Equal(Path.Combine(cache.Path, "chrome", "stable", OlderVersion, "chrome-linux64", "chrome"), path);
        Assert.Equal(1, server.RequestCount(AllVersionsDocumentPath));
        Assert.Equal(0, server.RequestCount(ChannelDocumentPath));
    }

    [Fact]
    public async Task FindBrowserThrowsForUnpublishedVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Specific("1.2.3.4"), FileLocationBehavior.AutoLocateAndDownload, downloadOptions: TestDownloadOptions.Create(server, cache)));

        Assert.Contains("1.2.3.4", exception.Message);
    }

    [Fact]
    public async Task FindBrowserUsesCacheWithoutNetworkRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        string firstPath = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);
        int requestCount = server.RequestedUrls.Count;

        string secondPath = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        Assert.Equal(firstPath, secondPath);
        Assert.Equal(requestCount, server.RequestedUrls.Count);
    }

    [Fact]
    public async Task FindBrowserRechecksVersionInformationOnceCacheEntryExpires()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        timeProvider.Advance(TimeSpan.FromHours(25));
        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        Assert.Equal(2, server.RequestCount(ChannelDocumentPath));
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, "linux64")));
    }

    [Fact]
    public async Task FindBrowserUsesSuppliedHttpClient()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        CountingHandler handler = new();
        using HttpClient httpClient = new(handler);
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            HttpClient = httpClient,
        };

        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        Assert.Equal(2, handler.RequestCount);
    }

    [Theory]
    [MemberData(nameof(Platforms))]
    public async Task FindDriverDownloadsDriverMatchingLatestChannelRelease(OperatingSystemFamily operatingSystem, Architecture architecture, string platformIdentifier)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        string driverFileName = operatingSystem == OperatingSystemFamily.Windows ? "chromedriver.exe" : "chromedriver";
        Assert.Equal(Path.Combine(cache.Path, "drivers", "chromedriver", LatestVersion, driverFileName), path);
        Assert.True(File.Exists(path));
        Assert.Equal(1, server.RequestCount(DriverArchivePath(LatestVersion, platformIdentifier)));
    }

    private sealed class CountingHandler : DelegatingHandler
    {
        private int requestCount;

        public CountingHandler()
            : base(new HttpClientHandler())
        {
        }

        public int RequestCount => this.requestCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref this.requestCount);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
