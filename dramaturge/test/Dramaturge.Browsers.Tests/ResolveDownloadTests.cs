// <copyright file="ResolveDownloadTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using Dramaturge.Browsers.TestUtilities;
using static Dramaturge.Browsers.TestUtilities.ChromeForTestingService;

public class ResolveDownloadTests
{
    private const string ChromeVersion = "131.0.6778.204";

    [Fact]
    public async Task BrowserIsResolvedWithoutDownloadingAndReportedCachedOnceInstalled()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);

        ResolvedDownload before = await BrowserLocator.ResolveDownloadAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        ResolvedDownload after = await BrowserLocator.ResolveDownloadAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        ResolvedDownload headlessShell = await BrowserLocator.ResolveDownloadAsync(BrowserKind.Chrome, downloadOptions: options, browserOptions: new ChromeLaunchOptions() { UseHeadlessShell = true }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"chrome@{ChromeVersion}", before.ToString());
        Assert.Equal(server.UrlFor(ChromeArchivePath(ChromeVersion, "linux64")), before.Url);
        Assert.False(before.IsCached);
        Assert.True(after.IsCached);
        Assert.Equal(1, server.RequestCount(ChromeArchivePath(ChromeVersion, "linux64")));
        Assert.Equal("chrome-headless-shell", headlessShell.Name);
        Assert.False(headlessShell.IsCached);
    }

    [Fact]
    public async Task NightlyIsNeverReportedCached()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = "/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.tar.xz";
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archivePath));
        using TemporaryDirectory cache = new();
        CacheSeeder.SeedInstallation(cache, "firefox/nightly", "133.0a1", "firefox/firefox");

        ResolvedDownload resolved = await BrowserLocator.ResolveDownloadAsync(BrowserKind.Firefox, BrowserReleaseChannel.Alpha, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("firefox@133.0a1", resolved.ToString());
        Assert.False(resolved.IsCached);
    }

    [Fact]
    public async Task DriverIsResolvedForTheChannelOrTheInstalledBrowser()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion, "130.0.2849.80");
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);
        await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        ResolvedDownload channel = await DriverLocator.ResolveDownloadAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        ResolvedDownload installed = await DriverLocator.ResolveDownloadAsync(BrowserKind.Chrome, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: FakeBrowserSetup.ExecutablePath, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        ResolvedDownload edge = await DriverLocator.ResolveDownloadAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: FakeBrowserSetup.ExecutablePath, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"chromedriver@{ChromeVersion}", channel.ToString());
        Assert.True(channel.IsCached);
        Assert.Equal("chromedriver@130.0.2849.80", installed.ToString());
        Assert.False(installed.IsCached);
        Assert.Equal("msedgedriver@130.0.2849.80", edge.ToString());
        Assert.Equal(server.UrlFor($"{TestDownloadOptions.EdgeDriverPath}130.0.2849.80/edgedriver_linux64.zip"), edge.Url);
    }

    [Fact]
    public async Task WhatIsNeverDownloadedCannotBeResolved()
    {
        CancellationToken token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<NotSupportedException>(() => BrowserLocator.ResolveDownloadAsync(BrowserKind.Edge, cancellationToken: token));
        await Assert.ThrowsAsync<NotSupportedException>(() => BrowserLocator.ResolveDownloadAsync(BrowserKind.Safari, cancellationToken: token));
        await Assert.ThrowsAsync<NotSupportedException>(() => DriverLocator.ResolveDownloadAsync(BrowserKind.Safari, cancellationToken: token));
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.ResolveDownloadAsync(BrowserKind.Firefox, browserOptions: new ChromeLaunchOptions(), cancellationToken: token));
    }

    [Fact]
    public async Task DriverIsResolvedWithDefaultOptions()
    {
        using TemporaryDirectory directory = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.ResolveDownloadAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: Path.Combine(directory.Path, "msedge"), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("could not be read", exception.Message);
    }

    [Fact]
    public async Task BuildThatCannotBeResolvedIsReported()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", ChromeVersion);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);

        BrowserDownloadException browser = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.ResolveDownloadAsync(BrowserKind.Chrome, version: BrowserVersion.Specific("1.2.3.4"), downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));
        BrowserDownloadException driver = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.ResolveDownloadAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: Path.Combine(cache.Path, "missing"), downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));

        Assert.StartsWith("Unable to resolve Chrome Stable", browser.Message);
        Assert.StartsWith("Unable to resolve msedgedriver", driver.Message);
    }
}
