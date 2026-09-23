// <copyright file="InstallCacheTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Text.Json.Nodes;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

public class InstallCacheTests
{
    private const string LatestVersion = "130.0.6723.58";
    private const string NewerVersion = "131.0.6778.33";
    private const string OlderVersion = "129.0.6668.100";

    [Fact]
    public async Task InstalledSpecificVersionNeedsNoNetworkRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion, OlderVersion);
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        string firstPath = await FindChromeAsync(options, BrowserVersion.Specific(OlderVersion));
        int requestCount = server.RequestedUrls.Count;

        timeProvider.Advance(TimeSpan.FromDays(30));
        string secondPath = await FindChromeAsync(options, BrowserVersion.Specific(OlderVersion));

        Assert.Equal(firstPath, secondPath);
        Assert.Equal(requestCount, server.RequestedUrls.Count);
    }

    [Fact]
    public async Task RecheckFindingSameVersionDefersNextCheck()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        await FindChromeAsync(options);
        timeProvider.Advance(TimeSpan.FromHours(25));
        await FindChromeAsync(options);

        timeProvider.Advance(TimeSpan.FromHours(23));
        await FindChromeAsync(options);

        Assert.Equal(2, server.RequestCount(ChannelDocumentPath));
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, "linux64")));
    }

    [Fact]
    public async Task RecheckFindingNewerVersionInstallsItAlongsideOlder()
    {
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        string olderPath;
        await using (DownloadServer server = await DownloadServer.StartAsync())
        {
            Serve(server, "Stable", LatestVersion);
            olderPath = await FindChromeAsync(TestDownloadOptions.Create(server, cache, timeProvider: timeProvider));
        }

        timeProvider.Advance(TimeSpan.FromHours(25));
        await using DownloadServer newerServer = await DownloadServer.StartAsync();
        Serve(newerServer, "Stable", NewerVersion);
        string newerPath = await FindChromeAsync(TestDownloadOptions.Create(newerServer, cache, timeProvider: timeProvider));

        Assert.Contains(NewerVersion, newerPath);
        Assert.True(File.Exists(newerPath));
        Assert.True(File.Exists(olderPath));
    }

    [Fact]
    public async Task UnreachableServiceFallsBackToInstalledVersion()
    {
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        string installedPath = await FindChromeAsync(options);
        await server.DisposeAsync();

        timeProvider.Advance(TimeSpan.FromHours(25));
        string path = await FindChromeAsync(options);

        Assert.Equal(installedPath, path);
    }

    [Fact]
    public async Task UnreachableServiceWithNothingInstalledFails()
    {
        using TemporaryDirectory cache = new();
        DownloadServer server = await DownloadServer.StartAsync();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        await server.DisposeAsync();

        await AssertDownloadFailedAsync(() => FindChromeAsync(options));
    }

    [Fact]
    public async Task UnreadableVersionRecordIsResolvedAgain()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        string installedPath = await FindChromeAsync(options);
        File.WriteAllText(Path.Combine(cache.Path, "chrome", "stable", "resolved-versions.json"), "{ not json");

        string path = await FindChromeAsync(options);

        Assert.Equal(installedPath, path);
        Assert.Equal(2, server.RequestCount(ChannelDocumentPath));
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, "linux64")));
    }

    [Fact]
    public async Task InterruptedInstallationIsReplaced()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        CacheSeeder.SeedPartialInstallation(cache, "chrome/stable", LatestVersion, ChromeExecutablePath("linux64"));
        string abandonedDirectory = CacheSeeder.GetVersionDirectory(cache, "chrome/stable", ".tmp-abandoned");
        Directory.CreateDirectory(abandonedDirectory);

        string path = await FindChromeAsync(TestDownloadOptions.Create(server, cache));

        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, "linux64")));
        Assert.True(File.Exists(Path.Combine(CacheSeeder.GetVersionDirectory(cache, "chrome/stable", LatestVersion), CacheSeeder.InstallationMarkerFileName)));
        Assert.Equal(ChromeExecutablePath("linux64"), File.ReadAllText(path));
        Assert.False(Directory.Exists(abandonedDirectory));
    }

    [Fact]
    public async Task FailedExtractionLeavesNothingInstalled()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion, "not a zip archive"u8.ToArray());
        using TemporaryDirectory cache = new();

        await Assert.ThrowsAnyAsync<Exception>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache)));

        Assert.Equal([".lock"], Directory.GetFileSystemEntries(Path.Combine(cache.Path, "chrome", "stable")).Select(Path.GetFileName));
    }

    [Fact]
    public async Task ConcurrentLocatorsInstallOnce()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);

        string[] paths = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => FindChromeAsync(options)));

        Assert.Single(paths.Distinct());
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(LatestVersion, "linux64")));
    }

    [Fact]
    public async Task LockHeldByAnotherInstallerTimesOut()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        string scopeDirectory = Path.Combine(cache.Path, "chrome", "stable");
        Directory.CreateDirectory(scopeDirectory);
        using FileStream heldLock = new(Path.Combine(scopeDirectory, ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            LockTimeout = TimeSpan.FromMilliseconds(300),
        };

        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => FindChromeAsync(options));

        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task LockFileRemainsAfterRelease()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();

        await FindChromeAsync(TestDownloadOptions.Create(server, cache));

        string lockFile = Path.Combine(cache.Path, "chrome", "stable", ".lock");
        using FileStream reacquired = new(lockFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [Fact]
    public async Task DriverLaunchWithBrowserAndDriverInstalledNeedsNoNetworkRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, BrowserPlatform.Current);
        string platformIdentifier = GetCurrentChromePlatformIdentifier();

        // The downloaded "driver" is a text file, so starting it fails once it has been located.
        await StartChromeDriverLauncherAsync(options);
        int requestCount = server.RequestedUrls.Count;
        await StartChromeDriverLauncherAsync(options);

        Assert.Equal(1, server.RequestCount(DriverArchivePath(LatestVersion, platformIdentifier)));
        Assert.Equal(requestCount, server.RequestedUrls.Count);
    }

    [Fact]
    public async Task DriverMatchesCachedBrowserVersionRatherThanChannelLatest()
    {
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", NewerVersion, LatestVersion);
        CacheSeeder.SeedInstallation(cache, "chrome/stable", LatestVersion, ChromeExecutablePath(GetCurrentChromePlatformIdentifier()));
        File.WriteAllText(
            Path.Combine(cache.Path, "chrome", "stable", "resolved-versions.json"),
            new JsonObject() { ["latest"] = new JsonObject() { ["version"] = LatestVersion, ["resolvedAt"] = timeProvider.GetUtcNow() } }.ToJsonString());
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, BrowserPlatform.Current, timeProvider);

        await StartChromeDriverLauncherAsync(options);

        Assert.Equal(1, server.RequestCount(DriverArchivePath(LatestVersion, GetCurrentChromePlatformIdentifier())));
        Assert.Equal(0, server.RequestCount(DriverArchivePath(NewerVersion, GetCurrentChromePlatformIdentifier())));
    }

    [Fact]
    public async Task UnreachableServiceFallsBackToInstalledDriver()
    {
        using TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, timeProvider: timeProvider);
        string? installedPath = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.UseSystemInstallLocation, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        await server.DisposeAsync();

        timeProvider.Advance(TimeSpan.FromHours(25));
        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.UseSystemInstallLocation, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
    }

    [Fact]
    public async Task SkipDownloadUsesInstalledVersionWithoutChecking()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "chrome/stable", OlderVersion, ChromeExecutablePath("linux64"));
        CacheSeeder.SeedResolvedVersion(cache, "chrome/stable", "latest", OlderVersion, DateTimeOffset.UtcNow.AddDays(-30));

        string path = await FindChromeAsync(TestDownloadOptions.Create(server, cache, skipDownload: true));

        Assert.Equal(installedPath, path);
        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task SkipDownloadWithNothingInstalledFailsWithoutNetworkRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(TestDownloadOptions.Create(server, cache, skipDownload: true)));

        Assert.Contains("SkipDownload", exception.Message);
        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task SkipDownloadUsesInstalledSpecificVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "chrome/stable", OlderVersion, ChromeExecutablePath("linux64"));
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, skipDownload: true);

        string path = await FindChromeAsync(options, BrowserVersion.Specific(OlderVersion));

        Assert.Equal(installedPath, path);
        await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(options, BrowserVersion.Specific(LatestVersion)));
        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task SkipDownloadUsesInstalledDriverWithoutChecking()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "drivers/chromedriver", OlderVersion, "chromedriver");
        CacheSeeder.SeedResolvedVersion(cache, "drivers/chromedriver", "latest-stable", OlderVersion, DateTimeOffset.UtcNow.AddDays(-30));
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, skipDownload: true);

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.UseSystemInstallLocation, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(installedPath, path);
        await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Beta, BrowserVersion.Latest, FileLocationBehavior.UseSystemInstallLocation, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, BrowserVersion.Specific(LatestVersion), FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(server.RequestedUrls);
    }

    private static Task<string> FindChromeAsync(BrowserDownloadOptions options, BrowserVersion? version = null)
    {
        return BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, BrowserReleaseChannel.Stable, version ?? BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static async Task StartChromeDriverLauncherAsync(BrowserDownloadOptions options)
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingDriver().WithDownloadOptions(options).Build();
        await Assert.ThrowsAnyAsync<Exception>(() => launcher.StartAsync(TestContext.Current.CancellationToken));
    }

    private static string GetCurrentChromePlatformIdentifier()
    {
        return BrowserPlatform.Current switch
        {
            { OperatingSystem: OperatingSystemFamily.MacOS, Architecture: System.Runtime.InteropServices.Architecture.Arm64 } => "mac-arm64",
            { OperatingSystem: OperatingSystemFamily.MacOS } => "mac-x64",
            { OperatingSystem: OperatingSystemFamily.Windows, Architecture: System.Runtime.InteropServices.Architecture.X64 } => "win64",
            { OperatingSystem: OperatingSystemFamily.Windows } => "win32",
            _ => "linux64",
        };
    }

    private static async Task AssertDownloadFailedAsync(Func<Task> action)
    {
        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(action);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }
}
