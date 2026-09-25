// <copyright file="EdgeTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

public class EdgeTests
{
    // The version the fake browser reports.
    private const string FakeBrowserVersion = "130.0.2849.80";

    public static TheoryData<OperatingSystemFamily, BrowserReleaseChannel, string[]> InstallLocations => new()
    {
        { OperatingSystemFamily.MacOS, BrowserReleaseChannel.Stable, ["Microsoft Edge.app", "Contents", "MacOS", "Microsoft Edge"] },
        { OperatingSystemFamily.MacOS, BrowserReleaseChannel.Alpha, ["Microsoft Edge Canary.app", "Contents", "MacOS", "Microsoft Edge Canary"] },
        { OperatingSystemFamily.Linux, BrowserReleaseChannel.Stable, ["opt", "microsoft", "msedge", "msedge"] },
        { OperatingSystemFamily.Linux, BrowserReleaseChannel.Beta, ["opt", "microsoft", "msedge-beta", "msedge"] },
        { OperatingSystemFamily.Windows, BrowserReleaseChannel.Stable, ["Microsoft", "Edge", "Application", "msedge.exe"] },
        { OperatingSystemFamily.Windows, BrowserReleaseChannel.DeveloperPreview, ["Microsoft", "Edge Dev", "Application", "msedge.exe"] },
        { OperatingSystemFamily.Windows, BrowserReleaseChannel.Alpha, ["Microsoft", "Edge SxS", "Application", "msedge.exe"] },
    };

    [Theory]
    [MemberData(nameof(InstallLocations))]
    public async Task InstalledEdgeOfTheChannelIsLocated(OperatingSystemFamily operatingSystem, BrowserReleaseChannel channel, string[] expectedPathEnd)
    {
        BrowserDownloadOptions options = new() { Platform = new BrowserPlatform(operatingSystem, Architecture.X64) };

        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Edge, channel, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.EndsWith(Path.Combine(expectedPathEnd), path);
    }

