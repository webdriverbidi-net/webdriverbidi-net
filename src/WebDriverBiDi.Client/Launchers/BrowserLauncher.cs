// <copyright file="BrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using WebDriverBiDi.Protocol;

/// <summary>
/// Abstract base class for launching a browser to connect to using a WebDriverBiDi session.
/// </summary>
public abstract class BrowserLauncher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncher"/> class.
    /// </summary>
    /// <param name="launcherExecutablePath">The path to the directory containing the launcher executable.</param>
    /// <param name="port">The port on which the launcher executable should listen for connections.</param>
    /// <param name="browserExecutableLocation">The path containing the directory and file name of the browser executable. Default to an empty string, indicating to use the default installed browser.</param>
    protected BrowserLauncher(string launcherExecutablePath, int port, string browserExecutableLocation)
    {
        this.LauncherPath = launcherExecutablePath;
        this.Port = port;
        this.BrowserExecutableLocation = browserExecutableLocation;
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
    public int Port { get; set; }

    /// <summary>
    /// Gets a value indicating whether the launched browser has a provided WebDriver BiDi
    /// session as part of its initialization.
    /// </summary>
    public virtual bool IsBiDiSessionInitialized => false;

    /// <summary>
    /// Gets or sets the location of the browser executable.
    /// </summary>
    protected string BrowserExecutableLocation { get; set; }

    /// <summary>
    /// Gets the path of the directory containing the launcher executable.
    /// </summary>
    protected string LauncherPath { get; }

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
        if (browserType == BrowserType.Chrome)
        {
            return new ChromeLauncher(browserExecutableLocation);
        }

        if (browserType == BrowserType.Firefox)
        {
            return new FirefoxLauncher(browserExecutableLocation);
        }

        if (browserType == BrowserType.ChromeDriver)
        {
            return new ChromeDriverLauncher(launcherPath, browserExecutableLocation);
        }

        if (browserType == BrowserType.GeckoDriver)
        {
            return new GeckoDriverLauncher(launcherPath, browserExecutableLocation);
        }

        throw new WebDriverBiDiException("Invalid browser type");
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
    /// Creates a Transport object that can be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Transport"/> to be used in instantiating the driver.</returns>
    public virtual Transport CreateTransport()
    {
        return new Transport();
    }
}
