// <copyright file="FirefoxLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;

// Browser downloads are served as 404s: that is enough to show which URL was requested and which
// version was parsed (the install directory is created before downloading), without running the
// macOS and Windows extractors on content they cannot extract.
public class FirefoxLocatorTests
{
    private const string GeckoDriverVersion = "0.36.0";

    public static TheoryData<OperatingSystemFamily, Architecture, string, string, string> LatestReleasePlatforms => new()
    {
        { OperatingSystemFamily.Linux, Architecture.X64, "linux64", "linux-x86_64/en-US/firefox-131.0.2.tar.xz", "firefox/firefox" },
        { OperatingSystemFamily.Linux, Architecture.Arm64, "linux64-aarch64", "linux-aarch64/en-US/firefox-131.0.2.tar.xz", "firefox/firefox" },
        { OperatingSystemFamily.Linux, Architecture.X86, "linux", "linux-i686/en-US/firefox-131.0.2.tar.xz", "firefox/firefox" },
        { OperatingSystemFamily.MacOS, Architecture.X64, "osx", "mac/en-US/Firefox%20131.0.2.dmg", "Firefox.app/Contents/MacOS/firefox" },
        { OperatingSystemFamily.MacOS, Architecture.Arm64, "osx", "mac/en-US/Firefox%20131.0.2.dmg", "Firefox.app/Contents/MacOS/firefox" },
        { OperatingSystemFamily.Windows, Architecture.X64, "win64", "win64/en-US/Firefox%20Setup%20131.0.2.exe", "firefox/firefox.exe" },
        { OperatingSystemFamily.Windows, Architecture.Arm64, "win64-aarch64", "win64-aarch64/en-US/Firefox%20Setup%20131.0.2.exe", "firefox/firefox.exe" },
        { OperatingSystemFamily.Windows, Architecture.X86, "win", "win32/en-US/Firefox%20Setup%20131.0.2.exe", "firefox/firefox.exe" },
    };

    public static TheoryData<BrowserReleaseChannel, string, string, string, string> Channels => new()
    {
        { BrowserReleaseChannel.Stable, "stable", "firefox-latest", "pub/firefox/releases/131.0.2/linux-x86_64/en-US/firefox-131.0.2.tar.xz", "131.0.2" },
        { BrowserReleaseChannel.Beta, "beta", "firefox-beta-latest", "pub/firefox/releases/132.0b9/linux-x86_64/en-US/firefox-132.0b9.tar.xz", "132.0b9" },
        { BrowserReleaseChannel.DeveloperPreview, "dev", "firefox-devedition-latest", "pub/devedition/releases/132.0b9/linux-x86_64/en-US/firefox-132.0b9.tar.xz", "132.0b9" },
        { BrowserReleaseChannel.ExtendedSupport, "esr", "firefox-esr-latest", "pub/firefox/releases/140.16.0esr/linux-x86_64/en-US/firefox-140.16.0esr.tar.xz", "140.16.0esr" },
    };

