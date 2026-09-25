// <copyright file="WebDriverClassicBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebDriverBiDi;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriver BiDi session.
/// This class establishes a WebDriver Classic session that is upgraded to use WebDriver BiDi,
/// and is suitable for using with any remote end compatible with WebDriver Classic and WebDriver
/// BiDi, including Selenium Grid.
/// </summary>
public class WebDriverClassicBrowserLauncher : BrowserLauncher
{
    private readonly HttpClient httpClient = new();

    private Uri? remoteEndUrl;
    private string sessionId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverClassicBrowserLauncher"/> class using browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="browserLocatorSettings">The browser locator settings to use for locating the browser and driver executables.</param>
    /// <param name="port">The port on which the launcher will listen.</param>
    internal WebDriverClassicBrowserLauncher(BrowserLocatorSettings browserLocatorSettings, int port = 0)
        : base(browserLocatorSettings, port)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public override bool IsBiDiSessionInitialized => true;

    /// <summary>
    /// Gets a value indicating whether the browser is currently running.
    /// For remote browsers, this is true if a session has been established.
    /// </summary>
    public override bool IsRunning => !string.IsNullOrEmpty(this.sessionId);

    /// <summary>
    /// Gets a value indicating whether the browser can be closed using WebDriver BiDi's browser.close command.
    /// </summary>
    public override bool IsBrowserCloseAllowed => this.BrowserLocator.BrowserName != "firefox";

    /// <summary>
    /// Gets or sets the capabilities the user added to the new session request. The builder rejects any
    /// named in <see cref="LaunchCapabilityNames"/>.
    /// </summary>
    internal Dictionary<string, object?> AdditionalCapabilities { get; set; } = [];

    /// <summary>
    /// Gets the capabilities requested unless <see cref="AdditionalCapabilities"/> replaces them.
    /// </summary>
    internal Dictionary<string, object?> DefaultCapabilities { get; } = [];

    /// <summary>
    /// Gets the names of the capabilities <see cref="CreateBrowserLaunchCapabilities"/> can set.
    /// </summary>
    internal virtual IReadOnlyCollection<string> LaunchCapabilityNames { get; } = ["browserName", "webSocketUrl"];

    /// <summary>
    /// Sets the URL of a remote end not started by this launcher, such as a grid (e.g.,
    /// "https://grid.example/wd/hub"), or <see langword="null"/> for a driver on this machine's
    /// <see cref="BrowserLauncher.Port"/>. Credentials in the URL are sent as Basic authorization.
    /// </summary>
    internal Uri RemoteEndUrl
    {
        set
        {
            this.remoteEndUrl = new UriBuilder(value) { UserName = string.Empty, Password = string.Empty }.Uri;
            if (!string.IsNullOrEmpty(value.UserInfo))
            {
                byte[] credentials = Encoding.UTF8.GetBytes(Uri.UnescapeDataString(value.UserInfo));
                this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
            }
        }
    }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("classicBrowserLauncher.logMessage");

