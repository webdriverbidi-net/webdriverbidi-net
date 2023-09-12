// <copyright file="BrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriverBiDi session.
/// </summary>
public abstract class BrowserLauncher
{
    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly string launcherPath;
    private readonly string launcherExecutableName;
    private readonly string browserExecutableLocation;
    private readonly HttpClient httpClient = new();
    private string launcherHostName = "localhost";
    private int launcherPort;
    private bool hideCommandPromptWindow;
    private TimeSpan initializationTimeout = TimeSpan.FromSeconds(20);
    private Process? launcherProcess;
    private string sessionId = string.Empty;
    private string webSocketUrl = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="launcherExecutablePath">The path to the directory containing the launcher executable.</param>
    /// <param name="launcherExecutableName">The name of the launcher executable.</param>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    /// <param name="browserExecutableLocation">The path containing the directory and file name of the browser executable. Default to an empty string, indicating to use the default installed browser.</param>
    protected BrowserLauncher(string launcherExecutablePath, string launcherExecutableName, int port, string browserExecutableLocation = "")
    {
        this.launcherPath = launcherExecutablePath;
        this.launcherExecutableName = launcherExecutableName;
        this.launcherPort = port;
        this.browserExecutableLocation = browserExecutableLocation;
    }

    /// <summary>
    /// Occurs when the launcher process is starting.
    /// </summary>
    public event EventHandler<BrowserLauncherProcessStartingEventArgs>? LauncherProcessStarting;

    /// <summary>
    /// Occurs when the launcher process has completely started.
    /// </summary>
    public event EventHandler<BrowserLauncherProcessStartedEventArgs>? LauncherProcessStarted;

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
    /// Gets or sets the port on which the launcher should listen.
    /// </summary>
    public int Port { get => this.launcherPort; set => this.launcherPort = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the command prompt window of the service should be hidden.
    /// </summary>
    public bool HideCommandPromptWindow { get => this.hideCommandPromptWindow; set => this.hideCommandPromptWindow = value; }

    /// <summary>
    /// Gets or sets a value indicating the time to wait for an initial connection before timing out.
    /// </summary>
    public TimeSpan InitializationTimeout { get => this.initializationTimeout; set => this.initializationTimeout = value; }

    /// <summary>
    /// Gets a value indicating whether the service is running.
    /// </summary>
    public bool IsRunning => this.launcherProcess is not null && !this.launcherProcess.HasExited;

    /// <summary>
    /// Gets the WebSocket URL for communicating with the browser via the WebDriver BiDi protocol.
    /// </summary>
    public string WebSocketUrl => this.webSocketUrl;

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
    protected virtual string CommandLineArguments => $"--port={this.launcherPort}";

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
    /// Gets the location of the browser executable.
    /// </summary>
    protected string BrowserExecutableLocation => this.browserExecutableLocation;

    /// <summary>
    /// Gets the Uri of the service.
    /// </summary>
    private string ServiceUrl => $"http://{this.launcherHostName}:{this.launcherPort}";

    /// <summary>
    /// Creates a launcher for the specified browser.
    /// </summary>
    /// <param name="browserType">The type of browser launcher to create.</param>
    /// <param name="launcherPath">The path to the browser launcher, not including the executable name.</param>
    /// <param name="browserExecutableLocation">The path and executable name of the browser executable.</param>
    /// <returns>The launcher for the specified browser type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when an invalid browser type is specified.</exception>
    public static BrowserLauncher Create(BrowserType browserType, string launcherPath, string browserExecutableLocation = "")
    {
        if (browserType == BrowserType.Firefox)
        {
            return new FirefoxLauncher(launcherPath, browserExecutableLocation);
        }

        if (browserType == BrowserType.Chrome)
        {
            return new ChromeLauncher(launcherPath, browserExecutableLocation);
        }

        throw new WebDriverBiDiException("Invalid browser type");
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        if (this.launcherProcess is not null)
        {
            return;
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
            if (this.launcherPort == 0)
            {
                this.launcherPort = FindFreePort();
            }

            string browserLauncherFullPath = Path.Combine(this.launcherPath, this.launcherExecutableName);
            if (!File.Exists(browserLauncherFullPath))
            {
                throw new BrowserLauncherNotFoundException($"Could not find browser launcher executable '{browserLauncherFullPath}'");
            }

            this.launcherProcess = new Process();
            this.launcherProcess.StartInfo.FileName = browserLauncherFullPath;
            this.launcherProcess.StartInfo.Arguments = this.CommandLineArguments;
            this.launcherProcess.StartInfo.UseShellExecute = false;
            this.launcherProcess.StartInfo.CreateNoWindow = this.hideCommandPromptWindow;

            BrowserLauncherProcessStartingEventArgs eventArgs = new(this.launcherProcess.StartInfo);
            this.OnLauncherProcessStarting(eventArgs);

            this.launcherProcess.Start();
            launcherAvailable = await this.WaitForInitializationAsync().ConfigureAwait(false);
            BrowserLauncherProcessStartedEventArgs processStartedEventArgs = new(this.launcherProcess);
            this.OnLauncherProcessStarted(processStartedEventArgs);
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
    public async Task StopAsync()
    {
        if (this.IsRunning)
        {
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

            this.launcherProcess!.Dispose();
            this.launcherProcess = null;
        }
    }

    /// <summary>
    /// Asynchronously launches the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    public async Task LaunchBrowserAsync()
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
        Console.WriteLine($"Sending classic new session command. JSON:\n{json}");
        StringContent content = new(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync($"{this.ServiceUrl}/session", content).ConfigureAwait(false);
        string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new BrowserNotLaunchedException($"Unable to launch browser. Received status code {response.StatusCode} with body {responseJson} from launcher");
        }

        Console.WriteLine($"Received classic new session response. JSON:\n{responseJson}");
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
                        this.webSocketUrl = returnedWebSocketUrl.GetString()!;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(this.sessionId))
        {
            throw new BrowserNotLaunchedException($"Unable to launch browser. Could not detect session ID in WebDriver classic new session response (response JSON: {responseJson})");
        }

        if (string.IsNullOrEmpty(this.webSocketUrl))
        {
            throw new BrowserNotLaunchedException($"Unable to connect to WebSocket. Launched browse may not support the WebDriver BiDi protocol (response JSON: {responseJson})");
        }
    }

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public async Task QuitBrowserAsync()
    {
        if (!string.IsNullOrEmpty(this.sessionId))
        {
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
    /// Raises the <see cref="LauncherProcessStarting"/> event.
    /// </summary>
    /// <param name="eventArgs">A <see cref="BrowserLauncherProcessStartingEventArgs"/> that contains the event data.</param>
    protected void OnLauncherProcessStarting(BrowserLauncherProcessStartingEventArgs eventArgs)
    {
        if (this.LauncherProcessStarting is not null)
        {
            this.LauncherProcessStarting(this, eventArgs);
        }
    }

    /// <summary>
    /// Raises the <see cref="LauncherProcessStarted"/> event.
    /// </summary>
    /// <param name="eventArgs">A <see cref="BrowserLauncherProcessStartedEventArgs"/> that contains the event data.</param>
    protected void OnLauncherProcessStarted(BrowserLauncherProcessStartedEventArgs eventArgs)
    {
        if (this.LauncherProcessStarted is not null)
        {
            this.LauncherProcessStarted(this, eventArgs);
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
}
