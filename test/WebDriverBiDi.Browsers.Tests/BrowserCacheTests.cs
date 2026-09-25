// <copyright file="BrowserCacheTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

public class BrowserCacheTests
{
    private const string ChromeVersion = "131.0.6778.204";
    private const string OlderChromeVersion = "130.0.6723.58";
    private static readonly DateTimeOffset ResolvedAt = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CompleteInstallationsAreListedInOrder()
    {
        using TemporaryDirectory cache = new();
        CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, "chrome-linux64/chrome");
        CacheSeeder.SeedInstallation(cache, "chrome/canary", "133.0.6900.0", "chrome-linux64/chrome");
        CacheSeeder.SeedInstallation(cache, "firefox/nightly", "159.0a1", "firefox/firefox");
        CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");
        CacheSeeder.SeedInstallation(cache, "drivers/chromedriver", ChromeVersion, "chromedriver");
        CacheSeeder.SeedResolvedVersion(cache, "chrome/stable", "latest", ChromeVersion, ResolvedAt);
        CacheSeeder.SeedPartialInstallation(cache, "firefox/stable", "140.0", "firefox/firefox");
        CacheSeeder.SeedInstallation(cache, "chrome/stable", ".tmp-0123", "chrome-linux64/chrome");

        IReadOnlyList<CachedInstallation> installations = BrowserCache.List(Options(cache));

        Assert.Equal(
            ["chrome@133.0.6900.0 canary", $"chrome@{ChromeVersion} stable", $"chromedriver@{ChromeVersion} driver", "firefox@159.0a1 nightly", "geckodriver@0.36.0 driver"],
            installations.Select(installation => $"{installation} {(installation.IsDriver ? "driver" : installation.Channel)}"));
        CachedInstallation stable = installations[1];
        Assert.Equal(CacheSeeder.GetVersionDirectory(cache, "chrome/stable", ChromeVersion), stable.Directory);
        Assert.Equal("seeded".Length + ChromeVersion.Length, stable.Size);
        Assert.Equal(ResolvedAt, stable.LastResolved);
        Assert.Null(installations[0].LastResolved);
    }

    [Fact]
    public void CacheThatDoesNotExistOrIsEmptyListsNothingAndIsNotMarked()
    {
        using TemporaryDirectory cache = new();

        Assert.Empty(BrowserCache.List(new BrowserDownloadOptions() { CacheDirectory = Path.Combine(cache.Path, "missing") }));
        Assert.Empty(BrowserCache.List(Options(cache)));
        Assert.False(File.Exists(Path.Combine(cache.Path, ".layout-version")));
    }

    [Fact]
    public async Task RemovedBrowserIsDownloadedAgain()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        CachedInstallation installation = Assert.Single(BrowserCache.List(options));

        await BrowserCache.RemoveAsync(installation, options, TestContext.Current.CancellationToken);
        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(File.Exists(path));
        Assert.Equal(2, server.RequestCount(ChromeArchivePath(ChromeVersion, "linux64")));
        Assert.Equal("1", File.ReadAllText(Path.Combine(cache.Path, ".layout-version")));
    }

    [Fact]
    public async Task RemovingKeepsOtherVersionsAndWhatResolvedToThem()
    {
        using TemporaryDirectory cache = new();
        CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, "chrome-linux64/chrome");
        CacheSeeder.SeedInstallation(cache, "chrome/stable", OlderChromeVersion, "chrome-linux64/chrome");
        JsonObject records = new()
        {
            ["latest"] = new JsonObject() { ["version"] = ChromeVersion, ["resolvedAt"] = ResolvedAt },
            ["milestone-130"] = new JsonObject() { ["version"] = OlderChromeVersion, ["resolvedAt"] = ResolvedAt },
        };
        File.WriteAllText(Path.Combine(cache.Path, "chrome", "stable", "resolved-versions.json"), records.ToJsonString());
        CachedInstallation latest = BrowserCache.List(Options(cache)).Single(installation => installation.Version == ChromeVersion);

        await BrowserCache.RemoveAsync(latest, Options(cache), TestContext.Current.CancellationToken);

        CachedInstallation remaining = Assert.Single(BrowserCache.List(Options(cache)));
        Assert.Equal(OlderChromeVersion, remaining.Version);
        Assert.Equal(ResolvedAt, remaining.LastResolved);
        Assert.False(Directory.Exists(latest.Directory));
        JsonObject remainingRecords = JsonNode.Parse(File.ReadAllText(Path.Combine(cache.Path, "chrome", "stable", "resolved-versions.json")))!.AsObject();
        Assert.Equal(["milestone-130"], remainingRecords.Select(record => record.Key));
    }

    [Fact]
    public async Task RemovingAnInstallationAlreadyDeletedSucceeds()
    {
        using TemporaryDirectory cache = new();
        CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");
        CachedInstallation installation = Assert.Single(BrowserCache.List(Options(cache)));
        Directory.Delete(installation.Directory, true);

        await BrowserCache.RemoveAsync(installation, Options(cache), TestContext.Current.CancellationToken);

        Assert.Empty(BrowserCache.List(Options(cache)));
    }

    [Fact]
    public async Task RemovingRejectsAnInstallationOfAnotherCache()
    {
        using TemporaryDirectory cache = new();
        using TemporaryDirectory otherCache = new();
        CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");
        CachedInstallation installation = Assert.Single(BrowserCache.List(Options(cache)));

        await Assert.ThrowsAsync<ArgumentException>(() => BrowserCache.RemoveAsync(installation, Options(otherCache), TestContext.Current.CancellationToken));

        Assert.True(Directory.Exists(installation.Directory));
    }

    [Fact]
    public async Task CacheOfAnotherLayoutIsNotUsed()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        File.WriteAllText(Path.Combine(cache.Path, ".layout-version"), "2\n");

        BrowserDownloadException listException = Assert.Throws<BrowserDownloadException>(() => BrowserCache.List(Options(cache)));
        BrowserDownloadException findException = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("layout version 2", listException.Message);
        Assert.Contains("layout version 2", findException.Message);
        Assert.Equal(0, server.RequestCount(ChannelDocumentPath));
    }

    [Fact]
    public void SizeDoesNotCountLinks()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Creating links on Windows requires a privilege tests do not have.");
        using TemporaryDirectory cache = new();
        string executable = CacheSeeder.SeedInstallation(cache, "chrome/stable", ChromeVersion, "chrome-mac-arm64/Versions/1/chrome");
        string versionDirectory = CacheSeeder.GetVersionDirectory(cache, "chrome/stable", ChromeVersion);
        long unlinkedSize = Assert.Single(BrowserCache.List(Options(cache))).Size;
        Directory.CreateSymbolicLink(Path.Combine(versionDirectory, "chrome-mac-arm64", "Versions", "Current"), Path.GetDirectoryName(executable)!);
        File.CreateSymbolicLink(Path.Combine(versionDirectory, "chrome-link"), executable);

        Assert.Equal(unlinkedSize, Assert.Single(BrowserCache.List(Options(cache))).Size);
    }

    private static BrowserDownloadOptions Options(TemporaryDirectory cache) => new() { CacheDirectory = cache.Path };
}
