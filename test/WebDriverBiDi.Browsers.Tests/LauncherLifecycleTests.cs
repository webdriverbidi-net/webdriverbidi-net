// <copyright file="LauncherLifecycleTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

public class LauncherLifecycleTests
{
    private static readonly TimeSpan ProcessExitTimeout = TimeSpan.FromSeconds(10);

    public static TheoryData<BrowserKind> DirectLaunchBrowsers => [BrowserKind.Chrome, BrowserKind.Firefox];

    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task KillTerminatesBrowserAndItsChildProcesses(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new(startChild: true);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.KillBrowserAsync(TestContext.Current.CancellationToken);

        Assert.False(launcher.IsRunning);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
        Assert.False(fakeBrowser.ExitedGracefully);
    }

    // Windows can only ask a process to exit by closing its main window, which the fake browser does not have.
    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task QuitAsksBrowserToExitBeforeKillingIt(BrowserKind browser)
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Windows has no way to ask a windowless process to exit.");
        using FakeBrowserSetup fakeBrowser = new(startChild: true);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        Assert.False(launcher.IsRunning);
        Assert.True(fakeBrowser.ExitedGracefully);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task QuitKillsBrowserThatIgnoresRequestToExit(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "ignore-term", startChild: true);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.ShutdownTimeout = TimeSpan.FromMilliseconds(500);
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        Assert.False(launcher.IsRunning);
        Assert.False(fakeBrowser.ExitedGracefully);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, "--user-data-dir")]
    [InlineData(BrowserKind.Firefox, "--profile")]
    public async Task QuitDeletesTemporaryProfile(BrowserKind browser, string profileArgumentName)
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        string profileDirectory = fakeBrowser.GetLastLaunchArgument(profileArgumentName);
        Assert.True(Directory.Exists(profileDirectory));

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        Assert.False(Directory.Exists(profileDirectory));
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, "--user-data-dir")]
    [InlineData(BrowserKind.Firefox, "--profile")]
    public async Task FailedLaunchKillsBrowserAndDeletesProfile(BrowserKind browser, string profileArgumentName)
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent", startChild: true);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.InitializationTimeout = TimeSpan.FromSeconds(1);
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.False(launcher.IsRunning);
        Assert.False(Directory.Exists(fakeBrowser.GetLastLaunchArgument(profileArgumentName)));
        using Process child = GetProcessIfRunning(fakeBrowser);
        Assert.True(child.HasExited || child.WaitForExit(ProcessExitTimeout));
    }

    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task LaunchWithPortZeroUsesPortBrowserChose(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        Assert.Equal("0", fakeBrowser.GetLastLaunchArgument("--remote-debugging-port"));
        Assert.NotEqual(0, launcher.Port);
        Assert.Contains($":{launcher.Port}/", instance.ConnectionString);
    }

    [Fact]
    public async Task ChromePipeLaunchReportsBrowserProcess()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithConnection(ConnectionKind.Pipes)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserInstance instance = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        // A pipe launch is ready as soon as the process starts, which may be before the browser has
        // run far enough to record its arguments.
        Assert.Equal($"pipe://chrome:{instance.ProcessId}", instance.ConnectionString);
        Assert.Contains("--remote-debugging-pipe", (await fakeBrowser.WaitForLaunchAsync()).Arguments);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChromePipeLaunchEndsWithBrowserProcess(bool kill)
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithConnection(ConnectionKind.Pipes)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        ChromeLauncher chromeLauncher = Assert.IsType<ChromeLauncher>(launcher);
        bool hadPipeServer = chromeLauncher.PipeServerProcess is not null;
        Transport transport = launcher.CreateTransport();

        if (kill)
        {
            await launcher.KillBrowserAsync(TestContext.Current.CancellationToken);
        }
        else
        {
            await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);
        }

        Assert.True(hadPipeServer);
        Assert.IsType<ChromiumTransport>(transport);
        Assert.False(launcher.IsRunning);
        Assert.Null(chromeLauncher.PipeServerProcess);
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, typeof(ChromiumTransport), true)]
    [InlineData(BrowserKind.Firefox, typeof(Transport), true)]
    public async Task DirectLauncherCreatesTransportForBrowser(BrowserKind browser, Type expectedTransportType, bool isCloseAllowed)
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();

        Transport transport = launcher.CreateTransport();

        Assert.IsType(expectedTransportType, transport);
        Assert.Equal(isCloseAllowed, launcher.IsBrowserCloseAllowed);
        Assert.False(launcher.IsBiDiSessionInitialized);
    }

    [Fact]
    public async Task LaunchRemovesProfilesWhoseOwnersHaveExited()
    {
        string exitedOwner = await CreateProfileOwnedByExitedProcessAsync();
        string reusedProcessId = CreateProfile(Environment.ProcessId, Process.GetCurrentProcess().StartTime.AddHours(-1));
        string runningOwner = CreateProfile(Environment.ProcessId, Process.GetCurrentProcess().StartTime);
        string oldUnowned = CreateProfile(null, null);
        Directory.SetLastWriteTimeUtc(oldUnowned, DateTime.UtcNow.AddHours(-2));
        string recentUnowned = CreateProfile(null, null);
        try
        {
            using FakeBrowserSetup fakeBrowser = new();
            await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
            await launcher.StartAsync(TestContext.Current.CancellationToken);

            await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

            Assert.False(Directory.Exists(exitedOwner));
            Assert.False(Directory.Exists(reusedProcessId));
            Assert.False(Directory.Exists(oldUnowned));
            Assert.True(Directory.Exists(runningOwner));
            Assert.True(Directory.Exists(recentUnowned));
        }
        finally
        {
            foreach (string directory in new[] { exitedOwner, reusedProcessId, runningOwner, oldUnowned, recentUnowned }.Where(Directory.Exists))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    private static Process GetProcessIfRunning(FakeBrowserSetup fakeBrowser)
    {
        try
        {
            return fakeBrowser.GetChildProcess();
        }
        catch (ArgumentException)
        {
            // Already exited: stand in with a process known to have exited.
            Process exited = Process.Start(new ProcessStartInfo(FakeBrowserSetup.ExecutablePath) { Environment = { ["WEBDRIVERBIDI_FAKE_BROWSER_MODE"] = "exit:0" } })!;
            exited.WaitForExit();
            return exited;
        }
    }

    private static async Task<string> CreateProfileOwnedByExitedProcessAsync()
    {
        using Process process = Process.Start(new ProcessStartInfo(FakeBrowserSetup.ExecutablePath) { UseShellExecute = false })!;
        DateTime startTime = process.StartTime;
        process.Kill();
        await process.WaitForExitAsync();
        return CreateProfile(process.Id, startTime);
    }

    private static string CreateProfile(int? ownerProcessId, DateTime? ownerStartTime)
    {
        string directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"webdriverbidi-net-test-data-{Guid.NewGuid()}")).FullName;
        if (ownerProcessId is not null)
        {
            File.WriteAllText(Path.Combine(directory, ".webdriverbidi-owner"), $"{ownerProcessId}\n{ownerStartTime!.Value.ToUniversalTime().Ticks}");
        }

        return directory;
    }
}
