// <copyright file="LauncherReadinessTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

// The fake browser reads its failure mode from an environment variable it inherits, so these
// tests must not run alongside each other.
[Collection("NonParallel")]
public sealed class LauncherReadinessTests : IDisposable
{
    private const string ModeVariableName = "WEBDRIVERBIDI_FAKE_BROWSER_MODE";

    private static readonly string FakeBrowserPath = Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "WebDriverBiDi.FakeBrowser.exe" : "WebDriverBiDi.FakeBrowser");

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ModeVariableName, null);
    }

    [Fact]
    public async Task ChromeLaunchReportsDevToolsEndpoint()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).AtLocation(FakeBrowserPath).Build();
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
        Environment.SetEnvironmentVariable(ModeVariableName, "exit:3");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).AtLocation(FakeBrowserPath).Build();
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("exited with code 3", exception.Message);
    }

    [Fact]
    public async Task ChromeLaunchTimesOutWhenBrowserNeverReportsReady()
    {
        Environment.SetEnvironmentVariable(ModeVariableName, "silent");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).AtLocation(FakeBrowserPath).Build();
        launcher.InitializationTimeout = TimeSpan.FromSeconds(1);
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("did not report its DevTools endpoint within 1 seconds", exception.Message);
    }

    [Fact]
    public async Task FirefoxLaunchReportsSessionEndpoint()
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).AtLocation(FakeBrowserPath).Build();
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
        Environment.SetEnvironmentVariable(ModeVariableName, "exit:3");
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).AtLocation(FakeBrowserPath).Build();
        await launcher.StartAsync();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

        Assert.Contains("exited with code 3", exception.Message);
    }
}