    // Firefox for Linux is .tar.bz2 through version 134.
    public static TheoryData<BrowserReleaseChannel, OperatingSystemFamily, Architecture, string, string> PinnedReleases => new()
    {
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, Architecture.X64, "128.0", "/archive/firefox/releases/128.0/linux-x86_64/en-US/firefox-128.0.tar.bz2" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, Architecture.X64, "135.0", "/archive/firefox/releases/135.0/linux-x86_64/en-US/firefox-135.0.tar.xz" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, Architecture.X64, "136", "/archive/firefox/releases/136/linux-x86_64/en-US/firefox-136.tar.xz" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, Architecture.Arm64, "140.0", "/archive/firefox/releases/140.0/linux-aarch64/en-US/firefox-140.0.tar.xz" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, Architecture.X86, "128.0", "/archive/firefox/releases/128.0/linux-i686/en-US/firefox-128.0.tar.bz2" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.MacOS, Architecture.Arm64, "128.0", "/archive/firefox/releases/128.0/mac/en-US/Firefox%20128.0.dmg" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Windows, Architecture.X64, "128.0", "/archive/firefox/releases/128.0/win64/en-US/Firefox%20Setup%20128.0.exe" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Windows, Architecture.Arm64, "128.0", "/archive/firefox/releases/128.0/win64-aarch64/en-US/Firefox%20Setup%20128.0.exe" },
        { BrowserReleaseChannel.Stable, OperatingSystemFamily.Windows, Architecture.X86, "128.0", "/archive/firefox/releases/128.0/win32/en-US/Firefox%20Setup%20128.0.exe" },
        { BrowserReleaseChannel.Beta, OperatingSystemFamily.Linux, Architecture.X64, "141.0b3", "/archive/firefox/releases/141.0b3/linux-x86_64/en-US/firefox-141.0b3.tar.xz" },
        { BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.Linux, Architecture.X64, "141.0b3", "/archive/devedition/releases/141.0b3/linux-x86_64/en-US/firefox-141.0b3.tar.xz" },
        { BrowserReleaseChannel.ExtendedSupport, OperatingSystemFamily.Linux, Architecture.X64, "115.20.0esr", "/archive/firefox/releases/115.20.0esr/linux-x86_64/en-US/firefox-115.20.0esr.tar.bz2" },
    };

    public static TheoryData<OperatingSystemFamily, Architecture, string> GeckoDriverPlatforms => new()
    {
        { OperatingSystemFamily.Linux, Architecture.X64, "linux64.tar.gz" },
        { OperatingSystemFamily.Linux, Architecture.X86, "linux32.tar.gz" },
        { OperatingSystemFamily.Linux, Architecture.Arm64, "linux-aarch64.tar.gz" },
        { OperatingSystemFamily.MacOS, Architecture.Arm64, "macos-aarch64.tar.gz" },
        { OperatingSystemFamily.MacOS, Architecture.X64, "macos.tar.gz" },
        { OperatingSystemFamily.Windows, Architecture.X64, "win64.zip" },
        { OperatingSystemFamily.Windows, Architecture.X86, "win32.zip" },
        { OperatingSystemFamily.Windows, Architecture.Arm64, "win-aarch64.zip" },
    };

    // The redirect target's version is already installed, so the locator returns it without
    // downloading, which shows both the product service request and the parsed version.
    [Theory]
    [MemberData(nameof(LatestReleasePlatforms))]
    public async Task FindBrowserFollowsProductServiceRedirectForPlatform(OperatingSystemFamily operatingSystem, Architecture architecture, string serviceOperatingSystem, string archiveRelativePath, string executablePath)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor($"/pub/firefox/releases/131.0.2/{archiveRelativePath}"));
        using TemporaryDirectory cache = new();
        string installedPath = CacheSeeder.SeedInstallation(cache, "firefox/stable", "131.0.2", executablePath);
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

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

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, channel, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken);

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
        string checksumsPath = ServeChecksums(server, archivePath, new string('0', 64));
        using TemporaryDirectory cache = new();
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox)
            .WithReleaseChannel(BrowserReleaseChannel.Alpha)
            .WithDownloadOptions(TestDownloadOptions.Create(server, cache, BrowserPlatform.Current))
            .Build();
        List<string> messages = [];
        launcher.OnLogMessage.AddObserver(e => messages.Add(e.Message));
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await AssertDownloadFailedAsync(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Equal("/mozilla/?product=firefox-nightly-latest-ssl", server.RequestedUrls[0].Split("&os=")[0]);
        Assert.Contains("Downloading Firefox Nightly 133.0a1...", messages);
        Assert.Equal("/pub/firefox/nightly/latest-mozilla-central/firefox-133.0a1.en-US.linux-x86_64.checksums", checksumsPath);
        Assert.Equal(1, server.RequestCount(checksumsPath));
    }

    [Theory]
    [MemberData(nameof(PinnedReleases))]
    public async Task FindBrowserDownloadsPinnedVersionFromArchive(BrowserReleaseChannel channel, OperatingSystemFamily operatingSystem, Architecture architecture, string version, string archivePath)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddNotFound(archivePath);
        string checksumsPath = ServeChecksums(server, archivePath, new string('0', 64));
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        await AssertDownloadFailedAsync(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, channel, BrowserVersion.Specific(version), FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([checksumsPath, archivePath], server.RequestedUrls);
    }

    [Fact]
    public async Task FindBrowserRejectsPinnedNightly()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Alpha, BrowserVersion.Specific("133.0a1"), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("only the latest", exception.Message);
    }

    [Fact]
    public async Task FindBrowserRejectsMilestone()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, version: BrowserVersion.Milestone(131), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindBrowserRejectsPlatformWithoutBuilds()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.Arm));

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));

        Assert.IsType<PlatformNotSupportedException>(exception.InnerException);
        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task FindBrowserReportsRedirectWithoutVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddRedirect(TestDownloadOptions.FirefoxProductPath, server.UrlFor("/unexpected/firefox.tar.xz"));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Fact]
    public async Task FindDriverRejectsPlatformWithoutGeckoDriver()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGeckoDriverRelease(server);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.Arm));

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));

        Assert.IsType<PlatformNotSupportedException>(exception.InnerException);
    }

    // geckodriver does not follow the Firefox version, so a Firefox that is not downloaded is not asked for its version.
    [Fact]
    public async Task GeckoDriverForFirefoxAtCustomLocationIsTheLatest()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGeckoDriverRelease(server);
        using TemporaryDirectory cache = new();

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Firefox, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: Path.Combine(cache.Path, "firefox"), downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "geckodriver", GeckoDriverVersion, "geckodriver"), path);
    }

    [Theory]
    [MemberData(nameof(GeckoDriverPlatforms))]
    public async Task FindDriverDownloadsGeckoDriverAssetForPlatform(OperatingSystemFamily operatingSystem, Architecture architecture, string assetSuffix)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ServeGeckoDriverRelease(server);
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture));

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Firefox, BrowserReleaseChannel.Stable, BrowserVersion.Latest, FileLocationBehavior.AutoLocateAndDownload, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        string driverFileName = operatingSystem == OperatingSystemFamily.Windows ? "geckodriver.exe" : "geckodriver";
        Assert.Equal(Path.Combine(cache.Path, "drivers", "geckodriver", GeckoDriverVersion, driverFileName), path);
        Assert.True(File.Exists(path));
        Assert.Equal(["/gecko/latest", GeckoDriverAssetPath(assetSuffix)], server.RequestedUrls);
    }

    private static string GeckoDriverAssetPath(string assetSuffix) => $"/gecko/download/geckodriver-v{GeckoDriverVersion}-{assetSuffix}";

    // Assets are listed in the order GitHub returns them for a geckodriver release, each archive
    // after its signature, whose name also contains the platform.
    private static void ServeGeckoDriverRelease(DownloadServer server)
    {
        string[] assetSuffixes = ["linux-aarch64.tar.gz", "linux32.tar.gz", "linux64.tar.gz", "macos-aarch64.tar.gz", "macos.tar.gz", "win-aarch64.zip", "win32.zip", "win64.zip"];
        JsonArray assets = [];
        foreach (string assetSuffix in assetSuffixes)
        {
            string assetPath = GeckoDriverAssetPath(assetSuffix);
            bool isWindows = assetSuffix.StartsWith("win", StringComparison.Ordinal);
            byte[] archive = isWindows ? TestArchives.Zip("geckodriver.exe") : TestArchives.TarGz("geckodriver");
            server.AddFile(assetPath, archive);
            assets.Add(new JsonObject() { ["name"] = $"{assetPath.Split('/')[^1]}.asc", ["browser_download_url"] = server.UrlFor($"{assetPath}.asc").AbsoluteUri });
            assets.Add(new JsonObject()
            {
                ["name"] = assetPath.Split('/')[^1],
                ["browser_download_url"] = server.UrlFor(assetPath).AbsoluteUri,
                ["digest"] = $"sha256:{Convert.ToHexStringLower(SHA256.HashData(archive))}",
                ["size"] = archive.Length,
            });
        }

        JsonObject release = new() { ["tag_name"] = $"v{GeckoDriverVersion}", ["name"] = GeckoDriverVersion, ["assets"] = assets };
        server.AddText("/gecko/latest", release.ToJsonString());
    }

    // Serves the checksums file Mozilla publishes for an archive, listing a hash for it, and returns its path.
    internal static string ServeChecksums(DownloadServer server, string archivePath, string sha256)
    {
        string path = Uri.UnescapeDataString(archivePath);
        string fileName = path[(path.LastIndexOf('/') + 1)..];
        int releasesIndex = path.IndexOf("/releases/", StringComparison.Ordinal);
        string checksumsPath;
        string line;
        if (releasesIndex < 0)
        {
            checksumsPath = path[..path.IndexOf(".tar.", StringComparison.Ordinal)] + ".checksums";
            line = $"{new string('1', 128)} sha512 100 {fileName}\n{sha256} sha256 100 {fileName}\n";
        }
        else
        {
            int versionEnd = path.IndexOf('/', releasesIndex + "/releases/".Length) + 1;
            checksumsPath = path[..versionEnd] + "SHA256SUMS";
            line = $"{new string('2', 64)}  other/file.txt\n{sha256}  {path[versionEnd..]}\n";
        }

        server.AddText(checksumsPath, line);
        return checksumsPath;
    }

    private static async Task AssertDownloadFailedAsync(Func<Task> action)
    {
        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(action);
        Assert.Contains("(404)", exception.Message);
    }
}
