// <copyright file="ClassicDriverExecutableBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using WebDriverBiDi;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriver BiDi session.
/// This class launches a WebDriver Classic browser driver executable (geckodriver,
/// chromedriver, safaridriver, etc.), and then establishes a WebDriver Classic session
/// that is upgraded to use WebDriver BiDi.
/// </summary>
public abstract class ClassicDriverExecutableBrowserLauncher : WebDriverClassicBrowserLauncher
{
    private const string LauncherProcessStartingEventName = "classicDriverBrowserLauncher.launcherProcessStarting";
    private const string LauncherProcessStartedEventName = "classicDriverBrowserLauncher.launcherProcessStarted";

    // A port is free when chosen, but another process can take it before the driver binds it, in
    // which case the driver exits at once and is started again on another port.
    private const int AutomaticPortAttempts = 3;

    private readonly ObservableEventInvocable<BrowserLauncherProcessStartingEventArgs> invocableBrowserLauncherProcessStartingObservableEvent = new(LauncherProcessStartingEventName);
    private readonly ObservableEventInvocable<BrowserLauncherProcessStartedEventArgs> invocableBrowserLauncherProcessStartedObservableEvent = new(LauncherProcessStartedEventName);

    private readonly string launcherExecutableName;

    private readonly ProcessOutputTail outputTail = new();

    private Process? launcherProcess;
    private Task[] outputReaderTasks = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassicDriverExecutableBrowserLauncher"/> class using browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="browserLocatorSettings">The browser locator settings to use for locating the browser and driver executables.</param>
    /// <param name="port">The port on which the launcher will listen.</param>
    internal ClassicDriverExecutableBrowserLauncher(BrowserLocatorSettings browserLocatorSettings, int port = 0)
        : base(browserLocatorSettings, port)
    {
        this.launcherExecutableName = browserLocatorSettings.DriverExecutableName;
    }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public override bool IsBiDiSessionInitialized => true;

    /// <summary>
    /// Gets an observable event that notifies when the launcher process is starting.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartingEventArgs> OnLauncherProcessStarting => this.invocableBrowserLauncherProcessStartingObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the launcher process has completely started.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartedEventArgs> OnLauncherProcessStarted => this.invocableBrowserLauncherProcessStartedObservableEvent;

