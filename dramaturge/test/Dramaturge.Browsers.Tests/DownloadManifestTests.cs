// <copyright file="DownloadManifestTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Dramaturge.Browsers.TestUtilities;

// The download server serves only the manifest and the builds it lists, so any request to a
// vendor service would fail.
public class DownloadManifestTests
{
    private const string ManifestPath = "/mirror/manifest.json";
    private const string ChromeVersion = "130.0.6723.58";
    private const string ChromeExecutable = "chrome-linux64/chrome";

    [Fact]
    public async Task ChannelResolvesToListedVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", "stable", ChromeVersion);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, "linux-x64", "chrome/chrome-linux64.zip", TestArchives.Zip(ChromeExecutable));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string path = await FindChromeAsync(server, cache);

        Assert.Equal(Path.Combine(cache.Path, "chrome", "stable", ChromeVersion, TestDownloadOptions.ToLocalPath(ChromeExecutable)), path);
        Assert.Equal([ManifestPath, "/mirror/chrome/chrome-linux64.zip"], server.RequestedUrls);
    }

    [Fact]
    public async Task SpecificVersionIsFoundWithoutChannel()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("browsers", "chrome", "129.0.6668.100", "linux-x64", "chrome/129.zip", TestArchives.Zip(ChromeExecutable));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string path = await FindChromeAsync(server, cache, version: BrowserVersion.Specific("129.0.6668.100"));

        Assert.Contains("129.0.6668.100", path);
    }

    [Fact]
    public async Task MilestoneResolvesToHighestListedVersionOfMilestone()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        foreach (string version in new[] { "129.0.6668.9", "129.0.6668.100", "130.0.6723.58", "129-custom-build" })
        {
            manifest.AddBuild("browsers", "chrome", version, "linux-x64", $"chrome/{version}.zip", TestArchives.Zip(ChromeExecutable));
        }

        manifest.Serve();
        using TemporaryDirectory cache = new();

        string path = await FindChromeAsync(server, cache, version: BrowserVersion.Milestone(129));

        Assert.Contains("129.0.6668.100", path);
    }

    [Fact]
    public async Task HeadlessShellIsListedUnderItsOwnName()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome-headless-shell", "stable", ChromeVersion);
        manifest.AddBuild("browsers", "chrome-headless-shell", ChromeVersion, "linux-x64", "shell.zip", TestArchives.Zip("chrome-headless-shell-linux64/chrome-headless-shell"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: CreateOptions(server, cache), browserOptions: new ChromeLaunchOptions() { UseHeadlessShell = true }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task FirefoxIsResolvedAndExtractedFromManifest()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "firefox", "esr", "140.3.0esr");
        manifest.AddBuild("browsers", "firefox", "140.3.0esr", "linux-x64", "firefox-140.3.0esr.tar.xz", TestArchives.TarGz("firefox/firefox"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.ExtendedSupport, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "firefox", "esr", "140.3.0esr", "firefox", "firefox"), path);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task ChromeDriverFollowsBrowserChannelVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", "stable", ChromeVersion);
        manifest.AddBuild("drivers", "chromedriver", ChromeVersion, "linux-x64", "chromedriver-linux64.zip", TestArchives.Zip("chromedriver-linux64/chromedriver"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "chromedriver", ChromeVersion, "chromedriver"), path);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ChromeDriverFollowsRequestedBrowserVersion(bool byMilestone)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("browsers", "chrome", "129.0.6668.100", "linux-x64", "chrome-129.zip", TestArchives.Zip(ChromeExecutable));
        manifest.AddBuild("drivers", "chromedriver", "129.0.6668.100", "linux-x64", "chromedriver-129.zip", TestArchives.Zip("chromedriver-linux64/chromedriver"));
        manifest.Serve();
        using TemporaryDirectory cache = new();
        BrowserVersion version = byMilestone ? BrowserVersion.Milestone(129) : BrowserVersion.Specific("129.0.6668.100");

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Chrome, version: version, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "chromedriver", "129.0.6668.100", "chromedriver"), path);
    }

    [Fact]
    public async Task EdgeDriverIsTheListedBuildOfTheInstalledEdgeVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("drivers", "msedgedriver", "130.0.2849.80", "linux-x64", "edgedriver_linux64.zip", TestArchives.Zip("msedgedriver"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: FakeBrowserSetup.ExecutablePath, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "msedgedriver", "130.0.2849.80", "msedgedriver"), path);
    }

    public static TheoryData<string[], string> ListedChromeDriverBuilds => new()
    {
        { ["130.0.2849.95:linux-x64", "130.0.2850.1:linux-x64"], "130.0.2849.95" },
        { ["130.0.2900.1:linux-x64", "129.0.6668.100:linux-x64"], "130.0.2900.1" },
        { ["130.0.2849.99:windows-x64", "130.0.2849.70:linux-x64"], "130.0.2849.70" },
    };

    [Theory]
    [MemberData(nameof(ListedChromeDriverBuilds))]
    public async Task ChromeDriverForInstalledChromeIsTheClosestListedAndRemembered(string[] listedBuilds, string expectedVersion)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        foreach (string listed in listedBuilds)
        {
            string[] parts = listed.Split(':');
            manifest.AddBuild("drivers", "chromedriver", parts[0], parts[1], $"chromedriver-{parts[0]}-{parts[1]}.zip", TestArchives.Zip("chromedriver-linux64/chromedriver"));
        }

        manifest.Serve();
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = CreateOptions(server, cache);

        string? path = await FindInstalledBrowserDriverAsync(BrowserKind.Chrome, options);
        string? cachedPath = await FindInstalledBrowserDriverAsync(BrowserKind.Chrome, options);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "chromedriver", expectedVersion, "chromedriver"), path);
        Assert.Equal(path, cachedPath);
        Assert.Equal(1, server.RequestCount(ManifestPath));
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, "chromedriver", "129.0.6668.100")]
    [InlineData(BrowserKind.Edge, "msedgedriver", "130.0.2849.95")]
    public async Task DriverWithoutACompatibleListedVersionFails(BrowserKind browser, string driverName, string listedVersion)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("drivers", driverName, listedVersion, "linux-x64", "driver.zip", TestArchives.Zip(driverName));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindInstalledBrowserDriverAsync(browser, CreateOptions(server, cache)));

        Assert.Contains($"lists no {driverName} 130.0.2849.80", exception.Message);
    }

    [Fact]
    public async Task MilestoneTheManifestDoesNotListIsReported()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, "linux-x64", "chrome.zip", TestArchives.Zip(ChromeExecutable));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, version: BrowserVersion.Milestone(999), downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("lists no chrome version of milestone 999", exception.Message);
    }

    [Fact]
    public async Task GeckoDriverUsesListedLatestVersion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.SetLatest("geckodriver", "0.36.0");
        manifest.AddBuild("drivers", "geckodriver", "0.36.0", "linux-x64", "geckodriver-v0.36.0-linux64.tar.gz", TestArchives.TarGz("geckodriver"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        string? path = await DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "geckodriver", "0.36.0", "geckodriver"), path);
    }

    [Fact]
    public async Task GeckoDriverWithoutListedLatestVersionFails()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddBuild("drivers", "geckodriver", "0.36.0", "linux-x64", "geckodriver.tar.gz", TestArchives.TarGz("geckodriver"));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => DriverLocator.FindDriverAsync(BrowserKind.Firefox, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("lists no latest version of geckodriver", exception.Message);
    }

    [Theory]
    [InlineData("beta", "linux-x64", "lists no stable channel of chrome")]
    [InlineData("stable", "macos-arm64", $"lists no chrome {ChromeVersion} build for linux-x64")]
    public async Task UnlistedBuildIsNotDownloadedFromVendor(string listedChannel, string listedPlatform, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", listedChannel, ChromeVersion);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, listedPlatform, "chrome.zip", TestArchives.Zip(ChromeExecutable));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(server, cache));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.Equal([ManifestPath], server.RequestedUrls);
    }

    [Fact]
    public async Task UnlistedBrowserFails()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        new ManifestBuilder(server).Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("lists no firefox builds", exception.Message);
    }

    [Fact]
    public async Task BuildNotMatchingListedHashIsRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", "stable", ChromeVersion);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, "linux-x64", "chrome.zip", TestArchives.Zip(ChromeExecutable), sha256: new string('a', 64));
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(server, cache));

        Assert.Contains($"its publisher lists {new string('a', 64)}", exception.Message);
        Assert.Empty(Directory.GetDirectories(Path.Combine(cache.Path, "chrome", "stable")));
    }

    [Fact]
    public async Task BuildNotMatchingListedSizeIsRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", "stable", ChromeVersion);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, "linux-x64", "chrome.zip", TestArchives.Zip(ChromeExecutable), sizeAdjustment: 1);
        manifest.Serve();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(server, cache));

        Assert.Contains("bytes, but its publisher lists", exception.Message);
    }

    [Theory]
    [InlineData("null", "only 1 is supported")]
    [InlineData("{\"schemaVersion\": 1, \"browsers\": {\"chrome\": {\"channels\": {\"stable\": \"1.0\"}, \"versions\": {\"1.0\": {\"linux-x64\": {\"url\": \"\", \"sha256\": \"0000000000000000000000000000000000000000000000000000000000000000\"}}}}}}", "without a URL and a 64-digit hexadecimal sha256")]
    [InlineData("{\"schemaVersion\": 1, \"browsers\": {\"chrome\": {\"channels\": {\"stable\": \"1.0\"}, \"versions\": {\"1.0\": {\"linux-x64\": {\"url\": \"chrome.zip\", \"sha256\": \"zz00000000000000000000000000000000000000000000000000000000000000\"}}}}}}", "without a URL and a 64-digit hexadecimal sha256")]
    [InlineData("{\"schemaVersion\": 1, \"browsers\": {\"chrome\": {\"channels\": {\"stable\": \"1.0\"}, \"versions\": {}}}}", "lists no chrome 1.0.")]
    [InlineData("{\"schemaVersion\": 2}", "only 1 is supported")]
    [InlineData("{\"browsers\": {}}", "only 1 is supported")]
    [InlineData("not json", "is not valid JSON")]
    [InlineData("{\"schemaVersion\": 1, \"browsers\": {\"chrome\": {\"channels\": {\"stable\": \"1.0\"}, \"versions\": {\"1.0\": {\"linux-x64\": {\"url\": \"chrome.zip\", \"sha256\": \"abc\"}}}}}}", "without a URL and a 64-digit hexadecimal sha256")]
    public async Task InvalidManifestIsReported(string json, string expectedMessage)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddText(ManifestPath, json);
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindChromeAsync(server, cache));

        Assert.Contains(expectedMessage, exception.Message);
    }

    // A folder holding the manifest and the builds it lists, by relative URL, needs no server.
    [Fact]
    public async Task FileManifestListsBuildsBesideIt()
    {
        using TemporaryDirectory mirror = new();
        using TemporaryDirectory cache = new();
        byte[] archive = TestArchives.Zip(ChromeExecutable);
        Directory.CreateDirectory(Path.Combine(mirror.Path, "chrome"));
        File.WriteAllBytes(Path.Combine(mirror.Path, "chrome", "chrome linux.zip"), archive);
        JsonObject build = new() { ["url"] = "chrome/chrome%20linux.zip", ["sha256"] = Sha256Of(archive) };
        JsonObject manifest = new()
        {
            ["schemaVersion"] = 1,
            ["browsers"] = new JsonObject()
            {
                ["chrome"] = new JsonObject()
                {
                    ["channels"] = new JsonObject() { ["stable"] = ChromeVersion },
                    ["versions"] = new JsonObject() { [ChromeVersion] = new JsonObject() { ["linux-x64"] = build } },
                },
            },
        };
        string manifestPath = Path.Combine(mirror.Path, "manifest.json");
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        BrowserDownloadOptions options = new() { CacheDirectory = cache.Path, Platform = TestDownloadOptions.DefaultPlatform, ManifestUrl = new Uri(manifestPath) };

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void ManifestUrlMustBeAbsoluteHttpOrFileUrl()
    {
        Assert.Throws<ArgumentException>(() => new BrowserDownloadOptions() { ManifestUrl = new Uri("manifest.json", UriKind.Relative) });
        Assert.Throws<ArgumentException>(() => new BrowserDownloadOptions() { ManifestUrl = new Uri("ftp://mirror.example/manifest.json") });
    }

    [Theory]
    [InlineData(OperatingSystemFamily.Linux, Architecture.Arm64, "linux-arm64", "linux-arm64")]
    [InlineData(OperatingSystemFamily.MacOS, Architecture.Arm64, "macos-arm64", "mac-arm64")]
    [InlineData(OperatingSystemFamily.MacOS, Architecture.X64, "macos-x64", "mac-x64")]
    [InlineData(OperatingSystemFamily.Windows, Architecture.X64, "windows-x64", "win64")]
    [InlineData(OperatingSystemFamily.Windows, Architecture.X86, "windows-x86", "win32")]
    [InlineData(OperatingSystemFamily.Windows, Architecture.Arm64, "windows-arm64", "win64")]
    public async Task BuildsAreListedUnderNeutralPlatformNames(OperatingSystemFamily operatingSystem, Architecture architecture, string platformKey, string chromePlatformIdentifier)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ManifestBuilder manifest = new(server);
        manifest.AddChannel("browsers", "chrome", "stable", ChromeVersion);
        string executablePath = ChromeForTestingService.ChromeExecutablePath(chromePlatformIdentifier);
        manifest.AddBuild("browsers", "chrome", ChromeVersion, platformKey, "chrome.zip", TestArchives.Zip(executablePath));
        manifest.Serve();
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = new() { CacheDirectory = cache.Path, Platform = new BrowserPlatform(operatingSystem, architecture), ManifestUrl = server.UrlFor(ManifestPath) };

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(cache.Path, "chrome", "stable", ChromeVersion, TestDownloadOptions.ToLocalPath(executablePath)), path);
    }

    private static BrowserDownloadOptions CreateOptions(DownloadServer server, TemporaryDirectory cache)
    {
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        return new BrowserDownloadOptions() { CacheDirectory = defaults.CacheDirectory, Platform = defaults.Platform, ManifestUrl = server.UrlFor(ManifestPath) };
    }

    private static Task<string> FindChromeAsync(DownloadServer server, TemporaryDirectory cache, BrowserVersion? version = null)
    {
        return BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, version: version, downloadOptions: CreateOptions(server, cache), cancellationToken: TestContext.Current.CancellationToken);
    }

    // The fake browser, as an installed browser, reports version 130.0.2849.80.
    private static Task<string?> FindInstalledBrowserDriverAsync(BrowserKind browser, BrowserDownloadOptions options)
    {
        return DriverLocator.FindDriverAsync(browser, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: FakeBrowserSetup.ExecutablePath, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    // Builds a manifest served beside the builds it lists, which are served at URLs relative to it.
    private sealed class ManifestBuilder(DownloadServer server)
    {
        private readonly JsonObject browsers = [];
        private readonly JsonObject drivers = [];

        public void AddChannel(string section, string product, string channel, string version)
        {
            this.GetProduct(section, product)["channels"]![channel] = version;
        }

        public void SetLatest(string driver, string version)
        {
            this.GetProduct("drivers", driver)["latest"] = version;
        }

        public void AddBuild(string section, string product, string version, string platform, string relativeUrl, byte[] content, string? sha256 = null, int sizeAdjustment = 0)
        {
            server.AddFile($"/mirror/{relativeUrl}", content);
            JsonObject versions = this.GetProduct(section, product)["versions"]!.AsObject();
            if (versions[version] is not JsonObject builds)
            {
                builds = [];
                versions[version] = builds;
            }

            builds[platform] = new JsonObject() { ["url"] = relativeUrl, ["sha256"] = sha256 ?? Sha256Of(content), ["size"] = content.Length + sizeAdjustment };
        }

        public void Serve()
        {
            server.AddText(ManifestPath, new JsonObject() { ["schemaVersion"] = 1, ["browsers"] = this.browsers, ["drivers"] = this.drivers }.ToJsonString());
        }

        private JsonObject GetProduct(string section, string product)
        {
            JsonObject products = section == "browsers" ? this.browsers : this.drivers;
            if (products[product] is not JsonObject entry)
            {
                entry = new JsonObject() { ["channels"] = new JsonObject(), ["versions"] = new JsonObject() };
                products[product] = entry;
            }

            return entry;
        }
    }
}
