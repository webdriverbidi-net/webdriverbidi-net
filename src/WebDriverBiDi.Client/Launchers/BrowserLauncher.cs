// <copyright file="BrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WebDriverBiDi.Protocol;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriverBiDi session.
/// </summary>
public abstract class BrowserLauncher : IAsyncDisposable
{
    /// <summary>
    /// Gets the the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Launcher";

    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The <see cref="BrowserLocatorSettings"/> settings to use for locating the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    internal BrowserLauncher(BrowserLocatorSettings browserLocatorSettings, int port)
    {
        if (browserLocatorSettings is null)
        {
            throw new ArgumentNullException(nameof(browserLocatorSettings));
        }

        this.Port = port;
        this.BrowserLocator = new BrowserLocator(browserLocatorSettings);
        this.BrowserLocator.OnLogMessage.AddObserver(this.OnLocatorLogAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    public abstract ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }

    /// <summary>
    /// Gets or sets a value indicating the time to wait for an initial connection before timing out.
    /// </summary>
    public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Gets or sets the WebSocket URL for communicating with the browser via the WebDriver BiDi protocol.
    /// </summary>
    public string WebSocketUrl { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port on which the launcher should listen.
    /// </summary>
    public int Port { get; set; } = 0;

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public virtual bool IsBiDiSessionInitialized => false;

    /// <summary>
    /// Gets a value indicating whether the browser can be closed using WebDriver BiDi's browser.close command.
    /// </summary>
    public virtual bool IsBrowserCloseAllowed => true;

    /// <summary>
    /// Gets or sets a value indicating whether to run the browser in an invisible (headless) mode.
    /// </summary>
    public bool IsBrowserHeadless { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the browser is currently running.
    /// </summary>
    public abstract bool IsRunning { get; }

    /// <summary>
    /// Gets or sets the <see cref="BrowserLocator"/> to use for locating the browser executable.
    /// </summary>
    internal BrowserLocator BrowserLocator { get; set; }

    /// <summary>
    /// Creates a launcher for the specified browser using default settings.
    /// The browser will be auto-downloaded if not found, use the stable channel, and run in headed mode.
    /// </summary>
    /// <param name="browser">The browser to launch.</param>
    /// <param name="headless">Whether to run the browser in headless mode. Default is false.</param>
    /// <returns>The configured browser launcher.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    public static BrowserLauncher Create(Browser browser, bool headless = false)
    {
        BrowserLauncherBuilder builder = Configure(browser);
        if (headless)
        {
            builder.WithHeadlessOption();
        }

        return builder.Build();
    }

    /// <summary>
    /// Returns a builder for configuring a launcher for the specified browser.
    /// Use this method to access the full fluent API for advanced configuration.
    /// </summary>
    /// <param name="browser">The browser to launch.</param>
    /// <returns>A <see cref="BrowserLauncherBuilder"/> for configuring the launcher.</returns>
    public static BrowserLauncherBuilder Configure(Browser browser)
    {
        return new BrowserLauncherBuilder(browser);
    }

    /// <summary>
    /// Asynchronously launches the browser and returns a <see cref="BrowserInstance"/> representing the running browser.
    /// This is the recommended method for launching browsers with the new API.
    /// </summary>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public virtual async Task<BrowserInstance> LaunchAsync()
    {
        this.ThrowIfDisposed();
        await this.StartAsync().ConfigureAwait(false);
        return await this.LaunchBrowserAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public abstract Task StartAsync();

    /// <summary>
    /// Asynchronously launches the browser and returns a <see cref="BrowserInstance"/> representing the running browser.
    /// </summary>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    public abstract Task<BrowserInstance> LaunchBrowserAsync();

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public abstract Task QuitBrowserAsync();

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public abstract Task StopAsync();

    /// <summary>
    /// Creates a <see cref="Transport"/> object that can be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Transport"/> to be used in instantiating the driver.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public virtual Transport CreateTransport()
    {
        this.ThrowIfDisposed();
        return new Transport(this.CreateConnection());
    }

    /// <summary>
    /// Asynchronously releases the resources used by this browser launcher.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finds a random, free port to be listened on.
    /// </summary>
    /// <returns>A random, free port to be listened on.</returns>
    protected static int FindFreePort()
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
    /// Gets the process ID of the browser process, or 0 if not applicable (e.g., remote browsers).
    /// </summary>
    /// <returns>The process ID, or 0 if not applicable.</returns>
    protected virtual int GetProcessId()
    {
        return 0;
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
    protected void ThrowIfDisposed()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this browser launcher.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        try
        {
            await this.QuitBrowserAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from QuitBrowserAsync to ensure StopAsync is called
        }

        try
        {
            await this.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from StopAsync during disposal
        }
    }

    /// <summary>
    /// Creates the <see cref="Connection"/> object to be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Connection"/> object to be used to communicate with the browser.</returns>
    protected virtual Connection CreateConnection()
    {
        return new WebSocketConnection();
    }

    /// <summary>
    /// Provides a handler for reading the standard error stream of the browser launcher process.
    /// This allows the launcher to detect the WebSocket URL on which to connect to the WebDriver
    /// BiDi remote end.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    protected virtual void ReadConsoleOutputForWebSocketUrl(object sender, DataReceivedEventArgs e)
    {
        Regex websocketUrlMatcher = new(@"DevTools listening on (ws:\/\/.*)$", RegexOptions.IgnoreCase);
        if (e.Data is not null)
        {
            Match regexMatch = websocketUrlMatcher.Match(e.Data);
            if (regexMatch.Success)
            {
                this.WebSocketUrl = regexMatch.Groups[1].Value;
            }
        }
    }

    /// <summary>
    /// Logs a message from the browser launcher to the OnLogMessage observable event with the specified log level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="component">The component name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel logLevel = WebDriverBiDiLogLevel.Info, string component = LoggerComponentName)
    {
        LogMessageEventArgs logMessageArgs = new(message, logLevel, component);
        await this.OnLogMessage.NotifyObserversAsync(logMessageArgs);
    }

    private async Task OnLocatorLogAsync(LogMessageEventArgs args)
    {
        await this.LogAsync(args.Message, args.Level, args.ComponentName).ConfigureAwait(false);
    }
}
