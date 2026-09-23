// <copyright file="LauncherReadinessTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Browsers.TestUtilities;

public class LauncherReadinessTests
{
    [Fact]
    public async Task ChromeLaunchReportsDevToolsEndpoint()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();

        BrowserInstance instance = await launcher.LaunchBrowserAsync();

        Assert.Equal($"ws://127.0.0.1:{launcher.Port}/devtools/browser/fake-browser", instance.ConnectionString);
        Assert.True(launcher.IsRunning);
        await launcher.QuitBrowserAsync();
        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public async Task ChromeLaunchReportsExitCodeWhenBrowserExitsBeforeReady()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "exit:3");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("exited with code 3", exception.Message);
    }

    [Fact]
    public async Task ChromeLaunchTimesOutWhenBrowserNeverReportsReady()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.InitializationTimeout = TimeSpan.FromSeconds(1);
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("did not report its DevTools endpoint within 1 seconds", exception.Message);
    }

    [Fact]
    public async Task FirefoxLaunchReportsSessionEndpoint()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();

        BrowserInstance instance = await launcher.LaunchBrowserAsync();

        Assert.Equal($"ws://localhost:{launcher.Port}/session", instance.ConnectionString);
        Assert.True(launcher.IsRunning);
        await launcher.QuitBrowserAsync();
        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public async Task FirefoxLaunchReportsExitCodeWhenBrowserExitsBeforeReady()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "exit:3");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("exited with code 3", exception.Message);
    }
}
