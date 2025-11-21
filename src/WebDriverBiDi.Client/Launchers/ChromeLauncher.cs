// <copyright file="ChromeLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Object for launching a Chrome browser to connect to using a WebDriverBiDi session.
/// This browser launcher does not rely on any external executable except for the browser itself.
/// </summary>
public class ChromeLauncher : BrowserLauncher
{
    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly ObservableEvent<LogMessageEventArgs> onLogMessageEvent = new("chromeLauncher.logMessage");

    private readonly List<string> disabledFeatures = [
      "Translate",

      // AcceptCHFrame disabled because of crbug.com/1348106.
      "AcceptCHFrame",
      "MediaRouter",
      "OptimizationHints",
   ];

    private readonly List<string> enabledFeatures = [
        "PdfOopif",
    ];

    private string userDataDirectory = string.Empty;

    private List<string> chromeArguments = [
        "--allow-browser-signin=false",
        "--allow-pre-commit-input",
        "--disable-background-networking",
        "--disable-background-timer-throttling",
        "--disable-backgrounding-occluded-windows",
        "--disable-breakpad",
        "--disable-client-side-phishing-detection",
        "--disable-component-extensions-with-background-pages",
        "--disable-component-update",
        "--disable-default-apps",
        "--disable-dev-shm-usage",
        "--disable-extensions",
        "--disable-hang-monitor",
        "--disable-infobars",
        "--disable-ipc-flooding-protection",
        "--disable-notifications",
        "--disable-popup-blocking",
        "--disable-prompt-on-repost",
        "--disable-renderer-backgrounding",
        "--disable-search-engine-choice-screen",
        "--disable-sync",
        "--enable-automation",
        "--export-tagged-pdf",
        "--generate-pdf-document-outline",
        "--force-color-profile=srgb",
        "--metrics-recording-only",
        "--no-default-browser-check",
        "--no-first-run",
        "--password-store=basic",
        "--use-mock-keychain",
    ];

    private Process? browserProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    public ChromeLauncher()
        : this(string.Empty, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    /// <param name="browserExecutableLocation">The path and executable name of the browser executable.</param>
    public ChromeLauncher(string browserExecutableLocation)
        : this(browserExecutableLocation, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    /// <param name="browserExecutableLocation">The path and executable name of the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    public ChromeLauncher(string browserExecutableLocation, int port)
        : base(string.Empty, port, browserExecutableLocation)
    {
        if (string.IsNullOrEmpty(browserExecutableLocation))
        {
            this.BrowserExecutableLocation = this.GetDefaultBrowserExecutableLocation();
        }
        else
        {
            this.BrowserExecutableLocation = browserExecutableLocation;
        }
    }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    public override ObservableEvent<LogMessageEventArgs> OnLogMessage => this.onLogMessageEvent;

    /// <summary>
    /// Gets a value indicating whether the service is running.
    /// </summary>
    public bool IsRunning => this.browserProcess is not null && !this.browserProcess.HasExited;

    private IList<string> CommandLineArguments
    {
        get
        {
            List<string> args = [.. this.chromeArguments];
            args.Add($"--disable-features=${string.Join(",", this.disabledFeatures)}");
            args.Add($"--enable-features=${string.Join(",", this.enabledFeatures)}");
            args.Add($"--user-data-dir={this.userDataDirectory}");
            args.Add($"--remote-debugging-port={this.Port}");
            args.Add("about:blank");
            return args.AsReadOnly();
        }
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StartAsync()
    {
        // No operation required to start the launcher.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously launches the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="BrowserNotLaunchedException">Thrown when the browser cannot be launched.</exception>
    public override async Task LaunchBrowserAsync()
    {
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

            this.CreateUserDataDirectory();

            this.browserProcess = new Process();
            this.browserProcess.StartInfo.FileName = this.BrowserExecutableLocation;
            this.browserProcess.StartInfo.Arguments = string.Join(" ", this.CommandLineArguments);
            this.browserProcess.StartInfo.UseShellExecute = false;
            this.browserProcess.StartInfo.RedirectStandardOutput = true;
            this.browserProcess.StartInfo.RedirectStandardError = true;
            this.browserProcess.StartInfo.CreateNoWindow = true;
            this.browserProcess.ErrorDataReceived += this.ReadStandardError;
            this.browserProcess.OutputDataReceived += this.ReadStandardOutput;
            this.browserProcess.Start();
            this.browserProcess.BeginOutputReadLine();
            this.browserProcess.BeginErrorReadLine();
            bool launcherAvailable = await this.WaitForInitializationAsync().ConfigureAwait(false);
        }
        finally
        {
            LockObject.Release();
        }
    }

    /// <summary>
    /// Asynchronously quits the browser.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser could not be exited.</exception>
    public override Task QuitBrowserAsync()
    {
        if (this.browserProcess is not null)
        {
            if (!this.browserProcess.HasExited)
            {
                this.browserProcess.Kill();
            }

            this.browserProcess = null;
            this.RemoveUserDataDirectory();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StopAsync()
    {
        // No operation required to stop the launcher.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a Transport object that can be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Transport"/> to be used in instantiating the driver.</returns>
    public override Transport CreateTransport()
    {
        return new ChromiumTransport();
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

            if (!string.IsNullOrEmpty(this.WebSocketUrl))
            {
                isInitialized = true;
                break;
            }
            else
            {
                await Task.Delay(100);
            }
        }

        return isInitialized;
    }

    private void CreateUserDataDirectory()
    {
        string tempPath = Path.GetTempPath();
        string directoryName = Path.Combine(tempPath, $"webdriverbidi-net-chrome-data-{Guid.NewGuid()}");
        DirectoryInfo info = Directory.CreateDirectory(directoryName);
        this.userDataDirectory = info.FullName;
    }

    private void RemoveUserDataDirectory()
    {
        // NOTE: This is a naive algorithm for demonstration purposes only.
        // Production code might do something like allow the user to keep
        // the profile directory around for examination after shutting the
        // browser down.
        if (!string.IsNullOrEmpty(this.userDataDirectory) && Directory.Exists(this.userDataDirectory))
        {
            try
            {
                Directory.Delete(this.userDataDirectory, true);
            }
            catch (IOException)
            {
            }
        }
    }

    private void ReadStandardError(object sender, DataReceivedEventArgs e)
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

    private void ReadStandardOutput(object sender, DataReceivedEventArgs e)
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

    private string GetDefaultBrowserExecutableLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (File.Exists("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"))
            {
                return "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            }

            return "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            List<string> linuxLocations =
            [
                "/usr/local/sbin",
                "/usr/local/bin",
                "/usr/sbin",
                "/usr/bin",
                "/sbin",
                "/bin",
                "/opt/google/chrome",
            ];
            foreach (string linuxLocation in linuxLocations)
            {
                string fullPath = Path.Combine(linuxLocation, "chrome");
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                fullPath = Path.Combine(linuxLocation, "google-chrome");
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return string.Empty;
    }
}
