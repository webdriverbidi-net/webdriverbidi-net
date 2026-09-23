// <copyright file="BuilderValidationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Protocol;

public class BuilderValidationTests
{
    private static readonly Uri GridUrl = new("http://grid.example/");
    private static readonly Uri WebSocketUrl = new("ws://127.0.0.1:9222/session");

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    public void WithPortRejectsOutOfRangePort(int port)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).WithPort(port));
    }

    [Fact]
    public async Task WithRandomPortResetsPort()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).WithPort(9222).WithRandomPort().Build();

        Assert.Equal(0, launcher.Port);
    }

    [Fact]
    public void ArgumentMethodsRejectMissingValues()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Chrome);

        Assert.Throws<ArgumentException>(() => builder.AtLocation(" "));
        Assert.Throws<ArgumentNullException>(() => builder.WithVersion(null!));
        Assert.Throws<ArgumentNullException>(() => builder.WithBrowserOptions(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithLaunchTimeout(TimeSpan.Zero));
        Assert.Throws<ArgumentException>(() => builder.LaunchUsingRemoteGrid(null!));
        Assert.Throws<ArgumentException>(() => builder.ConnectToExisting(null!));
    }

    [Theory]
    [MemberData(nameof(ConflictingLocations))]
    public void ConflictingLocationIsRejectedNamingBothChoices(Func<BrowserLauncherBuilder, BrowserLauncherBuilder> first, Func<BrowserLauncherBuilder, BrowserLauncherBuilder> second, string expectedMessage)
    {
        BrowserLauncherBuilder builder = first(BrowserLauncher.Configure(BrowserKind.Chrome));

        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(() => second(builder));

        Assert.Equal(expectedMessage, exception.Message);
    }

    public static TheoryData<Func<BrowserLauncherBuilder, BrowserLauncherBuilder>, Func<BrowserLauncherBuilder, BrowserLauncherBuilder>, string> ConflictingLocations => new()
    {
        { b => b.AtLocation("/opt/chrome"), b => b.AtDefaultInstallationLocation(), "Cannot specify to use the system-installed browser; you already specified to use a custom browser location (/opt/chrome)." },
        { b => b.AtDefaultInstallationLocation(), b => b.AtLocation("/opt/chrome"), "Cannot specify to use a custom browser location; you already specified to use the system-installed browser." },
        { b => b.AtLocation("/opt/chrome"), b => b.AtAutomaticallyDownloadedLocation(), "Cannot specify to auto-download the browser; you already specified to use a custom browser location (/opt/chrome)." },
        { b => b.LaunchUsingDriver(), b => b.LaunchUsingRemoteGrid(GridUrl), "Cannot specify to connect to remote grid; you already specified to launch via driver executable." },
        { b => b.LaunchUsingRemoteGrid(GridUrl), b => b.LaunchUsingDriver(), $"Cannot specify to launch via driver executable; you already specified to connect to remote grid ({GridUrl})." },
        { b => b.ConnectToExisting(WebSocketUrl), b => b.LaunchUsingDriver(), $"Cannot specify to launch via driver executable; you already specified to connect to an existing browser ({WebSocketUrl})." },
    };

    [Fact]
    public async Task RepeatingSameChoiceIsAllowed()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .AtAutomaticallyDownloadedLocation()
            .AtAutomaticallyDownloadedLocation()
            .LaunchUsingDriver()
            .LaunchUsingDriver()
            .Build();

        Assert.IsType<ChromeDriverLauncher>(launcher);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, BrowserReleaseChannel.ExtendedSupport)]
    [InlineData(BrowserKind.Chrome, (BrowserReleaseChannel)99)]
    [InlineData(BrowserKind.Firefox, (BrowserReleaseChannel)99)]
    [InlineData(BrowserKind.Safari, BrowserReleaseChannel.Beta)]
    public void BuildRejectsChannelBrowserDoesNotHave(BrowserKind browser, BrowserReleaseChannel channel)
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser).WithReleaseChannel(channel);
        if (browser == BrowserKind.Safari)
        {
            builder.LaunchUsingDriver().AtDefaultInstallationLocation();
        }

        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(builder.Build);

        Assert.Contains($"Invalid browser release channel for {browser}", exception.Message);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, BrowserReleaseChannel.Beta)]
    [InlineData(BrowserKind.Chrome, BrowserReleaseChannel.DeveloperPreview)]
    [InlineData(BrowserKind.Chrome, BrowserReleaseChannel.Alpha)]
    [InlineData(BrowserKind.Firefox, BrowserReleaseChannel.Beta)]
    [InlineData(BrowserKind.Firefox, BrowserReleaseChannel.DeveloperPreview)]
    public async Task BuildAcceptsEveryChannelOfBrowser(BrowserKind browser, BrowserReleaseChannel channel)
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).WithReleaseChannel(channel).AtDefaultInstallationLocation().Build();

        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public void BuildRejectsUnknownBrowser()
    {
        BrowserLauncherConfigurationException exception = Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure((BrowserKind)99).Build);

        Assert.Contains("Unknown browser type", exception.Message);
    }

    [Fact]
    public void BuildRejectsPipesForBrowserOtherThanChromeOrWithDriver()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).WithConnection(ConnectionKind.Pipes).Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).WithConnection(ConnectionKind.Pipes).LaunchUsingDriver().Build);
    }

    [Fact]
    public void BuildRejectsSafariWithoutDriverOrSystemInstallation()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Safari).AtDefaultInstallationLocation().Build);
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Safari).LaunchUsingDriver().Build);
    }

    [Fact]
    public async Task SafariCanConnectToExistingBrowser()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Safari).ConnectToExisting(WebSocketUrl).Build();

        Assert.Equal("ExistingBrowserLauncher", launcher.GetType().Name);
    }

    [Fact]
    public void BuildRejectsFirefoxPreferencesWithoutName()
    {
        FirefoxLaunchOptions options = new();
        options.Preferences[" "] = true;

        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Firefox).WithBrowserOptions(options).Build);
    }
}
