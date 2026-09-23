// <copyright file="FirefoxLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;

// Browser downloads are served as 404s: that is enough to show which URL was requested and which
// version was parsed (the install directory is created before downloading), without running the
// macOS and Windows extractors on content they cannot extract.
public class FirefoxLocatorTests
{
    private const string GeckoDriverVersion = "0.36.0";

    public static TheoryData<OperatingSystemFamily, string, string, string> LatestReleasePlatforms => new()
    {
        { OperatingSystemFamily.Linux, "linux64", "linux-x86_64/en-US/firefox-131.0.2.tar.xz", "firefox/firefox" },
        { OperatingSystemFamily.MacOS, "osx", "mac/en-US/Firefox%20131.0.2.dmg", "Firefox.app/Contents/MacOS/firefox" },
        { OperatingSystemFamily.Windows, "win64", "win64/en-US/Firefox%20Setup%20131.0.2.exe", "firefox/firefox.exe" },
    };

    public static TheoryData<BrowserReleaseChannel, string, string, string, string> Channels => new()
    {
        { BrowserReleaseChannel.Stable, "stable", "firefox-latest", "pub/firefox/releases/131.0.2/linux-x86_64/en-US/firefox-131.0.2.tar.xz", "131.0.2" },
        { BrowserReleaseChannel.Beta, "beta", "firefox-beta-latest", "pub/firefox/releases/132.0b9/linux-x86_64/en-US/firefox-132.0b9.tar.xz", "132.0b9" },
        { BrowserReleaseChannel.DeveloperPreview, "dev", "firefox-devedition-latest", "pub/devedition/releases/132.0b9/linux-x86_64/en-US/firefox-132.0b9.tar.xz", "132.0b9" },
    };

    public static TheoryData<OperatingSystemFamily, string> PinnedReleasePlatforms => new()
    {
        { OperatingSystemFamily.Linux, "/archive/128.0/linux-x86_64/en-US/firefox-128.0.tar.xz" },
        { OperatingSystemFamily.MacOS, "/archive/128.0/mac/en-US/Firefox%20128.0.dmg" },
        { OperatingSystemFamily.Windows, "/archive/128.0/win64/en-US/Firefox%20Setup%20128.0.exe" },
    };

    public static TheoryData<OperatingSystemFamily, Architecture, string> GeckoDriverPlatforms => new()
    {
        { OperatingSystemFamily.Linux, Architecture.X64, "linux64.tar.gz" },
        { OperatingSystemFamily.MacOS, Architecture.Arm64, "macos-aarch64.tar.gz" },
        { OperatingSystemFamily.Windows, Architecture.X64, "win64.zip" },
        { OperatingSystemFamily.Windows, Architecture.X86, "win32.zip" },
    };

