// <copyright file="ChromeLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Object for launching a Chrome browser to connect to using a WebDriverBiDi session.
/// This browser launcher does not rely on any external executable except for the browser itself.
/// </summary>
public class ChromeLauncher : BrowserLauncher, IPipeServerProcessProvider
{
    private const string NoStartupWindowArgument = "--no-startup-window";
    private const string ComponentExtensionsArgument = "--disable-component-extensions-with-background-pages";

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
        ComponentExtensionsArgument,
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

    private readonly ProcessOutputTail outputTail = new();

    private TemporaryProfile? profile;

    private Connection? connection;

    private Process? browserProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The settings to use for locating the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    internal ChromeLauncher(BrowserLocatorSettings browserLocatorSettings, int port = 0)
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
    /// Gets a value indicating whether the process runs as root on Linux, where Chrome's sandbox is
    /// unavailable and Chrome refuses to start without the --no-sandbox argument.
    /// </summary>
    [ExcludeFromCodeCoverage] // Depends on the operating system and user the tests run as.
    internal static IEnumerable<string> SandboxArguments => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Environment.UserName == "root" ? ["--no-sandbox"] : [];

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("chromeLauncher.logMessage");

    /// <summary>
    /// Gets the name of the browser in messages, and, in lowercase, in the name of its temporary profile.
    /// </summary>
    private protected virtual string ProductName => "Chrome";

