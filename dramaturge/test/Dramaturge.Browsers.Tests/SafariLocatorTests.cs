// <copyright file="SafariLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Runtime.InteropServices;

// Safari is never downloaded, so locating it names the paths of the macOS installation without touching them.
public class SafariLocatorTests
{
    private static readonly BrowserDownloadOptions MacOSOptions = new() { Platform = new BrowserPlatform(OperatingSystemFamily.MacOS, Architecture.Arm64) };

    public static TheoryData<BrowserReleaseChannel, string, string> Channels => new()
    {
        { BrowserReleaseChannel.Stable, "/Applications/Safari.app/Contents/MacOS/Safari", "/usr/bin/safaridriver" },
        { BrowserReleaseChannel.DeveloperPreview, "/Applications/Safari Technology Preview.app/Contents/MacOS/Safari Technology Preview", "/Applications/Safari Technology Preview.app/Contents/MacOS/safaridriver" },
    };

    [Theory]
    [MemberData(nameof(Channels))]
    public async Task FindBrowserAndDriverReturnSystemInstallationPaths(BrowserReleaseChannel channel, string browserPath, string driverPath)
    {
        Assert.Equal(browserPath, await BrowserLocator.FindBrowserAsync(BrowserKind.Safari, channel, downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(driverPath, await DriverLocator.FindDriverAsync(BrowserKind.Safari, channel, downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindBrowserReturnsCustomLocation()
    {
        string path = await BrowserLocator.FindBrowserAsync(BrowserKind.Safari, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: "/opt/Safari", downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/opt/Safari", path);
    }

    [Fact]
    public async Task FindBrowserRejectsPlatformOtherThanMacOS()
    {
        BrowserDownloadOptions options = new() { Platform = new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.X64) };

        await Assert.ThrowsAsync<NotSupportedException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Safari, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<NotSupportedException>(() => DriverLocator.FindDriverAsync(BrowserKind.Safari, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindBrowserRejectsChannelsAndVersionsSafariDoesNotHave()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Safari, BrowserReleaseChannel.Beta, downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Safari, version: BrowserVersion.Specific("18.0"), downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Safari, locationBehavior: FileLocationBehavior.UseCustomLocation, downloadOptions: MacOSOptions, cancellationToken: TestContext.Current.CancellationToken));
    }
}
