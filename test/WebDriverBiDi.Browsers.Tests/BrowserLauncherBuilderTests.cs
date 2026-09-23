// <copyright file="BrowserLauncherBuilderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using WebDriverBiDi.Browsers.TestUtilities;

public class BrowserLauncherBuilderTests
{
    private static readonly Uri GridUrl = new("http://grid.example:4444/");

    [Fact]
    public void WithDownloadOptionsRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).WithDownloadOptions(null!));
    }

    [Fact]
    public void BuildRejectsDownloadOptionsWithRemoteGrid()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Chrome)
            .LaunchUsingRemoteGrid(GridUrl)
            .WithDownloadOptions(new BrowserDownloadOptions());

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public void BuildRejectsLocalLaunchForAnotherPlatform()
    {
        OperatingSystemFamily otherOperatingSystem = BrowserPlatform.Current.OperatingSystem == OperatingSystemFamily.Windows
            ? OperatingSystemFamily.Linux
            : OperatingSystemFamily.Windows;
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox)
            .WithDownloadOptions(new BrowserDownloadOptions() { Platform = new BrowserPlatform(otherOperatingSystem, Architecture.X64) });

        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);

        Assert.Contains(BrowserPlatform.Current.ToString(), exception.Message);
    }

    [Fact]
    public void BuildRejectsLaunchOptionsWithRemoteGrid()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(GridUrl).WithArguments("--custom").Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(GridUrl).WithEnvironmentVariable("NAME", "value").Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(GridUrl).WithoutDefaultArguments().Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(GridUrl).WithUserDataDirectory("/profile").Build);
    }

    [Fact]
    public void BuildAllowsLaunchTimeoutWithRemoteGrid()
    {
        BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(GridUrl).WithLaunchTimeout(TimeSpan.FromSeconds(3)).Build();

        Assert.Equal(TimeSpan.FromSeconds(3), launcher.InitializationTimeout);
    }

    [Fact]
    public void BuildRejectsBrowserOptionsWithRemoteGrid()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox).LaunchUsingRemoteGrid(GridUrl).WithBrowserOptions(new FirefoxLaunchOptions());

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome)]
    [InlineData(BrowserKind.Safari)]
    public void BuildRejectsBrowserOptionsForAnotherBrowser(BrowserKind browser)
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser).LaunchUsingDriver().WithBrowserOptions(new FirefoxLaunchOptions());
        if (browser == BrowserKind.Safari)
        {
            builder.AtDefaultInstallationLocation();
        }

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public void BuildRejectsArgumentsForSafari()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Safari).LaunchUsingDriver().AtDefaultInstallationLocation().WithArguments("--custom");

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public void LaunchOptionMethodsRejectInvalidValues()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.Throws<ArgumentNullException>(() => builder.WithArguments(null!));
        Assert.Throws<ArgumentNullException>(() => builder.WithArguments("--valid", null!));
        Assert.Throws<ArgumentNullException>(() => builder.WithoutDefaultArguments(null!));
        Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable(string.Empty, "value"));
        Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable("NAME=VALUE", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithUserDataDirectory(" "));
        Assert.Throws<ArgumentNullException>(() => builder.WithBrowserOptions(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithLaunchTimeout(TimeSpan.Zero));
    }

    [Theory]
    [InlineData("some.preference", 1.5)]
    [InlineData("some.preference", 5L)]
    [InlineData("some.preference", null)]
    [InlineData(" ", true)]
    public void BuildRejectsFirefoxPreferencesOfUnsupportedTypesOrNames(string name, object? value)
    {
        FirefoxLaunchOptions options = new();
        options.Preferences[name] = value!;
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox).WithBrowserOptions(options);

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public async Task BuildPassesDownloadOptionsToLauncher()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ChromeForTestingService.Serve(server, "Stable", "130.0.6723.58");
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, BrowserPlatform.Current);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).WithDownloadOptions(options).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        // The downloaded "browser" is a text file, so starting it fails once it has been located.
        await Assert.ThrowsAnyAsync<Exception>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Equal(1, server.RequestCount(ChromeForTestingService.ChannelDocumentPath));
        Assert.True(Directory.Exists(Path.Combine(cache.Path, "chrome", "stable", "130.0.6723.58")));
    }

    [Fact]
    public async Task BuildPassesHeadlessShellAndMilestoneToLocator()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ChromeForTestingService.Serve(server, "Stable", "130.0.6723.58", "129.0.6668.100");
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, BrowserPlatform.Current);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithVersion(BrowserVersion.Milestone(129))
            .WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true })
            .WithDownloadOptions(options)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        // The downloaded "browser" is a text file, so starting it fails once it has been located.
        await Assert.ThrowsAnyAsync<Exception>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Equal(1, server.RequestCount(ChromeForTestingService.MilestoneDocumentPath));
        Assert.True(Directory.Exists(Path.Combine(cache.Path, "chrome-headless-shell", "stable", "129.0.6668.100")));
    }

    [Fact]
    public void BuildRejectsHeadlessShellAtDefaultInstallationLocation()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Chrome).AtDefaultInstallationLocation().WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true });

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public void BuildRejectsMilestoneForFirefox()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox).WithVersion(BrowserVersion.Milestone(131));

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public void BuildRejectsPinnedFirefoxNightly()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox).WithReleaseChannel(BrowserReleaseChannel.Alpha).WithVersion(BrowserVersion.Specific("133.0a1"));

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome)]
    [InlineData(BrowserKind.Safari)]
    public void BuildRejectsExtendedSupportChannelWithoutExtendedSupportReleases(BrowserKind browser)
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser).WithReleaseChannel(BrowserReleaseChannel.ExtendedSupport);
        if (browser == BrowserKind.Safari)
        {
            builder.LaunchUsingDriver().AtDefaultInstallationLocation();
        }

        Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);
    }

    [Fact]
    public async Task BuildAcceptsFirefoxExtendedSupportChannel()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).WithReleaseChannel(BrowserReleaseChannel.ExtendedSupport).Build();

        Assert.IsType<FirefoxLauncher>(launcher);
    }
}