    /// <summary>
    /// Gets or sets the location of the browser executable.
    /// </summary>
    protected string BrowserExecutableLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Uri of the service.
    /// </summary>
    // A driver on this machine is given its port before the URL is first used.
    protected string ServiceUrl => this.remoteEndUrl?.AbsoluteUri.TrimEnd('/') ?? $"http://localhost:{this.Port}";

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        bool launcherAvailable = await this.WaitForInitializationAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!launcherAvailable)
        {
            throw new BrowserLaunchException($"The remote end at {this.ServiceUrl} did not report that it was ready within {this.InitializationTimeout.TotalSeconds} seconds.");
        }
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the launcher to stop.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
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
        if (!string.IsNullOrEmpty(this.sessionId))
        {
            throw new InvalidOperationException("A browser launched by this launcher is still running; quit it before launching another.");
        }

        await this.BrowserLocator.LocateBrowserAsync(cancellationToken).ConfigureAwait(false);

        Dictionary<string, object?> sessionCapabilities = new(this.DefaultCapabilities);
        foreach (KeyValuePair<string, object?> capability in this.AdditionalCapabilities.Concat(this.CreateBrowserLaunchCapabilities()))
        {
            sessionCapabilities[capability.Key] = capability.Value;
        }

        string json = CapabilityWriter.WriteNewSessionRequest(sessionCapabilities);
        await this.LogAsync("Launching browser", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
        await this.LogAsync($"Sending classic new session command. JSON:\n{json}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync($"{this.ServiceUrl}/session", content, cancellationToken).ConfigureAwait(false);
        string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new BrowserLaunchException($"Unable to launch browser. Received status code {response.StatusCode} with body {responseJson} from launcher");
        }

        await this.LogAsync($"Received classic new session response. JSON:\n{responseJson}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
        using (JsonDocument returned = JsonDocument.Parse(responseJson))
        {
            JsonElement rootElement = returned.RootElement;
            if (rootElement.TryGetProperty("value", out JsonElement returnedValue))
            {
                if (returnedValue.TryGetProperty("sessionId", out JsonElement returnedSessionId))
                {
                    this.sessionId = returnedSessionId.GetString() ?? string.Empty;
                }

                if (returnedValue.TryGetProperty("capabilities", out JsonElement capabilities))
                {
                    if (capabilities.TryGetProperty("webSocketUrl", out JsonElement returnedWebSocketUrl))
                    {
                        this.ConnectionString = returnedWebSocketUrl.GetString() ?? string.Empty;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(this.sessionId))
        {
            throw new BrowserLaunchException($"Unable to launch browser. Could not detect session ID in WebDriver classic new session response (response JSON: {responseJson})");
        }

        if (string.IsNullOrEmpty(this.ConnectionString))
        {
            // The session is useless without a WebSocket URL, so it is ended rather than left open on the remote end.
            try
            {
                await this.QuitBrowserAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is CannotQuitBrowserException || ex is HttpRequestException)
            {
                this.sessionId = string.Empty;
            }

            throw new BrowserLaunchException($"Unable to connect to WebSocket. Launched browser may not support the WebDriver BiDi protocol (response JSON: {responseJson})");
        }

        return this.CreateBrowserInstance(this.ConnectionString, this.GetProcessId());
    }

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the browser to exit; the browser is then killed, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public override async Task QuitBrowserAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(this.sessionId))
        {
            await this.LogAsync($"Quitting browser", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
            using HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.ServiceUrl}/session/{this.sessionId}", cancellationToken).ConfigureAwait(false);
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new CannotQuitBrowserException($"Unable to quit browser. Received status code {response.StatusCode} with body {responseJson} from launcher");
            }

            // IsRunning is derived from the session ID, and a later quit must not delete a session that is gone.
            this.sessionId = string.Empty;
        }
    }

    /// <summary>
    /// Adds a header sent with every request to the remote end.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns><see langword="true"/> if the header was added; <see langword="false"/> if it cannot be sent as a request header.</returns>
    internal bool TryAddRequestHeader(string name, string value)
    {
        try
        {
            return this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }
        catch (FormatException)
        {
            // .NET Framework rejects a malformed name by throwing, rather than by returning false.
            return false;
        }
    }

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities, whose values are those <see cref="CapabilityWriter"/> can write.</returns>
    protected virtual Dictionary<string, object?> CreateBrowserLaunchCapabilities()
    {
        Dictionary<string, object?> capabilities = new()
        {
            ["browserName"] = this.BrowserLocator.BrowserName.ToLowerInvariant(),
            ["webSocketUrl"] = true,
        };

        return capabilities;
    }

    /// <summary>
    /// Releases the resources used by the launcher after quitting the browser and stopping the launcher.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        await base.DisposeAsyncCore().ConfigureAwait(false);
        this.httpClient.Dispose();
    }

    /// <summary>
    /// Asynchronously waits for the initialization of the browser launcher.
    /// </summary>
    /// <param name="hasFailed">Checked before each poll; returns <see langword="true"/> to stop waiting early, such as when the launcher process has exited.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task<bool> WaitForInitializationAsync(Func<bool>? hasFailed = null, CancellationToken cancellationToken = default)
    {
        bool isInitialized = false;
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (!isInitialized && initializationStopwatch.Elapsed < this.InitializationTimeout && hasFailed?.Invoke() != true)
        {
            // A remote end that accepts the connection but never answers must not outlast the timeout.
            using CancellationTokenSource requestTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            TimeSpan remaining = this.InitializationTimeout - initializationStopwatch.Elapsed;
            requestTokenSource.CancelAfter(TimeSpan.FromTicks(Math.Max(remaining.Ticks, 0)));
            try
            {
                using HttpResponseMessage response = await this.httpClient.GetAsync($"{this.ServiceUrl}/status", requestTokenSource.Token).ConfigureAwait(false);

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
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            if (!isInitialized && initializationStopwatch.Elapsed < this.InitializationTimeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
            }
        }

        initializationStopwatch.Stop();
        return isInitialized;
    }
}
