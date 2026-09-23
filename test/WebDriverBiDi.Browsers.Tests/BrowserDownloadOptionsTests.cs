// <copyright file="BrowserDownloadOptionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;

public class BrowserDownloadOptionsTests
{
    [Fact]
    public void LockTimeoutMustBePositiveOrInfinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BrowserDownloadOptions() { LockTimeout = TimeSpan.Zero });
        Assert.Equal(Timeout.InfiniteTimeSpan, new BrowserDownloadOptions() { LockTimeout = Timeout.InfiniteTimeSpan }.LockTimeout);
    }

    [Fact]
    public void DefaultsUsePublicDownloadServicesAndCurrentPlatform()
    {
        BrowserDownloadOptions options = new();

        Assert.Equal(BrowserDownloadOptions.DefaultCacheDirectory, options.CacheDirectory);
        Assert.Null(options.Platform);
        Assert.Same(TimeProvider.System, options.TimeProvider);
        Assert.Null(options.HttpClient);
        Assert.Equal("https://googlechromelabs.github.io/chrome-for-testing/", options.ChromeForTestingEndpoint.AbsoluteUri);
        Assert.Equal("https://download.mozilla.org/", options.FirefoxProductEndpoint.AbsoluteUri);
        Assert.Equal("https://download-installer.cdn.mozilla.net/pub/", options.FirefoxArchiveEndpoint.AbsoluteUri);
        Assert.Equal("https://api.github.com/repos/mozilla/geckodriver/releases/", options.GeckoDriverReleasesEndpoint.AbsoluteUri);
    }

    [Fact]
    public void EndpointsGainTrailingSlashSoRelativePathsResolveBelowThem()
    {
        BrowserDownloadOptions options = new()
        {
            ChromeForTestingEndpoint = new Uri("https://mirror.example/cft"),
            FirefoxProductEndpoint = new Uri("https://mirror.example/mozilla"),
            FirefoxArchiveEndpoint = new Uri("https://mirror.example/archive"),
            GeckoDriverReleasesEndpoint = new Uri("https://mirror.example/gecko"),
        };

        Assert.Equal("https://mirror.example/cft/", options.ChromeForTestingEndpoint.AbsoluteUri);
        Assert.Equal("https://mirror.example/mozilla/", options.FirefoxProductEndpoint.AbsoluteUri);
        Assert.Equal("https://mirror.example/archive/", options.FirefoxArchiveEndpoint.AbsoluteUri);
        Assert.Equal("https://mirror.example/gecko/", options.GeckoDriverReleasesEndpoint.AbsoluteUri);
    }

    [Fact]
    public void EndpointRejectsRelativeUrl()
    {
        Assert.Throws<ArgumentException>(() => new BrowserDownloadOptions() { ChromeForTestingEndpoint = new Uri("cft/", UriKind.Relative) });
    }

    [Fact]
    public void EndpointRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BrowserDownloadOptions() { GeckoDriverReleasesEndpoint = null! });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CacheDirectoryRejectsEmptyValue(string cacheDirectory)
    {
        Assert.Throws<ArgumentException>(() => new BrowserDownloadOptions() { CacheDirectory = cacheDirectory });
    }

    [Fact]
    public void TimeProviderRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BrowserDownloadOptions() { TimeProvider = null! });
    }

    [Fact]
    public void CurrentPlatformMatchesRunningProcess()
    {
        BrowserPlatform platform = BrowserPlatform.Current;

        OperatingSystemFamily expected = OperatingSystem.IsWindows() ? OperatingSystemFamily.Windows
            : OperatingSystem.IsMacOS() ? OperatingSystemFamily.MacOS
            : OperatingSystemFamily.Linux;
        Assert.Equal(expected, platform.OperatingSystem);
        Assert.Equal(RuntimeInformation.ProcessArchitecture, platform.Architecture);
    }

    [Fact]
    public void PlatformDescribesItself()
    {
        Assert.Equal("MacOS-Arm64", new BrowserPlatform(OperatingSystemFamily.MacOS, Architecture.Arm64).ToString());
    }
}
