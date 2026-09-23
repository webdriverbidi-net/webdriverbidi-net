// <copyright file="ChromeLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using System.Runtime.InteropServices;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Object for launching a Chrome browser to connect to using a WebDriverBiDi session.
/// This browser launcher does not rely on any external executable except for the browser itself.
/// </summary>
public class ChromeLauncher : BrowserLauncher, IPipeServerProcessProvider
{
    private readonly List<string> disabledFeatures = [
        "Translate",

        // AcceptCHFrame disabled because of crbug.com/1348106.
        "AcceptCHFrame",
        "MediaRouter",
        "OptimizationHints",
        "WebUIReloadButton",
        "WebUIOmniboxPopup",
        "WebUIOmniboxAimPopup",
        "IPH_ReadingModePageActionLabel",
        "ReadAnythingOmniboxChip",
        "ProcessPerSiteUpToMainFrameThreshold",
        "IsolateSandboxedIframes",
    ];

    private readonly List<string> enabledFeatures = [
        "PdfOopif",
    ];

    private readonly List<string> chromeArguments = [
        "--allow-pre-commit-input",
        "--disable-background-networking",
        "--disable-background-timer-throttling",
        "--disable-backgrounding-occluded-windows",
        "--disable-breakpad",
        "--disable-client-side-phishing-detection",
        "--disable-component-extensions-with-background-pages",
        "--disable-default-apps",
        "--disable-dev-shm-usage",
        "--disable-field-trial-config",
        "--disable-hang-monitor",
        "--disable-infobars",
        "--disable-ipc-flooding-protection",
        "--disable-popup-blocking",
        "--disable-prompt-on-repost",
        "--disable-renderer-backgrounding",
        "--disable-search-engine-choice-screen",
        "--disable-sync",
        "--enable-automation",
        "--enable-blink-features=IdleDetection",
        "--export-tagged-pdf",
        "--generate-pdf-document-outline",
        "--force-color-profile=srgb",
        "--metrics-recording-only",
        "--no-first-run",
        "--password-store=basic",
        "--use-mock-keychain",
    ];

    private TemporaryProfile? profile;

    private Connection? connection;

    private Process? browserProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The <see cref="ChromeBrowserLocatorSettings"/> settings to use for locating the Chrome browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    internal ChromeLauncher(ChromeBrowserLocatorSettings browserLocatorSettings, int port = 0)
        : base(browserLocatorSettings, port)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the service is running.
    /// </summary>
    public override bool IsRunning => this.browserProcess is not null && !this.browserProcess.HasExited;

    /// <summary>
    /// Gets or sets a value indicating the type of connection to use in communicating with the browser.
    /// </summary>
    public ConnectionKind ConnectionType { get; set; } = ConnectionKind.WebSocket;

    /// <summary>
    /// Gets the process that hosts the pipe server. This process is
    /// expected to be started and running when accessed. The pipe
    /// server process should be responsible for managing the lifetime
    /// of the pipe handles used for communication.
    /// </summary>
    public Process? PipeServerProcess => this.browserProcess;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("chromeLauncher.logMessage");

