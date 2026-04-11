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

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    protected BrowserLauncher(int port)
    {
        this.Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The <see cref="BrowserLocatorSettings"/> settings to use for locating the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    protected BrowserLauncher(BrowserLocatorSettings browserLocatorSettings, int port)
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
    /// Gets or sets the <see cref="BrowserLocator"/> to use for locating the browser executable.
    /// </summary>
    protected BrowserLocator BrowserLocator { get; set; } = null!;

    /// <summary>
    /// Creates a launcher creator for the specified browser using the specified browser type.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> for which to create a launcher creator.</param>
    /// <returns>The <see cref="Creator"/> for the specified browser type.</returns>
    public static Creator ForBrowser(BrowserType browserType)
    {
        return new Creator(browserType);
    }

    /// <summary>
    /// Creates a launcher for the specified browser type and release channel.
    /// </summary>
    /// <param name="browserType">The type of browser for which to create a launcher.</param>
    /// <param name="releaseChannel">The release channel for the browser.</param>
    /// <returns>The launcher for the specified browser type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when an invalid browser type is specified.</exception>
    public static BrowserLauncher Create(BrowserType browserType, BrowserReleaseChannel releaseChannel = BrowserReleaseChannel.Stable)
    {
        Creator creator = ForBrowser(browserType).UsingReleaseChannel(releaseChannel).AtDefaultInstallLocation();
        return creator.Create();
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public abstract Task StartAsync();

    /// <summary>
    /// Asynchronously launches the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    public abstract Task LaunchBrowserAsync();

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
    public virtual Transport CreateTransport()
    {
        return new Transport(this.CreateConnection());
    }

    /// <summary>
    /// Asynchronously releases the resources used by this browser launcher.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
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
    /// Asynchronously releases the resources used by this browser launcher.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await this.QuitBrowserAsync();
        await this.StopAsync();
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

    /// <summary>
    /// Defines a builder for creating a <see cref="BrowserLauncher"/> with specific settings such as release channel,
    /// browser executable location, and headless mode. The builder allows for a fluent API to configure the desired
    /// settings before creating the launcher instance.
    /// </summary>
    public class Creator
    {
        private readonly BrowserType browserType;
        private BrowserReleaseChannel releaseChannel = BrowserReleaseChannel.Stable;
        private FileLocationBehavior? browserLocationBehavior;
        private string browserExecutableLocation = string.Empty;
        private string driverExecutableLocation = string.Empty;
        private FileLocationBehavior? driverLocationBehavior;
        private string version = BrowserLocatorSettings.LatestVersionString;
        private string hostName = string.Empty;
        private bool useSsl = false;
        private bool isHeadless = false;
        private int port = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Creator"/> class for the specified browser type.
        /// </summary>
        /// <param name="browserType">The browser type for which to create a launcher.</param>
        internal Creator(BrowserType browserType)
        {
            this.browserType = browserType;
        }

        /// <summary>
        /// Specifies the release channel to use for the browser launcher being created.
        /// </summary>
        /// <param name="releaseChannel">The release channel to use.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        public Creator UsingReleaseChannel(BrowserReleaseChannel releaseChannel)
        {
            this.releaseChannel = releaseChannel;
            return this;
        }

        /// <summary>
        /// Specifies to automatically locate the browser executable, downloading it if needed,
        /// for the browser launcher being created.
        /// </summary>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="WebDriverBiDiException">Thrown when the user has spefified to either use the default browser install location or a custom browser install location.</exception>
        public Creator AutoLocateBrowser()
        {
            if (this.browserLocationBehavior is null)
            {
                this.browserLocationBehavior = FileLocationBehavior.AutoLocateAndDownload;
            }
            else if (this.browserLocationBehavior != FileLocationBehavior.AutoLocateAndDownload)
            {
                string errorDetail = this.browserLocationBehavior == FileLocationBehavior.UseSystemInstallLocation
                    ? "to use the default install location"
                    : $"to use a custom location ({this.browserExecutableLocation})";
                throw new WebDriverBiDiException($"Cannot specify auto-location for the browser executable; you already specified {errorDetail}.");
            }

            return this;
        }

        /// <summary>
        /// Specifies to use the default installed browser executable location for the browser launcher being created.
        /// </summary>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="WebDriverBiDiException">Thrown when the user has spefified to either use a custom browser install location or to automatically locate and download the browser executable.</exception>
        public Creator AtDefaultInstallLocation()
        {
            if (this.browserLocationBehavior is null)
            {
                this.browserLocationBehavior = FileLocationBehavior.UseSystemInstallLocation;
            }
            else if (this.browserLocationBehavior != FileLocationBehavior.UseSystemInstallLocation)
            {
                string errorDetail = this.browserLocationBehavior == FileLocationBehavior.AutoLocateAndDownload
                    ? "to automatically locate the browser executable, downloading if needed"
                    : $"to use a custom location ({this.browserExecutableLocation})";
                throw new WebDriverBiDiException($"Cannot specify a custom browser executable location; you already specified {errorDetail}.");
            }

            return this;
        }

        /// <summary>
        /// Specifies the custom location of the browser executable for the browser launcher being created.
        /// </summary>
        /// <param name="browserExecutableLocation">The custom location of the browser executable.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="WebDriverBiDiException">Thrown when the user has spefified to either use the default browser install location or to automatically locate and download the browser executable.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified browser executable location is null or empty.</exception>
        public Creator AtLocation(string browserExecutableLocation)
        {
            if (string.IsNullOrEmpty(browserExecutableLocation))
            {
                throw new ArgumentException("Browser executable location cannot be null or empty.", nameof(browserExecutableLocation));
            }

            if (this.browserLocationBehavior is null)
            {
                this.browserLocationBehavior = FileLocationBehavior.UseCustomLocation;
            }
            else if (this.browserLocationBehavior != FileLocationBehavior.UseCustomLocation)
            {
                string errorDetail = this.browserLocationBehavior == FileLocationBehavior.UseSystemInstallLocation
                    ? "to use the default browser install location"
                    : "to automatically locate the browser executable, downloading if needed";
                throw new WebDriverBiDiException($"Cannot specify a custom browser executable location; you already specified {errorDetail}.");
            }

            this.browserExecutableLocation = browserExecutableLocation;
            return this;
        }

        /// <summary>
        /// Specifies the version of the browser to use for the browser launcher being created.
        /// This is only applicable when the user has specified to automatically locate the browser
        /// executable, downloading if needed; if a version is specified without selecting
        /// auto-location, an exception will be thrown.
        /// </summary>
        /// <param name="version">The version of the browser to use.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified browser version is null or empty.</exception>
        /// <exception cref="WebDriverBiDiException">Thrown when the user has not specified to automatically locate the browser executable.</exception>
        public Creator WithVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Browser version cannot be null or empty.", nameof(version));
            }

            if (this.browserLocationBehavior is null)
            {
                this.browserLocationBehavior = FileLocationBehavior.AutoLocateAndDownload;
            }
            else if (this.browserLocationBehavior != FileLocationBehavior.AutoLocateAndDownload)
            {
                string errorDetail = this.browserLocationBehavior == FileLocationBehavior.UseSystemInstallLocation
                    ? "to use the default browser install location"
                    : $"to use a custom location ({this.browserExecutableLocation})";
                throw new WebDriverBiDiException($"Cannot specify a browser version; you already specified {errorDetail}.");
            }

            this.version = version;
            return this;
        }

        /// <summary>
        /// Specifies the port on which the browser launcher should listen for connections.
        /// </summary>
        /// <param name="port">The port on which the browser launcher should listen for connections.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        public Creator WithPort(int port)
        {
            this.port = port;
            return this;
        }

        /// <summary>
        /// Specifies to run the launched browser in an invisible (headless) mode for the browser launcher being created.
        /// </summary>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        public Creator UseHeadlessBrowser()
        {
            this.isHeadless = true;
            return this;
        }

        /// <summary>
        /// Specifies to locate the driver executable on the system PATH for the browser launcher being created.
        /// </summary>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="WebDriverBiDiException">Thrown when the browser type for this launcher does not use a driver executable or the user has already specified to use the system install location.</exception>
        public Creator WithDriverExecutableOnPath()
        {
            if (this.browserType != BrowserType.ChromeDriver && this.browserType != BrowserType.GeckoDriver)
            {
                throw new WebDriverBiDiException("Cannot specify a driver executable location for a browser type that does not use a driver executable.");
            }

            if (this.driverLocationBehavior is null)
            {
                this.driverLocationBehavior = FileLocationBehavior.UseSystemInstallLocation;
            }
            else if (this.driverLocationBehavior != FileLocationBehavior.UseSystemInstallLocation)
            {
                string errorDetail = this.driverLocationBehavior == FileLocationBehavior.AutoLocateAndDownload
                    ? "to automatically locate the driver executable, downloading if needed"
                    : $"to use a custom location ({this.driverExecutableLocation})";
                throw new WebDriverBiDiException($"Cannot specify to locate the driver executable on the system PATH; you already specified {errorDetail}.");
            }

            return this;
        }

        /// <summary>
        /// Specifies to use a browser located on a remote host, like when using Selenium Grid, for the browser launcher being created.
        /// </summary>
        /// <param name="hostName">The name of the remote host.</param>
        /// <param name="useSsl">A value indicating whether to use SSL for the connection.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        public Creator OnRemoteHost(string hostName, bool useSsl = false)
        {
            this.hostName = hostName;
            this.useSsl = useSsl;
            return this;
        }

        /// <summary>
        /// Specifies the custom location of the driver executable for the browser launcher being created.
        /// </summary>
        /// <param name="driverExecutableLocation">The custom location of the driver executable.</param>
        /// <returns>The current instance of the <see cref="Creator"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified driver executable location is null or empty.</exception>
        /// <exception cref="WebDriverBiDiException">Thrown when the browser type for this launcher does not use a driver executable or the user has already specified to use the system install location.</exception>
        public Creator WithDriverExecutableLocation(string driverExecutableLocation)
        {
            if (string.IsNullOrEmpty(driverExecutableLocation))
            {
                throw new ArgumentException("Driver executable location cannot be null or empty.", nameof(driverExecutableLocation));
            }

            if (this.browserType != BrowserType.ChromeDriver && this.browserType != BrowserType.GeckoDriver)
            {
                throw new WebDriverBiDiException("Cannot specify a driver executable location for a browser type that does not use a driver executable.");
            }

            if (this.driverLocationBehavior is null)
            {
                this.driverLocationBehavior = FileLocationBehavior.UseCustomLocation;
            }
            else if (this.driverLocationBehavior != FileLocationBehavior.UseCustomLocation)
            {
                string errorDetail = this.driverLocationBehavior == FileLocationBehavior.AutoLocateAndDownload
                    ? "to automatically locate the driver executable, downloading if needed"
                    : $"to use the instance of {this.driverExecutableLocation} on the system PATH";
                throw new WebDriverBiDiException($"Cannot specify a custom driver executable location; you already specified {errorDetail}.");
            }

            this.driverExecutableLocation = driverExecutableLocation;
            return this;
        }

        /// <summary>
        /// Creates the <see cref="BrowserLauncher"/> instance based on the specified settings in this builder.
        /// </summary>
        /// <returns>The created <see cref="BrowserLauncher"/> instance.</returns>
        /// <exception cref="WebDriverBiDiException">Thrown when an invalid browser type is specified.</exception>
        public BrowserLauncher Create()
        {
            if (!string.IsNullOrEmpty(this.hostName))
            {
                string browserName = this.browserType switch
                {
                    BrowserType.Chrome or BrowserType.ChromePipe or BrowserType.ChromeDriver => "Chrome",
                    BrowserType.Firefox or BrowserType.GeckoDriver => "Firefox",
                    _ => throw new WebDriverBiDiException("Invalid browser type for remote connection"),
                };
                return new WebDriverClassicBrowserLauncher(new RemoteBrowserLocatorSettings(browserName, this.hostName, this.useSsl))
                {
                    Port = this.port,
                };
            }

            this.browserLocationBehavior ??= FileLocationBehavior.UseSystemInstallLocation;

            if (this.browserType == BrowserType.Chrome || this.browserType == BrowserType.ChromePipe || this.browserType == BrowserType.ChromeDriver)
            {
                ChromeChannel chromeChannel = this.releaseChannel switch
                {
                    BrowserReleaseChannel.Stable => ChromeChannel.Stable,
                    BrowserReleaseChannel.Beta => ChromeChannel.Beta,
                    BrowserReleaseChannel.DeveloperPreview => ChromeChannel.Dev,
                    BrowserReleaseChannel.Alpha => ChromeChannel.Canary,
                    _ => throw new WebDriverBiDiException("Invalid browser release channel for Chrome"),
                };

                ChromeBrowserLocatorSettings settings = new(chromeChannel, this.browserLocationBehavior.Value, this.browserExecutableLocation, this.version);
                if (this.browserType == BrowserType.ChromeDriver)
                {
                    settings.IncludeDriver = true;
                    if (!string.IsNullOrEmpty(this.driverExecutableLocation))
                    {
                        settings.DriverExecutableLocation = this.driverExecutableLocation;
                    }

                    settings.DriverLocationBehavior = this.driverLocationBehavior ?? FileLocationBehavior.AutoLocateAndDownload;
                    ChromeDriverLauncher chromeDriverLauncher = new(settings)
                    {
                        IsBrowserHeadless = this.isHeadless,
                        Port = this.port,
                    };

                    return chromeDriverLauncher;
                }

                ChromeLauncher chromeLauncher = new(settings)
                {
                    IsBrowserHeadless = this.isHeadless,
                    Port = this.port,
                    ConnectionType = this.browserType == BrowserType.ChromePipe ? ConnectionType.Pipes : ConnectionType.WebSocket,
                };

                return chromeLauncher;
            }

            if (this.browserType == BrowserType.Firefox || this.browserType == BrowserType.GeckoDriver)
            {
                FirefoxChannel firefoxChannel = this.releaseChannel switch
                {
                    BrowserReleaseChannel.Stable => FirefoxChannel.Stable,
                    BrowserReleaseChannel.Beta => FirefoxChannel.Beta,
                    BrowserReleaseChannel.DeveloperPreview => FirefoxChannel.Dev,
                    BrowserReleaseChannel.Alpha => FirefoxChannel.Nightly,
                    _ => throw new WebDriverBiDiException("Invalid browser release channel for Firefox"),
                };

                FirefoxBrowserLocatorSettings settings = new(firefoxChannel, this.browserLocationBehavior.Value, this.browserExecutableLocation, this.version);
                if (this.browserType == BrowserType.GeckoDriver)
                {
                    settings.IncludeDriver = true;
                    if (!string.IsNullOrEmpty(this.driverExecutableLocation))
                    {
                        settings.DriverExecutableLocation = this.driverExecutableLocation;
                    }

                    settings.DriverLocationBehavior = this.driverLocationBehavior ?? FileLocationBehavior.AutoLocateAndDownload;
                    GeckoDriverLauncher geckoDriverLauncher = new(settings)
                    {
                        IsBrowserHeadless = this.isHeadless,
                        Port = this.port,
                    };

                    return geckoDriverLauncher;
                }

                FirefoxLauncher firefoxLauncher = new(settings)
                {
                    IsBrowserHeadless = this.isHeadless,
                    Port = this.port,
                };

                return firefoxLauncher;
            }

            throw new WebDriverBiDiException("Invalid browser type");
        }
    }
}
