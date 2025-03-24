// <copyright file="ClassicDriverBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WebDriverBiDi;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriverBiDi session.
/// </summary>
public abstract class ClassicDriverBrowserLauncher : BrowserLauncher
{
    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly string launcherExecutableName;
    private readonly HttpClient httpClient = new();

    private readonly ObservableEvent<BrowserLauncherProcessStartingEventArgs> onLauncherProcessStartingEvent = new();
    private readonly ObservableEvent<BrowserLauncherProcessStartedEventArgs> onLauncherProcessStartedEvent = new();
    private readonly ObservableEvent<LogMessageEventArgs> onLogMessageEvent = new();

    private string launcherHostName = "localhost";
    private bool hideCommandPromptWindow;
    private bool captureBrowserLauncherOutput = true;
    private bool isLoggingLauncherProcessOutput;
    private Process? launcherProcess;
    private string sessionId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassicDriverBrowserLauncher"/> class.
    /// </summary>
    /// <param name="launcherExecutablePath">The path to the directory containing the launcher executable.</param>
    /// <param name="launcherExecutableName">The name of the launcher executable.</param>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    /// <param name="browserExecutableLocation">The path containing the directory and file name of the browser executable. Default to an empty string, indicating to use the default installed browser.</param>
    protected ClassicDriverBrowserLauncher(string launcherExecutablePath, string launcherExecutableName, int port, string browserExecutableLocation = "")
        : base(launcherExecutablePath, port, browserExecutableLocation)
    {
        this.launcherExecutableName = launcherExecutableName;
    }

    /// <summary>
    /// Gets an observable event that notifies when the launcher process is starting.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartingEventArgs> OnLauncherProcessStarting => this.onLauncherProcessStartingEvent;

    /// <summary>
    /// Gets an observable event that notifies when the launcher process has completely started.
    /// </summary>
    public ObservableEvent<BrowserLauncherProcessStartedEventArgs> OnLauncherProcessStarted => this.onLauncherProcessStartedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    public override ObservableEvent<LogMessageEventArgs> OnLogMessage => this.onLogMessageEvent;