    private IList<string> CommandLineArguments
    {
        get
        {
            List<string> args = [.. this.chromeArguments];
            args.Add($"--disable-features={string.Join(",", this.disabledFeatures)}");
            args.Add($"--enable-features={string.Join(",", this.enabledFeatures)}");
            args.Add($"--user-data-dir={this.profile?.Path}");
            if (this.ConnectionType == ConnectionKind.Pipes)
            {
                args.Add("--remote-debugging-pipe");
            }
            else
            {
                args.Add($"--remote-debugging-port={this.Port}");
            }

            if (this.IsBrowserHeadless)
            {
                args.Add("--headless=new");
                args.Add("--no-sandbox");
                args.Add("--disable-gpu");
            }

            args.Add("about:blank");
            return args.AsReadOnly();
        }
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Task StartAsync()
    {
        this.ThrowIfDisposed();
        this.connection = this.CreateConnection();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously launches the browser and returns a <see cref="BrowserInstance"/> representing the running browser.
    /// </summary>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override async Task<BrowserInstance> LaunchBrowserAsync()
    {
        this.ThrowIfDisposed();
        string browserExecutableLocation = await this.BrowserLocator.LocateBrowserAsync().ConfigureAwait(false);
        await this.LogAsync($"Launching Chrome browser from {browserExecutableLocation}").ConfigureAwait(false);

        // With port 0, Chrome chooses a free port itself and reports it with its DevTools endpoint.
        this.ConnectionString = string.Empty;
        this.profile = TemporaryProfile.Create("chrome");
        try
        {
            Process process = new()
            {
                StartInfo = this.CreateProcessStartInfo(browserExecutableLocation),
            };
            process.ErrorDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.OutputDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.Start();
            this.browserProcess = process;
            this.profile.SetOwner(this.browserProcess);
            this.browserProcess.BeginOutputReadLine();
            this.browserProcess.BeginErrorReadLine();
            bool launcherAvailable = await this.WaitForInitializationAsync().ConfigureAwait(false);
            if (!launcherAvailable)
            {
                // The wait ends as soon as the browser process does, so a failure here is not
                // necessarily a timeout. A browser that fails at startup typically exits within
                // a fraction of the timeout, and reporting that as "did not start within N
                // seconds" hides the real fault; its exit code is the first clue as to the cause.
                string reason = this.IsRunning
                    ? $"Browser process did not report its DevTools endpoint within {this.InitializationTimeout.TotalSeconds} seconds."
                    : $"Browser process exited with code {this.browserProcess.ExitCode} before reporting its DevTools endpoint.";
                throw new BrowserNotLaunchedException($"Unable to launch Chrome browser. {reason}");
            }
        }
        catch (Exception)
        {
            // Whatever was started is killed, and the profile deleted, however the launch failed.
            await this.TerminateBrowserProcessAsync(requestExit: false).ConfigureAwait(false);
            throw;
        }

        if (this.ConnectionType == ConnectionKind.WebSocket)
        {
            this.Port = new Uri(this.ConnectionString).Port;
        }

        int processId = this.GetProcessId();
        return new BrowserInstance(this, this.ConnectionString, processId);
    }

    /// <summary>
    /// Asynchronously quits the browser: asks it to exit, waits up to <see cref="BrowserLauncher.ShutdownTimeout"/>,
    /// then kills it and every process it started, and deletes its temporary profile.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task QuitBrowserAsync()
    {
        if (this.connection is not null && this.connection.IsActive && this.connection.ConnectionKind == ConnectionKind.Pipes && this.connection is PipeConnection pipeConnection)
        {
            await pipeConnection.StopAsync().ConfigureAwait(false);
            this.connection = null;
        }

        await this.TerminateBrowserProcessAsync(requestExit: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously forces the browser to terminate, for use when <see cref="QuitBrowserAsync"/> has failed.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task KillBrowserAsync()
    {
        // Terminate first: stopping the pipe connection is the step most likely to be what failed.
        await this.TerminateBrowserProcessAsync(requestExit: false).ConfigureAwait(false);
        if (this.connection is PipeConnection pipeConnection)
        {
            try
            {
                await pipeConnection.StopAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // The process is gone; a failure releasing the pipe must not mask that the kill succeeded.
            }
        }

        this.connection = null;
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StopAsync()
    {
        // No operation required to stop the launcher.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a Transport object that can be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Transport"/> to be used in instantiating the driver.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Transport CreateTransport()
    {
        this.ThrowIfDisposed();
        return new ChromiumTransport(this.connection ?? this.CreateConnection())
        {
            InitializationTimeout = this.InitializationTimeout,
        };
    }

    /// <summary>
    /// Creates the <see cref="Connection"/> object to be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Connection"/> object to be used to communicate with the browser.</returns>
    protected override Connection CreateConnection()
    {
        if (this.ConnectionType == ConnectionKind.Pipes)
        {
            return new PipeConnection(this);
        }

        return new WebSocketConnection()
        {
            StartupTimeout = this.InitializationTimeout,
        };
    }

    /// <summary>
    /// Gets the process ID of the browser process, or 0 if the browser is not running.
    /// </summary>
    /// <returns>The process ID, or 0 if not running.</returns>
    protected override int GetProcessId()
    {
        return this.browserProcess?.Id ?? 0;
    }

    private static string GetShellPath()
    {
        // Try common shell locations
        string[] shellPaths = ["/bin/bash", "/usr/bin/bash", "/bin/sh", "/usr/bin/sh"];

        foreach (string path in shellPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Fall back to bash and hope it's in PATH
        return "bash";
    }

    private ProcessStartInfo CreateProcessStartInfo(string browserExecutableLocation)
    {
        string fileName = browserExecutableLocation;
        List<string> arguments = [.. this.CommandLineArguments];
        if (this.connection is PipeConnection pipeConnection && pipeConnection.ConnectionKind == ConnectionKind.Pipes)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                arguments.Add($"--remote-debugging-io-pipes={pipeConnection.ReadPipeHandle},{pipeConnection.WritePipeHandle}");
            }
            else
            {
                // Chrome reads commands from file descriptor 3 and writes responses to 4, so a shell
                // duplicates the inherited pipe descriptors onto those, closes the originals, and
                // then replaces itself with the browser.
                string readHandle = pipeConnection.ReadPipeHandle;
                string writeHandle = pipeConnection.WritePipeHandle;
                string browserCommand = string.Join(" ", new[] { browserExecutableLocation }.Concat(arguments).Select(CommandLine.QuotePosixShellArgument));
                fileName = GetShellPath();
                arguments = ["-c", $"exec 3<&{readHandle} 4>&{writeHandle} {readHandle}<&- {writeHandle}>&-; exec {browserCommand}"];
            }
        }

        return new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = CommandLine.JoinArguments(arguments),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
    }

    private async Task<bool> WaitForInitializationAsync()
    {
        bool isInitialized = false;
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (!isInitialized && initializationStopwatch.Elapsed <= this.InitializationTimeout)
        {
            // If the driver service process has exited, we can exit early.
            if (!this.IsRunning)
            {
                break;
            }

            if (this.browserProcess is not null && this.connection is not null && this.connection.ConnectionKind == ConnectionKind.Pipes && this.connection is PipeConnection)
            {
                this.ConnectionString = $"pipe://chrome:{this.browserProcess.Id}";
            }

            if (!string.IsNullOrEmpty(this.ConnectionString))
            {
                isInitialized = true;
                break;
            }
            else
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        initializationStopwatch.Stop();
        return isInitialized;
    }

    private async Task TerminateBrowserProcessAsync(bool requestExit)
    {
        Process? process = this.browserProcess;
        if (process is not null)
        {
            bool hasExited = process.HasExited
                || (requestExit && ProcessTermination.RequestExit(process) && await ProcessTermination.WaitForExitAsync(process, this.ShutdownTimeout).ConfigureAwait(false));
            if (!hasExited)
            {
                ProcessTermination.KillTree(process);
                await ProcessTermination.WaitForExitAsync(process, ProcessTermination.KilledProcessExitTimeout).ConfigureAwait(false);
            }

            process.Dispose();
            this.browserProcess = null;
        }

        if (this.profile is not null)
        {
            await this.profile.DeleteAsync().ConfigureAwait(false);
            this.profile = null;
        }
    }
}
