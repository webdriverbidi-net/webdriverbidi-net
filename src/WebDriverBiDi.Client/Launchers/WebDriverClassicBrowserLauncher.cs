// <copyright file="WebDriverClassicBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Net;
using System.Text;
using System.Text.Json;
using WebDriverBiDi;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriver BiDi session.
/// This class establishes a WebDriver Classic session that is upgraded to use WebDriver BiDi,
/// and is suitable for using with any remote end compatible with WebDriver Classic and WebDriver
/// BiDi, including Selenium Grid.
/// </summary>
public abstract class WebDriverClassicBrowserLauncher : BrowserLauncher
{
    private readonly HttpClient httpClient = new();

    private readonly ObservableEvent<LogMessageEventArgs> onLogMessageEvent = new("classicBrowserLauncher.logMessage");

    private string launcherHostName = "localhost";
    private bool useSsl = false;
    private string sessionId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverClassicBrowserLauncher"/> class.
    /// </summary>
    /// <param name="hostName">The host name of the WebDriver Classic remote end.</param>
    /// <param name="port">The port of the WebDriver Classic remote end.</param>
    public WebDriverClassicBrowserLauncher(string hostName, int port)
        : this(hostName, port, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverClassicBrowserLauncher"/> class.
    /// </summary>
    /// <param name="hostName">The host name of the WebDriver Classic remote end.</param>
    /// <param name="port">The port of the WebDriver Classic remote end.</param>
    /// <param name="useSsl"><see langword="true"/> to use HTTPS to connect to the remote end; otherwise <see langword="false"/>.</param>
    public WebDriverClassicBrowserLauncher(string hostName, int port, bool useSsl)
        : this(string.Empty, port, string.Empty)
    {
        this.launcherHostName = hostName;
        this.useSsl = useSsl;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverClassicBrowserLauncher"/> class.
    /// </summary>
    /// <param name="launcherExecutablePath">The path to the directory containing the launcher executable.</param>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    /// <param name="browserExecutableLocation">The path containing the directory and file name of the browser executable. Default to an empty string, indicating to use the default installed browser.</param>
    protected WebDriverClassicBrowserLauncher(string launcherExecutablePath, int port, string browserExecutableLocation = "")
        : base(launcherExecutablePath, port, browserExecutableLocation)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public override bool IsBiDiSessionInitialized => true;

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
    /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
    /// it gracefully before forcing a termination.
    /// </summary>
    protected virtual bool HasShutdownApi => false;

    /// <summary>
    /// Gets the HTTP client used to communicate with the WebDriver Classic remote end. Note
    /// carefully that this is only used for launching and quitting the browser.
    /// </summary>
    protected HttpClient HttpClient => this.httpClient;

    /// <summary>
    /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
    /// </summary>
    protected virtual TimeSpan TerminationTimeout => TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the Uri of the service.
    /// </summary>
    protected string ServiceUrl => $"{(this.useSsl ? "https" : "http")}://{this.launcherHostName}:{this.Port}";

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override async Task StartAsync()
    {
        bool launcherAvailable = await this.WaitForInitializationAsync().ConfigureAwait(false);
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
    public override Task StopAsync()
    {
        return Task.CompletedTask;
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
    /// Asynchronously waits for the initialization of the browser launcher.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task<bool> WaitForInitializationAsync()
    {
        bool isInitialized = false;
        DateTime timeout = DateTime.Now.Add(this.InitializationTimeout);
        while (!isInitialized && DateTime.Now < timeout)
        {
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

    /// <summary>
    /// Asynchronously raises the OnLogMessageAsync event.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> of the message.</param>
    /// <param name="component">The component providing the message.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level, string component = "Browser Launcher")
    {
        await this.OnLogMessageAsync(new LogMessageEventArgs(message, level, component));
    }

    private async Task OnLogMessageAsync(LogMessageEventArgs eventArgs)
    {
        await this.onLogMessageEvent.NotifyObserversAsync(eventArgs);
    }
}
