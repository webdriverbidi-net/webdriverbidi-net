// <copyright file="ChromeDriverMatchingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

// The chromedriver for a Chrome that is not downloaded matches the version the installed Chrome reports.
public class ChromeDriverMatchingTests
{
    // The version the fake browser reports.
    private const string InstalledVersion = "130.0.2849.80";
    private const string ChannelVersion = "131.0.6778.204";

    public static TheoryData<string[], string> UnlistedVersionMatches => new()
    {
        { ["130.0.2849.70", "130.0.2849.95", "130.0.2850.1"], "130.0.2849.95" },
        { ["130.0.2800.5", "130.0.2900.1", "129.0.6668.100"], "130.0.2900.1" },
    };

    [Fact]
    public async Task DriverOfTheInstalledVersionIsUsed()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, InstalledVersion);
        using TemporaryDirectory cache = new();

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, TestDownloadOptions.Create(server, cache));

        Assert.Equal(DriverPath(cache, InstalledVersion), path);
    }

    [Theory]
    [MemberData(nameof(UnlistedVersionMatches))]
    public async Task ClosestDriverIsUsedAndRememberedWhenTheInstalledVersionIsNotListed(string[] listedVersions, string expectedVersion)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, listedVersions);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);
        string? cachedPath = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);

        Assert.Equal(DriverPath(cache, expectedVersion), path);
        Assert.Equal(path, cachedPath);
        Assert.Equal(1, server.RequestCount(AllVersionsDocumentPath));
    }

    [Fact]
    public async Task StaleMatchIsReplacedByTheExactDriverOncePublished()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, "130.0.2849.95");
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        string? matchedPath = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);
        Serve(server, "Stable", ChannelVersion, "130.0.2849.95", InstalledVersion);
        timeProvider.Advance(TimeSpan.FromHours(25));

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);

        Assert.Equal(DriverPath(cache, "130.0.2849.95"), matchedPath);
        Assert.Equal(DriverPath(cache, InstalledVersion), path);
    }

    [Fact]
    public async Task StaleMatchIsUsedWhenTheServiceCannotBeReached()
    {
        DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, "130.0.2849.95");
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);
        await server.DisposeAsync();
        timeProvider.Advance(TimeSpan.FromHours(25));

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);

        Assert.Equal(DriverPath(cache, "130.0.2849.95"), path);
    }

    [Fact]
    public async Task NoDriverOfTheInstalledMajorVersionFails()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, "129.0.6668.100");
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindDriverAsync(FakeBrowserSetup.ExecutablePath, TestDownloadOptions.Create(server, cache)));

        Assert.Contains($"Failed to find a chromedriver for Chrome version '{InstalledVersion}'", exception.Message);
    }

    [Fact]
    public async Task LatestDriverOfTheChannelIsUsedWhenTheVersionCannotBeRead()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion);
        using TemporaryDirectory cache = new();

        string? path = await FindDriverAsync(Path.Combine(cache.Path, "no-chrome-here"), TestDownloadOptions.Create(server, cache));

        Assert.Equal(DriverPath(cache, ChannelVersion), path);
    }

    [Fact]
    public async Task VersionThatIsNotNumericHasNoCompatibleDriver()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Chrome, version: BrowserVersion.Specific("custom-build"), downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Failed to find a chromedriver for Chrome version 'custom-build'", exception.Message);
    }

    [Fact]
    public async Task CompatibleDriverAlreadyInstalledIsNotDownloadedAgain()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion, "130.0.2849.95");
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        await DriverLocator.FindDriverAsync(BrowserKind.Chrome, version: BrowserVersion.Specific("130.0.2849.95"), downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);

        Assert.Equal(DriverPath(cache, "130.0.2849.95"), path);
        Assert.Equal(1, server.RequestCount(DriverArchivePath("130.0.2849.95", "linux64")));
    }

    [Fact]
    public async Task ChannelDriverResolvedRecentlyIsUsedWithoutRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChannelVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        int requests = server.RequestedUrls.Count;

        string? cachedPath = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(DriverPath(cache, ChannelVersion), path);
        Assert.Equal(path, cachedPath);
        Assert.Equal(requests, server.RequestedUrls.Count);
    }

    [Fact]
    public async Task StaleChannelDriverIsUsedWhenDownloadsAreSkipped()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string installedPath = CacheSeeder.SeedInstallation(cache, "drivers/chromedriver", ChannelVersion, "chromedriver");
        CacheSeeder.SeedResolvedVersion(cache, "drivers/chromedriver", "latest-stable", ChannelVersion, timeProvider.GetUtcNow() - TimeSpan.FromHours(25));

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache, timeProvider: timeProvider, skipDownload: true), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
        Assert.Empty(server.RequestedUrls);
    }

    private static Task<string?> FindDriverAsync(string browserPath, BrowserDownloadOptions options)
    {
        return DriverLocator.FindDriverAsync(BrowserKind.Chrome, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: browserPath, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static string DriverPath(TemporaryDirectory cache, string version) => Path.Combine(cache.Path, "drivers", "chromedriver", version, "chromedriver");
}