    [Fact]
    public async Task LocatingRejectsWhatEdgeDoesNotHave()
    {
        CancellationToken token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Edge, BrowserReleaseChannel.ExtendedSupport, cancellationToken: token));
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Edge, version: BrowserVersion.Specific(FakeBrowserVersion), cancellationToken: token));
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, cancellationToken: token));
        Assert.Equal("/opt/edge/msedge", await BrowserLocator.FindBrowserAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: "/opt/edge/msedge", cancellationToken: token));
    }

    [Fact]
    public async Task DriverMatchingTheInstalledEdgeIsDownloadedAndVerified()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        byte[] archive = TestArchives.Zip("msedgedriver");
        string archivePath = DriverArchivePath(FakeBrowserVersion, "linux64");
        server.AddResponses(archivePath, DriverResponse(archive, Md5Of(archive)));
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache);

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);
        string? cachedPath = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, options);

        Assert.Equal(Path.Combine(cache.Path, "drivers", "msedgedriver", FakeBrowserVersion, "msedgedriver"), path);
        Assert.Equal(path, cachedPath);
        Assert.Equal(1, server.RequestCount(archivePath));
    }

    [Theory]
    [InlineData(OperatingSystemFamily.Windows, Architecture.X64, "win64", "msedgedriver.exe")]
    [InlineData(OperatingSystemFamily.Windows, Architecture.X86, "win32", "msedgedriver.exe")]
    [InlineData(OperatingSystemFamily.Windows, Architecture.Arm64, "arm64", "msedgedriver.exe")]
    [InlineData(OperatingSystemFamily.MacOS, Architecture.X64, "mac64", "msedgedriver")]
    public async Task DriverIsDownloadedForThePlatform(OperatingSystemFamily operatingSystem, Architecture architecture, string platformIdentifier, string executableName)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(DriverArchivePath(FakeBrowserVersion, platformIdentifier), TestArchives.Zip(executableName));
        using TemporaryDirectory cache = new();

        string? path = await FindDriverAsync(FakeBrowserSetup.ExecutablePath, TestDownloadOptions.Create(server, cache, new BrowserPlatform(operatingSystem, architecture)));

        Assert.Equal(Path.Combine(cache.Path, "drivers", "msedgedriver", FakeBrowserVersion, executableName), path);
    }

    [Fact]
    public async Task DriverWhoseMd5DoesNotMatchIsRejected()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        byte[] archive = TestArchives.Zip("msedgedriver");
        server.AddResponses(DriverArchivePath(FakeBrowserVersion, "linux64"), DriverResponse(archive[..^10], Md5Of(archive)));
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindDriverAsync(FakeBrowserSetup.ExecutablePath, TestDownloadOptions.Create(server, cache)));

        Assert.Contains("MD5", exception.Message);
    }

    [Fact]
    public async Task VersionIsReadFromTheApplicationBundle()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        server.AddFile(DriverArchivePath("131.0.2903.51", "mac64_m1"), TestArchives.Zip("msedgedriver"));
        using TemporaryDirectory cache = new();
        using TemporaryDirectory applications = new();
        string executablePath = CreateApplicationBundle(applications.Path, "<key>CFBundleShortVersionString</key>\n\t<string>131.0.2903.51</string>");

        string? path = await FindDriverAsync(executablePath, TestDownloadOptions.Create(server, cache, new BrowserPlatform(OperatingSystemFamily.MacOS, Architecture.Arm64)));

        Assert.Equal(Path.Combine(cache.Path, "drivers", "msedgedriver", "131.0.2903.51", "msedgedriver"), path);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("not runnable")]
    [InlineData("bundle without version")]
    public async Task DriverIsNotFoundWhenTheVersionOfEdgeCannotBeRead(string kind)
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        using TemporaryDirectory browser = new();
        string executablePath = kind switch
        {
            "missing" => Path.Combine(browser.Path, "msedge"),
            "not runnable" => CreateFile(Path.Combine(browser.Path, "msedge")),
            _ => CreateApplicationBundle(browser.Path, "<key>CFBundleName</key>\n\t<string>Microsoft Edge</string>"),
        };

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindDriverAsync(executablePath, TestDownloadOptions.Create(server, cache)));

        Assert.Contains("could not be read", exception.Message);
        Assert.Contains("MSEDGEDRIVER_EXECUTABLE", exception.Message);
    }

    [Fact]
    public async Task DriverIsNotFoundWhenEdgeWritesNoVersion()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Windows has no echo executable to stand in for such a browser.");
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindDriverAsync("/bin/echo", TestDownloadOptions.Create(server, cache)));

        Assert.Contains("could not be read", exception.Message);
    }

    [Fact]
    public async Task NoDriverIsPublishedForLinuxOnArm()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => FindDriverAsync(FakeBrowserSetup.ExecutablePath, TestDownloadOptions.Create(server, cache, new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.Arm64))));

        Assert.Contains("publishes no msedgedriver", exception.Message);
    }

    [Fact]
    public void BuilderCreatesEdgeLaunchers()
    {
        Assert.IsType<EdgeLauncher>(BrowserLauncher.Configure(BrowserKind.Edge).Build());
        Assert.Equal(ConnectionKind.Pipes, Assert.IsType<EdgeLauncher>(BrowserLauncher.Configure(BrowserKind.Edge).WithConnection(ConnectionKind.Pipes).Build()).ConnectionType);
        Assert.IsType<EdgeDriverLauncher>(BrowserLauncher.Configure(BrowserKind.Edge).WithReleaseChannel(BrowserReleaseChannel.Beta).LaunchUsingDriver().Build());
        Assert.IsType<EdgeLauncher>(BrowserLauncher.Configure(BrowserKind.Edge).WithReleaseChannel(BrowserReleaseChannel.DeveloperPreview).WithDownloadOptions(new BrowserDownloadOptions()).Build());
        Assert.IsType<EdgeLauncher>(BrowserLauncher.Configure(BrowserKind.Edge).WithReleaseChannel(BrowserReleaseChannel.Alpha).AtLocation("/opt/edge/msedge").Build());
        Assert.False(BrowserLauncher.Configure(BrowserKind.Edge).ConnectToExisting(new Uri("ws://127.0.0.1:9222/devtools/browser/1")).Build().IsBrowserCloseAllowed);
    }

    [Fact]
    public void BuilderRejectsWhatEdgeDoesNotHave()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Edge).WithVersion(BrowserVersion.Specific(FakeBrowserVersion)).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Edge).WithReleaseChannel(BrowserReleaseChannel.ExtendedSupport).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Edge).LaunchUsingDriver().WithSessionCapability("ms:edgeOptions", "value").Build);
    }

    private static Task<string?> FindDriverAsync(string browserPath, BrowserDownloadOptions options)
    {
        return DriverLocator.FindDriverAsync(BrowserKind.Edge, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: browserPath, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static string DriverArchivePath(string version, string platform) => $"{TestDownloadOptions.EdgeDriverPath}{version}/edgedriver_{platform}.zip";

    private static ServedResponse DriverResponse(byte[] body, string md5)
    {
        return new ServedResponse(HttpStatusCode.OK, body, new Dictionary<string, string>() { ["Content-MD5"] = md5 });
    }

    private static string Md5Of(byte[] content) => Convert.ToBase64String(MD5.HashData(content));

    private static string CreateFile(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "not a browser");
        return path;
    }

    // An application bundle whose Info.plist holds the given entries.
    private static string CreateApplicationBundle(string directory, string entries)
    {
        string contents = Path.Combine(directory, "Microsoft Edge.app", "Contents");
        CreateFile(Path.Combine(contents, "Info.plist"));
        File.WriteAllText(Path.Combine(contents, "Info.plist"), $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<plist version=\"1.0\">\n<dict>\n\t{entries}\n</dict>\n</plist>\n");
        return CreateFile(Path.Combine(contents, "MacOS", "Microsoft Edge"));
    }
}
