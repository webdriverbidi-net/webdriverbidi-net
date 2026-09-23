// <copyright file="BrowserLauncherBuilderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;
using WebDriverBiDi.Browsers.TestUtilities;

public class BrowserLauncherBuilderTests
{
    [Fact]
    public void WithDownloadOptionsRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => BrowserLauncher.Configure(BrowserKind.Chrome).WithDownloadOptions(null!));
    }

    [Fact]
    public void BuildRejectsDownloadOptionsWithRemoteGrid()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Chrome)
            .LaunchUsingRemoteGrid("grid.example")
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
    public async Task BuildPassesDownloadOptionsToLauncher()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        ChromeForTestingService.Serve(server, "Stable", "130.0.6723.58");
        using TemporaryDirectory cache = new();
        BrowserDownloadOptions options = TestDownloadOptions.Create(server, cache, BrowserPlatform.Current);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).WithDownloadOptions(options).Build();
        await launcher.StartAsync();

        // The downloaded "browser" is a text file, so starting it fails once it has been located.
        await Assert.ThrowsAnyAsync<Exception>(launcher.LaunchBrowserAsync);

        Assert.Equal(1, server.RequestCount(ChromeForTestingService.ChannelDocumentPath));
        Assert.True(Directory.Exists(Path.Combine(cache.Path, "chrome", "stable", "130.0.6723.58")));
    }
}
