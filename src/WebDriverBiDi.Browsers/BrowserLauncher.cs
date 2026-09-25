// <copyright file="BrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WebDriverBiDi.Protocol;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriverBiDi session.
/// </summary>
/// <remarks>
/// A launcher owns everything it starts, and runs one browser at a time: disposing it quits its
/// browser and stops any driver it started. A <see cref="BrowserInstance"/> is a handle to one
/// launch, which quits the browser only while that launch is still the launcher's current one.
/// </remarks>
public abstract class BrowserLauncher : IAsyncDisposable
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Launcher";

    private bool disposed = false;
    private int launchCount = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The <see cref="BrowserLocatorSettings"/> settings to use for locating the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    internal BrowserLauncher(BrowserLocatorSettings browserLocatorSettings, int port)
    {
        this.Port = port;
        this.BrowserLocator = new BrowserLocator(browserLocatorSettings);
        this.BrowserLocator.OnLogMessage.AddObserver(this.OnLocatorLogAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.InvocableLogMessageObservableEvent;

    /// <summary>
    /// Gets or sets a value indicating the time to wait for an initial connection before timing out.
    /// </summary>
    /// <remarks>
    /// This bounds two distinct waits: the wait for the launched browser to report that its
    /// WebDriver BiDi endpoint is accepting connections, and the startup retries of the
    /// <see cref="Connection"/> created by <see cref="CreateConnection"/>. The value is read when
    /// the connection is created, so it must be set before calling <see cref="CreateTransport"/>
    /// to affect the latter.
    /// </remarks>
    public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Gets or sets how long <see cref="QuitBrowserAsync"/> waits for a browser that has been asked
    /// to exit before killing it and every process it started. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the connection string for communicating with the browser via the WebDriver BiDi protocol.
    /// For a WebSocket connection, this is the URL to the WebSocket; for other connection types, see the
    /// documentation for the connection type.
    /// </summary>
    public string ConnectionString { get; protected set; } = string.Empty;

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
    /// Gets or sets the settings for the launched browser process.
    /// </summary>
    internal LaunchSettings LaunchSettings { get; set; } = new();

    /// <summary>
    /// Gets the identity of the most recent successful launch, so that a <see cref="BrowserInstance"/>
    /// from an earlier launch cannot quit a browser launched after it.
    /// </summary>
    internal int CurrentLaunchId => this.launchCount;

    /// <summary>
    /// Gets an ObservableEventInvocable that subclasses can use to raise the OnLogMessage event.
    /// </summary>
    protected abstract ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; }

    /// <summary>
    /// Creates a launcher for the specified browser using default settings.
    /// The browser will be auto-downloaded if not found, use the stable channel, and run in headed mode.
    /// </summary>
    /// <param name="browser">The browser to launch.</param>
    /// <param name="headless">Whether to run the browser in headless mode. Default is false.</param>
    /// <returns>The configured browser launcher.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    public static BrowserLauncher Create(BrowserKind browser, bool headless = false)
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
    public static BrowserLauncherBuilder Configure(BrowserKind browser)
    {
        return new BrowserLauncherBuilder(browser);
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// For a browser that is launched by a WebDriver Classic driver executable (chromedriver,
    /// geckodriver, safaridriver, etc.), this method will start that executable. It will also
    /// wait for the remote end provided by that executable to be available for commands via
    /// the WebDriver Classic Status command.
    /// </para>
    /// <para>
    /// For browsers that support direct WebDriver BiDi connections, this method does nothing.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that cancels waiting for the launcher to stop. A driver executable is killed at once, so
    /// stopping one is not cancelled.
    /// </param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <remarks>
    /// For a browser that is launched by a driver executable (chromedriver, geckodriver,
    /// safaridriver, etc), this method will terminate that executable. For browsers that
    /// support direct WebDriver BiDi connections, this method does nothing.
    /// </remarks>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously launches the browser and returns a <see cref="BrowserInstance"/> representing the running browser.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the launch; anything already started is stopped.</param>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserDownloadException">Thrown when the browser or driver cannot be located or downloaded.</exception>
    /// <exception cref="BrowserLaunchException">Thrown when the browser cannot be launched.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a browser launched by this launcher is still running.</exception>
    public abstract Task<BrowserInstance> LaunchBrowserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that cancels waiting for the browser to exit; the browser is then killed, after which an
    /// <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public abstract Task QuitBrowserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously forces the browser to terminate, for use when <see cref="QuitBrowserAsync"/> has failed.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that cancels waiting for the killed browser to exit, after which an
    /// <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// The default implementation does nothing, which is correct for launchers that do not own a local
    /// browser process, such as a launcher connected to a remote grid.
    /// </remarks>
    public virtual Task KillBrowserAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

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
    /// Asynchronously starts the launcher, then launches the browser and returns a <see cref="BrowserInstance"/>
    /// representing the running browser.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the launch; anything already started is stopped.</param>
    /// <returns>A task that resolves to a <see cref="BrowserInstance"/> representing the running browser.</returns>
    /// <exception cref="BrowserDownloadException">Thrown when the browser or driver cannot be located or downloaded.</exception>
    /// <exception cref="BrowserLaunchException">Thrown when the browser cannot be launched.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public virtual async Task<BrowserInstance> LaunchAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        await this.StartAsync(cancellationToken).ConfigureAwait(false);
        return await this.LaunchBrowserAsync(cancellationToken).ConfigureAwait(false);
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

        await this.StopAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the <see cref="Connection"/> object to be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Connection"/> object to be used to communicate with the browser.</returns>
    protected virtual Connection CreateConnection()
    {
        return new WebSocketConnection()
        {
            StartupTimeout = this.InitializationTimeout,
        };
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
                this.ConnectionString = regexMatch.Groups[1].Value;
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
        await this.InvocableLogMessageObservableEvent.InvokeNotifyObserversAsync(logMessageArgs).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts a browser or driver process.
    /// </summary>
    /// <param name="process">The process, with its start information set.</param>
    /// <param name="description">What the process is, such as "Chrome", for the error message.</param>
    /// <exception cref="BrowserLaunchException">Thrown when the executable cannot be started, such as when it is not executable.</exception>
    private protected static void StartProcess(Process process, string description)
    {
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            process.Dispose();
            throw new BrowserLaunchException($"Unable to start {description} from {process.StartInfo.FileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Records a successful launch and creates the <see cref="BrowserInstance"/> representing it.
    /// </summary>
    /// <param name="connectionString">The connection string of the launched browser.</param>
    /// <param name="processId">The process ID of the launched browser, or 0 if it is not a local process.</param>
    /// <returns>The browser instance.</returns>
    private protected BrowserInstance CreateBrowserInstance(string connectionString, int processId)
    {
        this.launchCount++;
        return new BrowserInstance(this, connectionString, processId, this.launchCount);
    }

    private async Task OnLocatorLogAsync(LogMessageEventArgs args)
    {
        await this.LogAsync(args.Message, args.Level, args.ComponentName).ConfigureAwait(false);
    }
}
