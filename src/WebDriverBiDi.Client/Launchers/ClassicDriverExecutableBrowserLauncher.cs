// <copyright file="ClassicDriverExecutableBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly string launcherExecutableName;
    private bool isLoggingLauncherProcessOutput;
    private Process? launcherProcess;
    private string sessionId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassicDriverExecutableBrowserLauncher"/> class.
    /// </summary>
    /// <param name="launcherExecutablePath">The path to the directory containing the launcher executable.</param>
    /// <param name="launcherExecutableName">The name of the launcher executable.</param>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    /// <param name="browserExecutableLocation">The path containing the directory and file name of the browser executable. Default to an empty string, indicating to use the default installed browser.</param>
    protected ClassicDriverExecutableBrowserLauncher(string launcherExecutablePath, string launcherExecutableName, int port, string browserExecutableLocation = "")
        : base(launcherExecutablePath, port, browserExecutableLocation)
    {
        this.launcherExecutableName = launcherExecutableName;
    }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public override bool IsBiDiSessionInitialized => true;

    /// <summary>
    /// Gets an observable event that notifies when the launcher process is starting.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartingEventArgs> OnLauncherProcessStarting { get; } = new(LauncherProcessStartingEventName);

    /// <summary>
    /// Gets an observable event that notifies when the launcher process has completely started.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartedEventArgs> OnLauncherProcessStarted { get; } = new(LauncherProcessStartedEventName);

    /// <summary>
    /// Gets or sets a value indicating whether the command prompt window of the browser launcher should be hidden.
    /// </summary>
    public bool HideCommandPromptWindow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to capture the standard out for the browser launcher executable.
    /// </summary>
    public bool CaptureBrowserLauncherOutput { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the service is running.
    /// </summary>
    public bool IsRunning => this.launcherProcess is not null && !this.launcherProcess.HasExited;

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
                    return this.launcherProcess!.Id;
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
    /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
    /// it gracefully before forcing a termination.
    /// </summary>
    protected override bool HasShutdownApi => true;

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override async Task StartAsync()
    {
        if (this.launcherProcess is not null)
        {
            return;
        }

        string browserLauncherFullPath = Path.Combine(this.LauncherPath, this.launcherExecutableName);
        if (!File.Exists(browserLauncherFullPath))
        {
            throw new BrowserLauncherNotFoundException($"Could not find browser launcher executable '{browserLauncherFullPath}'");
        }

        // A word about the locking mechanism. It's not entirely possible to make
        // atomic the finding of a free port, then using that port as the port for
        // the launcher to listen on. There will always be a race condition between
        // releasing the port and starting the launcher where another application
        // could acquire the same port. The window of opportunity is likely in the
        // millisecond order of magnitude, but the chance does exist. We will attempt
        // to mitigate at least other instances of a BrowserLauncher acquiring the
        // same port when launching the browser.
        await LockObject.WaitAsync().ConfigureAwait(false);
        try
        {
            if (this.Port == 0)
            {
                this.Port = FindFreePort();
            }

            this.launcherProcess = new Process();
            this.launcherProcess.StartInfo.FileName = browserLauncherFullPath;
            this.launcherProcess.StartInfo.Arguments = this.CommandLineArguments;
            this.launcherProcess.StartInfo.UseShellExecute = false;
            this.launcherProcess.StartInfo.CreateNoWindow = this.HideCommandPromptWindow;
            this.launcherProcess.StartInfo.RedirectStandardInput = this.CaptureBrowserLauncherOutput;
            this.launcherProcess.StartInfo.RedirectStandardOutput = this.CaptureBrowserLauncherOutput;
            this.launcherProcess.StartInfo.RedirectStandardError = this.CaptureBrowserLauncherOutput;

            BrowserLauncherProcessStartingEventArgs eventArgs = new(this.launcherProcess.StartInfo);
            await this.OnLauncherProcessStartingAsync(eventArgs);
            await this.LogAsync("Starting browser launcher", WebDriverBiDiLogLevel.Info);

            this.launcherProcess.Start();
            this.StartLoggingProcessOutput();

            // The base class's StartAsync method polls the WebDriver remote end's "status" end point
            // until a timeout to make sure the HTTP server of the remote end is running.
            await base.StartAsync().ConfigureAwait(false);
            BrowserLauncherProcessStartedEventArgs processStartedEventArgs = new(this.launcherProcess);
            await this.OnLauncherProcessStartedAsync(processStartedEventArgs);
            await this.LogAsync("Browser launcher started", WebDriverBiDiLogLevel.Info);
        }
        finally
        {
            LockObject.Release();
        }
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override async Task StopAsync()
    {
        if (this.IsRunning)
        {
            await this.LogAsync("Shutting down browser launcher", WebDriverBiDiLogLevel.Info);
            if (this.HasShutdownApi)
            {
                DateTime timeout = DateTime.Now.Add(this.TerminationTimeout);
                while (this.IsRunning && DateTime.Now < timeout)
                {
                    try
                    {
                        // Issue the shutdown HTTP request, then wait a short while for
                        // the process to have exited. If the process hasn't yet exited,
                        // we'll retry. We wait for exit here, since catching the exception
                        // for a failed HTTP request due to a closed socket is particularly
                        // expensive.
                        using HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.ServiceUrl}/shutdown").ConfigureAwait(false);
                        this.launcherProcess!.WaitForExit(3000);
                    }
                    catch (WebException)
                    {
                    }
                }
            }

            // If at this point, the process still hasn't exited, wait for one
            // last-ditch time, then, if it still hasn't exited, kill it. Note
            // that falling into this branch of code should be exceedingly rare.
            if (this.IsRunning)
            {
                this.launcherProcess!.WaitForExit(Convert.ToInt32(this.TerminationTimeout.TotalMilliseconds));
                if (!this.launcherProcess.HasExited)
                {
                    this.launcherProcess.Kill();
                }
            }

            this.StopLoggingProcessOutput();
            this.launcherProcess!.Dispose();
            this.launcherProcess = null;
            await this.LogAsync("Browser launcher exited", WebDriverBiDiLogLevel.Info);
        }
    }

    /// <summary>
    /// Finds a random, free port to be listened on.
    /// </summary>
    /// <returns>A random, free port to be listened on.</returns>
    private static int FindFreePort()
    {
        // Locate a free port on the local machine by binding a socket to
        // an IPEndPoint using IPAddress.Any and port 0. The socket will
        // select a free port.
        int listeningPort = 0;
        Socket portSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPEndPoint socketEndPoint = new(IPAddress.Any, 0);
            portSocket.Bind(socketEndPoint);
            if (portSocket.LocalEndPoint is not null)
            {
                socketEndPoint = (IPEndPoint)portSocket.LocalEndPoint;
                listeningPort = socketEndPoint.Port;
            }
        }
        finally
        {
            portSocket.Close();
        }

        return listeningPort;
    }

    private async Task OnLauncherProcessStartingAsync(BrowserLauncherProcessStartingEventArgs eventArgs)
    {
        await this.OnLauncherProcessStarting.NotifyObserversAsync(eventArgs);
    }

    private async Task OnLauncherProcessStartedAsync(BrowserLauncherProcessStartedEventArgs eventArgs)
    {
        await this.OnLauncherProcessStarted.NotifyObserversAsync(eventArgs);
    }

    private void StartLoggingProcessOutput()
    {
        if (this.CaptureBrowserLauncherOutput)
        {
            this.isLoggingLauncherProcessOutput = true;
            _ = Task.Run(() => this.ReadStandardOutput());
            _ = Task.Run(() => this.ReadStandardError());
        }
    }

    private void StopLoggingProcessOutput()
    {
        this.isLoggingLauncherProcessOutput = false;
    }

    private async void ReadStandardOutput()
    {
        while (this.launcherProcess is not null && this.isLoggingLauncherProcessOutput)
        {
            await this.LogAsync(this.launcherProcess.StandardOutput.ReadLine(), WebDriverBiDiLogLevel.Debug, this.launcherExecutableName);
        }
    }

    private async void ReadStandardError()
    {
        while (this.launcherProcess is not null && this.isLoggingLauncherProcessOutput)
        {
            await this.LogAsync(this.launcherProcess!.StandardError.ReadLine(), WebDriverBiDiLogLevel.Debug, this.launcherExecutableName);
        }
    }
}
