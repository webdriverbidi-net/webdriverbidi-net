// <copyright file="ProcessEnvironmentLauncherTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

// These tests set environment variables of the test process itself, read by every launcher, so
// they must not run alongside other tests.
[Collection("NonParallel")]
public class ProcessEnvironmentLauncherTests
{
    private static readonly TimeSpan ProcessExitTimeout = TimeSpan.FromSeconds(10);

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
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser))
            .AtLocation(Path.Combine(executableDirectory, Path.GetFileName(FakeBrowserSetup.ExecutablePath)))
            .WithConnection(connectionKind)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await fakeBrowser.WaitForLaunchAsync();
        string profileDirectory = fakeBrowser.GetLastLaunchArgument(profileArgumentName);
        Assert.StartsWith(temporaryDirectory, profileDirectory);
        Assert.True(Directory.Exists(profileDirectory));
    }

    [Fact]
    public async Task DriverStopKillsDriverAndItsChildProcesses()
    {
        using FakeBrowserSetup fakeBrowser = new(startChild: true);
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).LaunchUsingDriver().Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        using Process child = fakeBrowser.GetChildProcess();
        Assert.True(launcher.IsRunning);

        await launcher.StopAsync(TestContext.Current.CancellationToken);

        Assert.False(launcher.IsRunning);
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Fact]
    public async Task DriverThatExitsBeforeReadyIsRetriedOnAnotherPort()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "exit:1");
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).LaunchUsingDriver().Build();

        BrowserLaunchException exception = await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.StartAsync(TestContext.Current.CancellationToken));

        Assert.Contains("exited with code 1", exception.Message);
        Assert.Equal(1, exception.ExitCode);
        Assert.Equal(3, fakeBrowser.Launches.Count);
        Assert.Equal(3, fakeBrowser.Launches.Select(launch => launch.Arguments.Single(argument => argument.StartsWith("--port=", StringComparison.Ordinal))).Distinct().Count());
    }

    [Fact]
    public async Task ChromeDriverLaunchPassesLaunchOptionsInCapabilities()
    {
        using FakeBrowserSetup fakeBrowser = new();
        using DriverOverride driverOverride = new();
        using TemporaryDirectory profile = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .LaunchUsingDriver()
            .WithArguments("--custom-argument")
            .WithUserDataDirectory(profile.Path)
            .WithEnvironmentVariable(FakeBrowserSetup.EchoVariableName, "for the driver")
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode chromeOptions = fakeBrowser.SessionRequests.Single()["capabilities"]!["firstMatch"]![0]!["goog:chromeOptions"]!;
        string[] arguments = chromeOptions["args"]!.Deserialize<string[]>()!;
        Assert.Contains("--custom-argument", arguments);
        Assert.Contains($"--user-data-dir={profile.Path}", arguments);
        Assert.Equal("for the driver", fakeBrowser.Launches.Single().Echo);
    }

    [Fact]
    public async Task GeckoDriverLaunchPassesLaunchOptionsInCapabilities()
    {
        using FakeBrowserSetup fakeBrowser = new();
        using DriverOverride driverOverride = new(BrowserKind.Firefox);
        using TemporaryDirectory profile = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox))
            .LaunchUsingDriver()
            .WithArguments("--custom-argument")
            .WithUserDataDirectory(profile.Path)
            .WithBrowserOptions(new FirefoxLaunchOptions() { Preferences = { ["custom.boolean"] = true, ["custom.number"] = 3 } })
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode firefoxOptions = fakeBrowser.SessionRequests.Single()["capabilities"]!["firstMatch"]![0]!["moz:firefoxOptions"]!;
        Assert.Equal(["-profile", profile.Path, "--custom-argument"], firefoxOptions["args"]!.Deserialize<string[]>()!);
        Assert.True((bool)firefoxOptions["prefs"]!["custom.boolean"]!);
        Assert.Equal(3, (int)firefoxOptions["prefs"]!["custom.number"]!);
    }

    [Fact]
    public async Task EnvironmentVariableSetToNullIsNotInherited()
    {
        Environment.SetEnvironmentVariable(FakeBrowserSetup.EchoVariableName, "inherited");
        try
        {
            using FakeBrowserSetup inheritingBrowser = new();
            await using (BrowserLauncher launcher = inheritingBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build())
            {
                await launcher.StartAsync(TestContext.Current.CancellationToken);
                await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
            }

            using FakeBrowserSetup isolatedBrowser = new();
            await using (BrowserLauncher launcher = isolatedBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
                .AtLocation(FakeBrowserSetup.ExecutablePath)
                .WithEnvironmentVariable(FakeBrowserSetup.EchoVariableName, null)
                .Build())
            {
                await launcher.StartAsync(TestContext.Current.CancellationToken);
                await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
            }

            Assert.Equal("inherited", inheritingBrowser.Launches.Single().Echo);
            Assert.Null(isolatedBrowser.Launches.Single().Echo);
        }
        finally
        {
            Environment.SetEnvironmentVariable(FakeBrowserSetup.EchoVariableName, null);
        }
    }

    [Fact]
    public async Task DriverStartCancelledWhileWaitingStopsDriver()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent");
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).LaunchUsingDriver().Build();
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => launcher.StartAsync(cancellationSource.Token));

        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public async Task HeadlessShellIsLaunchedThroughDriverWithoutHeadlessArgument()
    {
        using FakeBrowserSetup fakeBrowser = new();
        using DriverOverride driverOverride = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .LaunchUsingDriver()
            .WithHeadlessOption()
            .WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true })
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        JsonNode chromeOptions = fakeBrowser.SessionRequests.Single()["capabilities"]!["firstMatch"]![0]!["goog:chromeOptions"]!;
        string[] arguments = chromeOptions["args"]!.Deserialize<string[]>()!;
        Assert.DoesNotContain(arguments, argument => argument.StartsWith("--headless", StringComparison.Ordinal));
    }

    // Makes the fake browser the browser and driver found through the environment.
    private sealed class DriverOverride : IDisposable
    {
        private readonly string[] variableNames;

        public DriverOverride(BrowserKind browser = BrowserKind.Chrome)
        {
            this.variableNames = browser == BrowserKind.Firefox
                ? ["FIREFOX_EXECUTABLE", "GECKODRIVER_EXECUTABLE"]
                : ["CHROME_EXECUTABLE", "CHROMEDRIVER_EXECUTABLE"];
            foreach (string name in this.variableNames)
            {
                Environment.SetEnvironmentVariable(name, FakeBrowserSetup.ExecutablePath);
            }
        }

        public void Dispose()
        {
            foreach (string name in this.variableNames)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
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
