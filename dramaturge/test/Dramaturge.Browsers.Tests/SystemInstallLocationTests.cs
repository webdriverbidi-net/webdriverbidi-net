// <copyright file="SystemInstallLocationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Runtime.InteropServices;

// The paths are computed from the configured platform, so each is checked on any host by its
// platform-specific ending; the host's own folders supply the beginning.
public class SystemInstallLocationTests
{
    public static TheoryData<BrowserKind, BrowserReleaseChannel, OperatingSystemFamily, string> Locations => new()
    {
        { BrowserKind.Chrome, BrowserReleaseChannel.Stable, OperatingSystemFamily.MacOS, "Google Chrome.app/Contents/MacOS/Google Chrome" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Beta, OperatingSystemFamily.MacOS, "Google Chrome Beta.app/Contents/MacOS/Google Chrome Beta" },
        { BrowserKind.Chrome, BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.MacOS, "Google Chrome Dev.app/Contents/MacOS/Google Chrome Dev" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Alpha, OperatingSystemFamily.MacOS, "Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, "usr/bin/google-chrome" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Beta, OperatingSystemFamily.Linux, "usr/bin/google-chrome-beta" },
        { BrowserKind.Chrome, BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.Linux, "usr/bin/google-chrome-unstable" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Alpha, OperatingSystemFamily.Linux, "usr/bin/google-chrome-unstable" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Stable, OperatingSystemFamily.Windows, "Google/Chrome/Application/chrome.exe" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Beta, OperatingSystemFamily.Windows, "Google/Chrome Beta/Application/chrome.exe" },
        { BrowserKind.Chrome, BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.Windows, "Google/Chrome Dev/Application/chrome.exe" },
        { BrowserKind.Chrome, BrowserReleaseChannel.Alpha, OperatingSystemFamily.Windows, "Google/Chrome SxS/Application/chrome.exe" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Stable, OperatingSystemFamily.MacOS, "Firefox.app/Contents/MacOS/firefox" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Beta, OperatingSystemFamily.MacOS, "Firefox.app/Contents/MacOS/firefox" },
        { BrowserKind.Firefox, BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.MacOS, "Firefox Developer Edition.app/Contents/MacOS/firefox" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Alpha, OperatingSystemFamily.MacOS, "Firefox Nightly.app/Contents/MacOS/firefox" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Stable, OperatingSystemFamily.Linux, "usr/bin/firefox" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Beta, OperatingSystemFamily.Linux, "usr/bin/firefox-beta" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Alpha, OperatingSystemFamily.Linux, "usr/bin/firefox-nightly" },
        { BrowserKind.Firefox, BrowserReleaseChannel.ExtendedSupport, OperatingSystemFamily.Linux, "usr/bin/firefox-esr" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Stable, OperatingSystemFamily.Windows, "Mozilla Firefox/firefox.exe" },
        { BrowserKind.Firefox, BrowserReleaseChannel.DeveloperPreview, OperatingSystemFamily.Windows, "Firefox Developer Edition/firefox.exe" },
        { BrowserKind.Firefox, BrowserReleaseChannel.Alpha, OperatingSystemFamily.Windows, "Mozilla Firefox Nightly/firefox.exe" },
    };

    [Theory]
    [MemberData(nameof(Locations))]
    public async Task SystemInstallationIsFoundAtPlatformLocation(BrowserKind browser, BrowserReleaseChannel channel, OperatingSystemFamily operatingSystem, string expectedEnding)
    {
        BrowserDownloadOptions options = new() { Platform = new BrowserPlatform(operatingSystem, Architecture.X64) };

        string path = await BrowserLocator.FindBrowserAsync(browser, channel, locationBehavior: FileLocationBehavior.UseSystemInstallLocation, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        Assert.EndsWith(Path.Combine(expectedEnding.Split('/')), path);
    }

    [Fact]
    public async Task CustomLocationIsReturnedUnchanged()
    {
        string chromePath = await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: "/opt/chrome/chrome", cancellationToken: TestContext.Current.CancellationToken);
        string firefoxPath = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: "/opt/firefox/firefox", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/opt/chrome/chrome", chromePath);
        Assert.Equal("/opt/firefox/firefox", firefoxPath);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome)]
    [InlineData(BrowserKind.Firefox)]
    public async Task CustomLocationRequiresPath(BrowserKind browser)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => BrowserLocator.FindBrowserAsync(browser, locationBehavior: FileLocationBehavior.UseCustomLocation, cancellationToken: TestContext.Current.CancellationToken));
    }
}