    /// <summary>
    /// Gets or sets a value indicating whether the command prompt window of the browser launcher should be hidden.
    /// </summary>
    public bool HideCommandPromptWindow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to capture the standard out for the browser launcher executable.
    /// </summary>
    public bool CaptureBrowserLauncherOutput { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the driver service is running.
    /// </summary>
    [MemberNotNullWhen(true, nameof(launcherProcess))]
    public override bool IsRunning => this.launcherProcess is not null && !this.launcherProcess.HasExited;

    /// <summary>
    /// Gets the process ID of the running driver service executable. Returns 0 if the process is not running.
    /// </summary>
    public int ProcessId
    {
        get
        {
            if (this.IsRunning)
            {
                // There's a slight chance that the Process object is running,
                // but does not have an ID set. This should be rare, but we
                // definitely don't want to throw an exception.
                try
                {
                    // IsRunning contains a null check for the process.
                    return this.launcherProcess.Id;
                }
                catch (InvalidOperationException)
                {
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the command-line arguments for the driver service.
    /// </summary>
    protected virtual string CommandLineArguments => $"--port={this.Port}";

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (this.launcherProcess is not null)
        {
            return;
        }

        // Locate executables using BrowserLocator
        BrowserExecutableInfo executableInfo = await this.BrowserLocator.LocateExecutablesAsync(cancellationToken).ConfigureAwait(false);

        // The settings of every driver launcher include the driver, so a path is always located.
        string browserLauncherFullPath = executableInfo.DriverPath!;
        this.BrowserExecutableLocation = executableInfo.BrowserPath;
        string? driverDirectory = Path.GetDirectoryName(browserLauncherFullPath);

        // If the driver path has no directory (e.g., just "chromedriver"), it's on the system PATH
        string logDetail;
        if (string.IsNullOrEmpty(driverDirectory))
        {
            // Driver is on system PATH - executableInfo.DriverPath is just the executable name
            logDetail = $"'{browserLauncherFullPath}' on system PATH";
        }
        else
        {
            if (!File.Exists(browserLauncherFullPath))
            {
                throw new BrowserLauncherNotFoundException($"Could not find browser launcher executable '{browserLauncherFullPath}'");
            }

            logDetail = $"from {browserLauncherFullPath}";
        }

        await this.LogAsync($"Launching WebDriver classic driver executable {logDetail} (browser launched from: {this.BrowserExecutableLocation})").ConfigureAwait(false);

        bool isPortAutomatic = this.Port == 0;
        for (int attempt = 1; ; attempt++)
        {
            if (isPortAutomatic)
            {
                this.Port = FindFreePort();
            }

            await this.StartLauncherProcessAsync(browserLauncherFullPath).ConfigureAwait(false);
            bool isReady;
            try
            {
                isReady = await this.WaitForInitializationAsync(() => !this.IsRunning, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await this.StopLauncherProcessAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }

            if (isReady)
            {
                break;
            }

            int? exitCode = this.IsRunning ? null : this.launcherProcess!.ExitCode;
            string reason = exitCode is null
                ? $"did not respond on {this.ServiceUrl} within {this.InitializationTimeout.TotalSeconds} seconds"
                : $"exited with code {exitCode} before responding on {this.ServiceUrl}";
            await this.StopLauncherProcessAsync(CancellationToken.None).ConfigureAwait(false);
            if (!isPortAutomatic || exitCode is null || attempt == AutomaticPortAttempts)
            {
                throw new BrowserLaunchException($"Unable to start {this.launcherExecutableName}: it {reason}.", exitCode, this.outputTail.ToList());
            }

            await this.LogAsync($"{this.launcherExecutableName} {reason}; retrying on another port.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
        }

        BrowserLauncherProcessStartedEventArgs processStartedEventArgs = new(this.launcherProcess!);
        await this.OnLauncherProcessStartedAsync(processStartedEventArgs).ConfigureAwait(false);
        await this.LogAsync("Browser launcher started", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the launcher to stop.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (this.launcherProcess is not null)
        {
            await this.LogAsync("Shutting down browser launcher", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            await this.StopLauncherProcessAsync(cancellationToken).ConfigureAwait(false);
            await this.LogAsync("Browser launcher exited", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously forces the browser to terminate, for use when <see cref="BrowserLauncher.QuitBrowserAsync"/> has failed.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the killed browser to exit, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override Task KillBrowserAsync(CancellationToken cancellationToken = default)
    {
        // The browser is a descendant of the driver, so stopping the driver, which kills its whole
        // process tree, also kills the browser.
        return this.StopAsync(cancellationToken);
    }

    private async Task OnLauncherProcessStartingAsync(BrowserLauncherProcessStartingEventArgs eventArgs)
    {
        await this.invocableBrowserLauncherProcessStartingObservableEvent.InvokeNotifyObserversAsync(eventArgs).ConfigureAwait(false);
    }

    private async Task OnLauncherProcessStartedAsync(BrowserLauncherProcessStartedEventArgs eventArgs)
    {
        await this.invocableBrowserLauncherProcessStartedObservableEvent.InvokeNotifyObserversAsync(eventArgs).ConfigureAwait(false);
    }

    private async Task StartLauncherProcessAsync(string executablePath)
    {
        Process process = new();
        process.StartInfo.FileName = executablePath;
        process.StartInfo.Arguments = this.CommandLineArguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = this.HideCommandPromptWindow;
        process.StartInfo.RedirectStandardInput = this.CaptureBrowserLauncherOutput;
        process.StartInfo.RedirectStandardOutput = this.CaptureBrowserLauncherOutput;
        process.StartInfo.RedirectStandardError = this.CaptureBrowserLauncherOutput;
        this.LaunchSettings.ApplyEnvironmentVariables(process.StartInfo);

        BrowserLauncherProcessStartingEventArgs eventArgs = new(process.StartInfo);
        await this.OnLauncherProcessStartingAsync(eventArgs).ConfigureAwait(false);
        await this.LogAsync("Starting browser launcher", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        this.outputTail.Clear();
        process.Start();
        this.launcherProcess = process;
        if (this.CaptureBrowserLauncherOutput)
        {
            this.outputReaderTasks = [this.LogProcessOutputAsync(process.StandardOutput), this.LogProcessOutputAsync(process.StandardError)];
        }
    }

    private async Task StopLauncherProcessAsync(CancellationToken cancellationToken)
    {
        Process? process = this.launcherProcess;
        if (process is null)
        {
            return;
        }

        // Killing the whole tree also ends any browser the driver started and did not close.
        bool isCancelled = await ProcessTermination.StopAsync(process, requestExit: false, TimeSpan.Zero, cancellationToken).ConfigureAwait(false);

        // The readers finish once the killed processes' ends of the output pipes are closed.
        await Task.WhenAny(Task.WhenAll(this.outputReaderTasks), Task.Delay(ProcessTermination.KilledProcessExitTimeout)).ConfigureAwait(false);
        process.Dispose();
        this.launcherProcess = null;
        this.outputReaderTasks = [];
        if (isCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private async Task LogProcessOutputAsync(StreamReader reader)
    {
        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
            {
                this.outputTail.Add(line);
                await this.LogAsync(line, WebDriverBiDiLogLevel.Debug, this.launcherExecutableName).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
        {
            // The process was disposed while its output was being read.
        }
    }
}