    /// <summary>
    /// Gets or sets the host name of the launcher. Defaults to "localhost".
    /// </summary>
    /// <remarks>
    /// Most browser launcher executables do not allow connections from remote
    /// (non-local) machines. This property can be used as a workaround so
    /// that an IP address (like "127.0.0.1" or "::1") can be used instead.
    /// </remarks>
    public string HostName { get => this.launcherHostName; set => this.launcherHostName = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the command prompt window of the browser launcher should be hidden.
    /// </summary>
    public bool HideCommandPromptWindow { get => this.hideCommandPromptWindow; set => this.hideCommandPromptWindow = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to capture the standard out for the browser launcher executable.
    /// </summary>
    public bool CaptureBrowserLauncherOutput { get => this.captureBrowserLauncherOutput;  set => this.captureBrowserLauncherOutput = value; }

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
    protected virtual bool HasShutdownApi => true;

    /// <summary>
    /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
    /// </summary>
    protected virtual TimeSpan TerminationTimeout => TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the Uri of the service.
    /// </summary>
    private string ServiceUrl => $"http://{this.launcherHostName}:{this.Port}";

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
        bool launcherAvailable = false;
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
            this.launcherProcess.StartInfo.CreateNoWindow = this.hideCommandPromptWindow;
            this.launcherProcess.StartInfo.RedirectStandardInput = this.captureBrowserLauncherOutput;
            this.launcherProcess.StartInfo.RedirectStandardOutput = this.captureBrowserLauncherOutput;
            this.launcherProcess.StartInfo.RedirectStandardError = this.captureBrowserLauncherOutput;

            BrowserLauncherProcessStartingEventArgs eventArgs = new(this.launcherProcess.StartInfo);
            await this.OnLauncherProcessStartingAsync(eventArgs);
            await this.LogAsync("Starting browser launcher", WebDriverBiDiLogLevel.Info);

            this.launcherProcess.Start();
            this.StartLoggingProcessOutput();
            launcherAvailable = await this.WaitForInitializationAsync().ConfigureAwait(false);
            BrowserLauncherProcessStartedEventArgs processStartedEventArgs = new(this.launcherProcess);
            await this.OnLauncherProcessStartedAsync(processStartedEventArgs);
            await this.LogAsync("Browser launcher started", WebDriverBiDiLogLevel.Info);
        }
        finally
        {
            LockObject.Release();
        }

        if (!launcherAvailable)
        {
            string msg = "Cannot start the browser launcher on " + this.ServiceUrl;
            throw new WebDriverBiDiException(msg);
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
                        using HttpResponseMessage response = await this.httpClient.GetAsync($"{this.ServiceUrl}/shutdown").ConfigureAwait(false);
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
    /// Asynchronously launches the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    public override async Task LaunchBrowserAsync()
    {
        Dictionary<string, object> classicCapabilities = new()
        {
            ["capabilities"] = new Dictionary<string, object>()
            {
                ["firstMatch"] = new List<object>()
                {
                    this.CreateBrowserLaunchCapabilities(),
                },
            },
        };
        string json = JsonSerializer.Serialize(classicCapabilities);
        await this.LogAsync("Launching browser", WebDriverBiDiLogLevel.Info);
        await this.LogAsync($"Sending classic new session command. JSON:\n{json}", WebDriverBiDiLogLevel.Debug);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync($"{this.ServiceUrl}/session", content).ConfigureAwait(false);
        string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new BrowserNotLaunchedException($"Unable to launch browser. Received status code {response.StatusCode} with body {responseJson} from launcher");
        }

        await this.LogAsync($"Received classic new session response. JSON:\n{responseJson}", WebDriverBiDiLogLevel.Debug);
        using (JsonDocument returned = JsonDocument.Parse(responseJson))
        {
            JsonElement rootElement = returned.RootElement;
            if (rootElement.TryGetProperty("value", out JsonElement returnedValue))
            {
                if (returnedValue.TryGetProperty("sessionId", out JsonElement returnedSessionId))
                {
                    this.sessionId = returnedSessionId.GetString()!;
                }

                if (returnedValue.TryGetProperty("capabilities", out JsonElement capabilities))
                {
                    if (capabilities.TryGetProperty("webSocketUrl", out JsonElement returnedWebSocketUrl))
                    {
                        this.WebSocketUrl = returnedWebSocketUrl.GetString()!;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(this.sessionId))
        {
            throw new BrowserNotLaunchedException($"Unable to launch browser. Could not detect session ID in WebDriver classic new session response (response JSON: {responseJson})");
        }

        if (string.IsNullOrEmpty(this.WebSocketUrl))
        {
            throw new BrowserNotLaunchedException($"Unable to connect to WebSocket. Launched browse may not support the WebDriver BiDi protocol (response JSON: {responseJson})");
        }
    }

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public override async Task QuitBrowserAsync()
    {
        if (!string.IsNullOrEmpty(this.sessionId))
        {
            await this.LogAsync($"Quitting browser", WebDriverBiDiLogLevel.Info);
            using HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.ServiceUrl}/session/{this.sessionId}").ConfigureAwait(false);
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new CannotQuitBrowserException($"Unable to quit browser. Received status code {response.StatusCode} with body {responseJson} from launcher");
            }
        }
    }

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected abstract Dictionary<string, object> CreateBrowserLaunchCapabilities();

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

    /// <summary>
    /// Asynchronously waits for the initialization of the browser launcher.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    private async Task<bool> WaitForInitializationAsync()
    {
        bool isInitialized = false;
        DateTime timeout = DateTime.Now.Add(this.InitializationTimeout);
        while (!isInitialized && DateTime.Now < timeout)
        {
            // If the driver service process has exited, we can exit early.
            if (!this.IsRunning)
            {
                break;
            }

            try
            {
                using HttpResponseMessage response = await this.httpClient.GetAsync($"{this.ServiceUrl}/status").ConfigureAwait(false);

                // Checking the response from the 'status' end point. Note that we are simply checking
                // that the HTTP status returned is a 200 status, and that the response has the correct
                // Content-Type header. A more sophisticated check would parse the JSON response and
                // validate its values. At the moment we do not do this more sophisticated check.
                isInitialized = response.StatusCode == HttpStatusCode.OK &&
                    response.Content.Headers.ContentType is not null &&
                    response.Content.Headers.ContentType.MediaType is not null &&
                    response.Content.Headers.ContentType.MediaType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase);
            }
            catch (HttpRequestException)
            {
            }
        }

        return isInitialized;
    }

    private async Task OnLauncherProcessStartingAsync(BrowserLauncherProcessStartingEventArgs eventArgs)
    {
        await this.onLauncherProcessStartingEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnLauncherProcessStartedAsync(BrowserLauncherProcessStartedEventArgs eventArgs)
    {
        await this.onLauncherProcessStartedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnLogMessageAsync(LogMessageEventArgs eventArgs)
    {
        await this.onLogMessageEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task LogAsync(string message, WebDriverBiDiLogLevel level, string component = "Browser Launcher")
    {
        await this.OnLogMessageAsync(new LogMessageEventArgs(message, level, component));
    }

    private void StartLoggingProcessOutput()
    {
        if (this.captureBrowserLauncherOutput)
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