    // The redirect target's version is already installed, so the locator returns it without
    // downloading, which shows both the product service request and the parsed version.
    [Theory]
    [MemberData(nameof(LatestReleasePlatforms))]
    public async Task FindBrowserFollowsProductServiceRedirectForPlatform(OperatingSystemFamily operatingSystem, string serviceOperatingSystem, string archiveRelativePath, string executablePath)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor($"/pub/firefox/releases/131.0.2/{archiveRelativePath}"));
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/stable", "131.0.2", executablePath);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, Architecture.X64));

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        Assert.Equal(installedPath, path);
        Assert.Equal([$"/mozilla/?product=firefox-latest-ssl&os={serviceOperatingSystem}&lang=en-US"], server.RequestedUrls);
    }

    [Theory]
    [MemberData(nameof(Channels))]
    public async Task FindBrowserRequestsProductForChannelAndParsesVersion(BrowserReleaseChannel channel, string channelDirectory, string product, string archiveRelativePath, string version)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archiveRelativePath));
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, $"firefox/{channelDirectory}", version, "firefox/firefox");

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, channel, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: TestDownloadOptions.Create(server, cache));

        Assert.Equal(installedPath, path);
        Assert.Equal([$"/mozilla/?product={product}-ssl&os=linux64&lang=en-US"], server.RequestedUrls);
    }

    // Nightly builds share a version number, so Nightly is downloaded again whenever it is rechecked,
    // installed or not; the parsed version shows in the download log message.
    [Fact]
    public async Task FindBrowserParsesNightlyVersionFromBuildFileName()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        string archivePath = "/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.tar.xz";
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor(archivePath));
        server.AddNotFound(archivePath);
        using TemporaryDirectory cache = new();
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox)
            .WithReleaseChannel(BrowserReleaseChannel.Alpha)
            .WithDownloadOptions(TestDownloadOptions.Create(server, cache, BrowserPlatform.Current))
            .Build();
        List<string> messages = [];
        launcher.OnLogMessage.AddObserver(e => messages.Add(e.Message));
        await launcher.StartAsync();

        await Assert.ThrowsAsync<HttpRequestException>(launcher.LaunchBrowserAsync);

        Assert.Equal("/mozilla/?product=firefox-nightly-latest-ssl", server.RequestedUrls[0].Split("&os=")[0]);
        Assert.Contains("Downloading Firefox Nightly 133.0a1...", messages);
    }

    [Theory]
    [MemberData(nameof(PinnedReleasePlatforms))]
    public async Task FindBrowserDownloadsPinnedVersionFromReleaseArchive(OperatingSystemFamily operatingSystem, string archivePath)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddNotFound(archivePath);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, Architecture.X64));

        await Assert.ThrowsAsync<HttpRequestException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Stable, BrowserVersion.Specific("128.0"), FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options));

        Assert.Equal([archivePath], server.RequestedUrls);
    }

    [Theory]
    [MemberData(nameof(GeckoDriverPlatforms))]
    public async Task FindDriverDownloadsGeckoDriverAssetForPlatform(OperatingSystemFamily operatingSystem, Architecture architecture, string assetSuffix)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGeckoDriverRelease(server);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Firefox, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options);

        string driverFileName = operatingSystem == OperatingSystemFamily.Windows ? "geckodriver.exe" : "geckodriver";
        Assert.Equal(Path.Combine(cache.Path, "drivers", "geckodriver", GeckoDriverVersion, driverFileName), path);
        Assert.True(File.Exists(path));
        Assert.Equal(["/gecko/latest", GeckoDriverAssetPath(assetSuffix)], server.RequestedUrls);
    }

    private static string GeckoDriverAssetPath(string assetSuffix) => $"/gecko/download/geckodriver-v{GeckoDriverVersion}-{assetSuffix}";

    // Assets are listed in the order GitHub returns them for a geckodriver release.
    private static void ServeGeckoDriverRelease(DownloadServer server)
    {
        string[] assetSuffixes = ["linux-aarch64.tar.gz", "linux32.tar.gz", "linux64.tar.gz", "macos-aarch64.tar.gz", "macos.tar.gz", "win-aarch64.zip", "win32.zip", "win64.zip"];
        JsonArray assets = [];
        foreach (string assetSuffix in assetSuffixes)
        {
            string assetPath = GeckoDriverAssetPath(assetSuffix);
            bool isWindows = assetSuffix.StartsWith("win", StringComparison.Ordinal);
            server.AddFile(assetPath, isWindows ? TestArchives.Zip("geckodriver.exe") : TestArchives.TarGz("geckodriver"));
            assets.Add(new JsonObject() { ["name"] = assetPath.Split('/')[^1], ["browser_download_url"] = server.UrlFor(assetPath).AbsoluteUri });
        }

        JsonObject release = new() { ["tag_name"] = $"v{GeckoDriverVersion}", ["name"] = GeckoDriverVersion, ["assets"] = assets };
        server.AddText("/gecko/latest", release.ToJsonString());
    }
}