    private IList<string> CommandLineArguments
    {
        get
        {
            List<string> defaultArguments = [.. this.chromeArguments];

            // Headless Chrome starts without a window, so without a tab no one uses. Headed Chrome keeps its window:
            // without one, the windows it opens for new pages are not given focus. Starting without a window, Chrome
            // refuses the hidden tab the BiDi mapper runs in unless component extensions may run.
            if (this.IsBrowserHeadless && !this.LaunchSettings.UseHeadlessShell)
            {
                defaultArguments.Remove(ComponentExtensionsArgument);
                defaultArguments.Add(NoStartupWindowArgument);
            }

            defaultArguments.Add($"--disable-features={string.Join(",", this.disabledFeatures)}");
            defaultArguments.Add($"--enable-features={string.Join(",", this.enabledFeatures)}");
            defaultArguments.AddRange(SandboxArguments);

            List<string> args = [.. this.LaunchSettings.FilterDefaultArguments(defaultArguments)];
            args.Add($"--user-data-dir={this.LaunchSettings.UserDataDirectory ?? this.profile!.Path}");
            if (this.ConnectionType == ConnectionKind.Pipes)
            {
                args.Add("--remote-debugging-pipe");
            }
            else
            {
                args.Add($"--remote-debugging-port={this.Port}");
            }

            // chrome-headless-shell is always headless.
            if (this.IsBrowserHeadless && !this.LaunchSettings.UseHeadlessShell)
            {
                args.Add("--headless=new");
                args.Add("--disable-gpu");
            }

            args.AddRange(this.LaunchSettings.Arguments);

            // A window to start in gets a blank tab.
            if (!args.Contains(NoStartupWindowArgument))
            {
                args.AddRange(this.LaunchSettings.FilterDefaultArguments(["about:blank"]));
            }

            return args.AsReadOnly();
        }
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        this.connection = this.CreateConnection();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously launches the browser and returns a <see cref="BrowserInstance"/> representing the running browser.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the launch; anything already started is stopped.</param>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserLaunchException">Thrown when the browser cannot be launched.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override async Task<BrowserInstance> LaunchBrowserAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (this.browserProcess is not null)
        {
            throw new InvalidOperationException("A browser launched by this launcher is still running; quit it before launching another.");
        }

        string browserExecutableLocation = await this.BrowserLocator.LocateBrowserAsync(cancellationToken).ConfigureAwait(false);

        // Quitting releases the pipes, so a relaunch needs new ones.
        this.connection ??= this.CreateConnection();
        await this.LogAsync($"Launching {this.ProductName} browser from {browserExecutableLocation}").ConfigureAwait(false);

        // With port 0, Chrome chooses a free port itself and reports it with its DevTools endpoint.
        this.ConnectionString = string.Empty;
        this.profile = this.LaunchSettings.UserDataDirectory is null ? TemporaryProfile.Create(this.ProductName.ToLowerInvariant()) : null;
        try
        {
            Process process = new()
            {
                StartInfo = this.CreateProcessStartInfo(browserExecutableLocation),
            };
            process.ErrorDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.OutputDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.ErrorDataReceived += this.RecordProcessOutput;
            process.OutputDataReceived += this.RecordProcessOutput;
            this.outputTail.Clear();
            StartProcess(process, this.ProductName);
            this.browserProcess = process;
            this.profile?.SetOwner(this.browserProcess);
            this.browserProcess.BeginOutputReadLine();
            this.browserProcess.BeginErrorReadLine();
            if (this.connection is PipeConnection)
            {
                // A pipe connection is ready as soon as the process runs.
                this.ConnectionString = $"pipe://chrome:{process.Id}";
            }

            bool launcherAvailable = await this.WaitForInitializationAsync(cancellationToken).ConfigureAwait(false);
            if (!launcherAvailable)
            {
                // The wait ends as soon as the browser process does, so a failure here is not
                // necessarily a timeout. A browser that fails at startup typically exits within
                // a fraction of the timeout, and reporting that as "did not start within N
                // seconds" hides the real fault; its exit code is the first clue as to the cause.
                int? exitCode = null;
                string reason = $"Browser process did not report its DevTools endpoint within {this.InitializationTimeout.TotalSeconds} seconds.";
                if (!this.IsRunning)
                {
                    await ProcessTermination.WaitForOutputAsync(this.browserProcess).ConfigureAwait(false);
                    exitCode = this.browserProcess.ExitCode;
                    reason = $"Browser process exited with code {exitCode} before reporting its DevTools endpoint.";
                }

                throw new BrowserLaunchException($"Unable to launch {this.ProductName} browser. {reason}", exitCode, this.outputTail.ToList());
            }
        }
        catch (Exception)
        {
            // Whatever was started is killed, and the profile deleted, however the launch failed.
            await this.TerminateBrowserProcessAsync(requestExit: false, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        if (this.ConnectionType == ConnectionKind.WebSocket)
        {
            this.Port = new Uri(this.ConnectionString).Port;
        }

        return this.CreateBrowserInstance(this.ConnectionString, this.GetProcessId());
    }

    /// <summary>
    /// Asynchronously quits the browser: asks it to exit, waits up to <see cref="BrowserLauncher.ShutdownTimeout"/>,
    /// then kills it and every process it started, and deletes its temporary profile.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the browser to exit; the browser is then killed, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task QuitBrowserAsync(CancellationToken cancellationToken = default)
    {
        // Stopping a pipe connection that was never opened does nothing.
        if (this.connection is PipeConnection pipeConnection)
        {
            await pipeConnection.StopAsync().ConfigureAwait(false);
            this.connection = null;
        }

        await this.TerminateBrowserProcessAsync(requestExit: true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously forces the browser to terminate, for use when <see cref="QuitBrowserAsync"/> has failed.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the killed browser to exit, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task KillBrowserAsync(CancellationToken cancellationToken = default)
    {
        // Terminate first: stopping the pipe connection is the step most likely to be what failed.
        await this.TerminateBrowserProcessAsync(requestExit: false, cancellationToken).ConfigureAwait(false);
        if (this.connection is PipeConnection pipeConnection)
        {
            await pipeConnection.StopAsync().ConfigureAwait(false);
        }

        this.connection = null;
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the launcher to stop.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StopAsync(CancellationToken cancellationToken = default)
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
    /// Gets the process ID of the browser process just launched.
    /// </summary>
    /// <returns>The process ID.</returns>
    protected override int GetProcessId()
    {
        return this.browserProcess!.Id;
    }

    [ExcludeFromCodeCoverage] // Takes only the branch for the operating system it runs on.
    private static (string FileName, List<string> Arguments) GetPipeLaunchCommand(string browserExecutableLocation, List<string> arguments, string readHandle, string writeHandle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return (browserExecutableLocation, [.. arguments, $"--remote-debugging-io-pipes={readHandle},{writeHandle}"]);
        }

        // Chrome reads commands from file descriptor 3 and writes responses to 4, so a shell
        // duplicates the inherited pipe descriptors onto those, closes the originals, and
        // then replaces itself with the browser.
        string browserCommand = string.Join(" ", new[] { browserExecutableLocation }.Concat(arguments).Select(CommandLine.QuotePosixShellArgument));
        return (GetShellPath(), ["-c", $"exec 3<&{readHandle} 4>&{writeHandle} {readHandle}<&- {writeHandle}>&-; exec {browserCommand}"]);
    }

    [ExcludeFromCodeCoverage] // Depends on where the machine has a shell.
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
        if (this.connection is PipeConnection pipeConnection)
        {
            (fileName, arguments) = GetPipeLaunchCommand(browserExecutableLocation, arguments, pipeConnection.ReadPipeHandle, pipeConnection.WritePipeHandle);
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = CommandLine.JoinArguments(arguments),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        this.LaunchSettings.ApplyEnvironmentVariables(startInfo);
        return startInfo;
    }

    private async Task<bool> WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        // A browser process that has exited ends the wait early.
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (initializationStopwatch.Elapsed <= this.InitializationTimeout && this.IsRunning)
        {
            if (!string.IsNullOrEmpty(this.ConnectionString))
            {
                return true;
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private void RecordProcessOutput(object sender, DataReceivedEventArgs e)
    {
        this.outputTail.Add(e.Data);
    }

    private async Task TerminateBrowserProcessAsync(bool requestExit, CancellationToken cancellationToken)
    {
        bool isCancelled = false;
        Process? process = this.browserProcess;
        if (process is not null)
        {
            isCancelled = await ProcessTermination.StopAsync(process, requestExit, this.ShutdownTimeout, cancellationToken).ConfigureAwait(false);
            process.Dispose();
            this.browserProcess = null;
        }

        if (this.profile is not null)
        {
            await this.profile.DeleteAsync().ConfigureAwait(false);
            this.profile = null;
        }

        if (isCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }
}
