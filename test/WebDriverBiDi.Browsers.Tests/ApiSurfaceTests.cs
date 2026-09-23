// <copyright file="ApiSurfaceTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using WebDriverBiDi.Browsers.TestUtilities;
using static WebDriverBiDi.Browsers.TestUtilities.ChromeForTestingService;

public class ApiSurfaceTests
{
    private const string LatestVersion = "130.0.6723.58";
    private static readonly TimeSpan ProcessExitTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void EveryPublicExceptionDerivesFromWebDriverBiDiException()
    {
        IEnumerable<Type> exceptionTypes = typeof(BrowserLauncher).Assembly.GetExportedTypes().Where(type => typeof(Exception).IsAssignableFrom(type));

        Assert.NotEmpty(exceptionTypes);
        Assert.All(exceptionTypes, type => Assert.True(typeof(WebDriverBiDiException).IsAssignableFrom(type), type.Name));
    }

    [Fact]
    public async Task LaunchFailureReportsExitCodeAndProcessOutput()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "exit:3");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserLaunchException exception = await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Equal(3, exception.ExitCode);
        Assert.Contains("Fake browser exiting with code 3", exception.ProcessOutput);
        Assert.Contains("Fake browser exiting with code 3", exception.Message);
    }

    [Fact]
    public async Task FindBrowserRejectsUnsupportedBrowser()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Edge, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<NotSupportedException>(() => DriverLocator.FindDriverAsync(BrowserKind.Edge, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void BuildRejectsUnsupportedBrowser()
    {
        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Edge).Build);
    }

    [Fact]
    public async Task LaunchIsCancelledWhileWaitingForBrowserAndLeavesNothingRunning()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent", startChild: true);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => launcher.LaunchBrowserAsync(cancellationSource.Token));

        Assert.False(launcher.IsRunning);
        Assert.False(Directory.Exists(fakeBrowser.GetLastLaunchArgument("--user-data-dir")));
        using Process child = fakeBrowser.GetChildProcess();
        Assert.True(child.WaitForExit(ProcessExitTimeout));
    }

    [Fact]
    public async Task QuitCancelledWhileWaitingKillsBrowserThenThrows()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Windows cannot ask a windowless process to exit, so there is no wait to cancel.");
        using FakeBrowserSetup fakeBrowser = new(mode: "ignore-term");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        launcher.ShutdownTimeout = TimeSpan.FromMinutes(1);
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(500));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => launcher.QuitBrowserAsync(cancellationSource.Token));

        Assert.False(launcher.IsRunning);
    }

    [Fact]
    public async Task FindBrowserWithCancelledTokenMakesNoRequests()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        using CancellationTokenSource cancellationSource = new();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: cancellationSource.Token));

        Assert.Empty(server.RequestedUrls);
    }

    [Fact]
    public async Task WaitForLockIsCancellable()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        using TemporaryDirectory cache = new();
        string scopeDirectory = Path.Combine(cache.Path, "chrome", "stable");
        Directory.CreateDirectory(scopeDirectory);
        using FileStream heldLock = new(Path.Combine(scopeDirectory, ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: TestDownloadOptions.Create(server, cache), cancellationToken: cancellationSource.Token));
    }

    [Fact]
    public async Task LaunchWhileBrowserRunningIsRejected()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Single(fakeBrowser.Launches);
    }

    [Fact]
    public async Task InstanceFromEarlierLaunchDoesNotAffectLaterBrowser()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome)).AtLocation(FakeBrowserSetup.ExecutablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        BrowserInstance first = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
        await first.CloseAsync(TestContext.Current.CancellationToken);
        BrowserInstance second = await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await first.CloseAsync(TestContext.Current.CancellationToken);
        await first.KillAsync(TestContext.Current.CancellationToken);
        await first.DisposeAsync();

        Assert.False(first.IsRunning);
        Assert.True(second.IsRunning);
        Assert.True(launcher.IsRunning);
    }

    [Fact]
    public async Task DownloadReportsProgressThroughToCompletion()
    {
        await using DownloadServer server = await DownloadServer.StartAsync();
        Serve(server, "Stable", LatestVersion);
        using TemporaryDirectory cache = new();
        RecordingProgress progress = new();
        BrowserDownloadOptions defaults = TestDownloadOptions.Create(server, cache);
        BrowserDownloadOptions options = new()
        {
            CacheDirectory = defaults.CacheDirectory,
            Platform = defaults.Platform,
            ChromeForTestingEndpoint = defaults.ChromeForTestingEndpoint,
            Progress = progress,
        };

        await BrowserLocator.FindBrowserAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);
        await DriverLocator.FindDriverAsync(BrowserKind.Chrome, downloadOptions: options, cancellationToken: TestContext.Current.CancellationToken);

        BrowserDownloadProgress[] browserReports = [.. progress.Reports.Where(report => report.Name == $"Chrome Stable {LatestVersion}")];
        Assert.Equal(0, browserReports[0].BytesReceived);
        Assert.NotNull(browserReports[^1].TotalBytes);
        Assert.Equal(browserReports[^1].TotalBytes, browserReports[^1].BytesReceived);
        Assert.Contains(progress.Reports, report => report.Name == $"chromedriver {LatestVersion}");
    }

    private sealed class RecordingProgress : IProgress<BrowserDownloadProgress>
    {
        private readonly List<BrowserDownloadProgress> reports = [];

        public IReadOnlyList<BrowserDownloadProgress> Reports
        {
            get
            {
                lock (this.reports)
                {
                    return [.. this.reports];
                }
            }
        }

        public void Report(BrowserDownloadProgress value)
        {
            lock (this.reports)
            {
                this.reports.Add(value);
            }
        }
    }
}
