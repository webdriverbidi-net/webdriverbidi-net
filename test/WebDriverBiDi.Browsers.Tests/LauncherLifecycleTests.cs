// <copyright file="LauncherLifecycleTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

[Collection("NonParallel")]
public class LauncherLifecycleTests
{
    private static readonly TimeSpan ProcessExitTimeout = TimeSpan.FromSeconds(10);

    public static TheoryData<BrowserKind> DirectLaunchBrowsers => [BrowserKind.Chrome, BrowserKind.Firefox];

    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task KillTerminatesBrowserAndItsChildProcesses(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new(startChild: true);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.KillBrowserAsync();

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
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.QuitBrowserAsync();

        Assert.False(launcher.IsRunning);
        Assert.True(fakeBrowser.ExitedGracefully);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Theory]
    [MemberData(nameof(DirectLaunchBrowsers))]
    public async Task QuitKillsBrowserThatIgnoresRequestToExit(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "ignore-term", startChild: true);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.ShutdownTimeout = TimeSpan.FromMilliseconds(500);
        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        using Process child = fakeBrowser.GetChildProcess();

        await launcher.QuitBrowserAsync();

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
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        string profileDirectory = fakeBrowser.GetLastLaunchArgument(profileArgumentName);
        Assert.True(Directory.Exists(profileDirectory));

        await launcher.QuitBrowserAsync();

        Assert.False(Directory.Exists(profileDirectory));
    }

    [Theory]
    [InlineData(BrowserKind.Chrome, "--user-data-dir")]
    [InlineData(BrowserKind.Firefox, "--profile")]
    public async Task FailedLaunchKillsBrowserAndDeletesProfile(BrowserKind browser, string profileArgumentName)
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent", startChild: true);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.InitializationTimeout = TimeSpan.FromSeconds(1);
        await launcher.StartAsync();

        await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.LaunchBrowserAsync);

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
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync();

        BrowserInstance instance = await launcher.LaunchBrowserAsync();

        Assert.Equal("0", fakeBrowser.GetLastLaunchArgument("--remote-debugging-port"));
        Assert.NotEqual(0, launcher.Port);
        Assert.Contains($":{launcher.Port}/", instance.ConnectionString);
    }

    [Fact]
    public async Task ChromePipeLaunchReportsBrowserProcess()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithConnection(ConnectionKind.Pipes)
            .Build();
        await launcher.StartAsync();

        BrowserInstance instance = await launcher.LaunchBrowserAsync();

        // A pipe launch is ready as soon as the process starts, which may be before the browser has
        // run far enough to record its arguments.
        Assert.Equal($"pipe://chrome:{instance.ProcessId}", instance.ConnectionString);
        Assert.Contains("--remote-debugging-pipe", await fakeBrowser.WaitForLaunchAsync());
    }

    // The pipe launch passes the arguments through a POSIX shell on Unix, which quotes them differently.
    [Theory]
    [InlineData(BrowserKind.Chrome, ConnectionKind.WebSocket, "--user-data-dir")]
    [InlineData(BrowserKind.Chrome, ConnectionKind.Pipes, "--user-data-dir")]
    [InlineData(BrowserKind.Firefox, ConnectionKind.WebSocket, "--profile")]
    public async Task PathsWithSpacesAndQuotesReachBrowserUnchanged(BrowserKind browser, ConnectionKind connectionKind, string profileArgumentName)
    {
        // Windows does not allow double quotes in file names.
        string directoryName = OperatingSystem.IsWindows() ? "temp with spaces and 'quotes'" : "temp with \"spaces\" and 'quotes'";
        using TemporaryDirectory root = new();
        string temporaryDirectory = Directory.CreateDirectory(Path.Combine(root.Path, directoryName)).FullName;
        string executableDirectory = Directory.CreateDirectory(Path.Combine(root.Path, "fake browser")).FullName;
        foreach (string file in Directory.GetFiles(AppContext.BaseDirectory, "WebDriverBiDi.FakeBrowser*"))
        {
            File.Copy(file, Path.Combine(executableDirectory, Path.GetFileName(file)));
        }

        using FakeBrowserSetup fakeBrowser = new();
        using TemporaryDirectoryOverride temporaryDirectoryOverride = new(temporaryDirectory);
        await using BrowserLauncher launcher = BrowserLauncher.Configure(browser)
            .AtLocation(Path.Combine(executableDirectory, Path.GetFileName(FakeBrowserSetup.ExecutablePath)))
            .WithConnection(connectionKind)
            .Build();
        await launcher.StartAsync();

        await launcher.LaunchBrowserAsync();

        await fakeBrowser.WaitForLaunchAsync();
        string profileDirectory = fakeBrowser.GetLastLaunchArgument(profileArgumentName);
        Assert.StartsWith(temporaryDirectory, profileDirectory);
        Assert.True(Directory.Exists(profileDirectory));
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
            await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
            await launcher.StartAsync();

            await launcher.LaunchBrowserAsync();

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

    [Fact]
    public async Task DriverStopKillsDriverAndItsChildProcesses()
    {
        using FakeBrowserSetup fakeBrowser = new(startChild: true);
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingDriver().Build();
        await launcher.StartAsync();
        using Process child = fakeBrowser.GetChildProcess();
        Assert.True(launcher.IsRunning);

        await launcher.StopAsync();

        Assert.False(launcher.IsRunning);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Fact]
    public async Task DriverThatExitsBeforeReadyIsRetriedOnAnotherPort()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "exit:1");
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingDriver().Build();

        BrowserNotLaunchedException exception = await Assert.ThrowsAsync<BrowserNotLaunchedException>(launcher.StartAsync);

        Assert.Contains("exited with code 1", exception.Message);
        Assert.Equal(3, fakeBrowser.Launches.Count);
        Assert.Equal(3, fakeBrowser.Launches.Select(arguments => arguments.Single(argument => argument.StartsWith("--port=", StringComparison.Ordinal))).Distinct().Count());
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

    // Makes the fake browser the Chrome browser and chromedriver found through the environment.
    private sealed class DriverOverride : IDisposable
    {
        public DriverOverride()
        {
            Environment.SetEnvironmentVariable("CHROME_EXECUTABLE", FakeBrowserSetup.ExecutablePath);
            Environment.SetEnvironmentVariable("CHROMEDRIVER_EXECUTABLE", FakeBrowserSetup.ExecutablePath);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CHROME_EXECUTABLE", null);
            Environment.SetEnvironmentVariable("CHROMEDRIVER_EXECUTABLE", null);
        }
    }

    // Points Path.GetTempPath at another directory.
    private sealed class TemporaryDirectoryOverride : IDisposable
    {
        private static readonly string[] VariableNames = OperatingSystem.IsWindows() ? ["TMP", "TEMP"] : ["TMPDIR"];
        private readonly Dictionary<string, string?> originalValues = [];

        public TemporaryDirectoryOverride(string directory)
        {
            foreach (string name in VariableNames)
            {
                this.originalValues[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, directory);
            }
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, string?> pair in this.originalValues)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }
}
