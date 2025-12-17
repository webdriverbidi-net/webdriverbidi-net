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

    private Connection? connection;

    private Process? browserProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeLauncher"/> class.
    /// </summary>
    public ChromeLauncher()
        : this(string.Empty)
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
        // this.CreateConnection();
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

    /// <summary>
    /// Gets or sets a value indicating the type of connection to use in communicating with the browser.
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.WebSocket;

    private IList<string> CommandLineArguments
    {
        get
        {
            List<string> args = [.. this.chromeArguments];
            args.Add($"--disable-features=${string.Join(",", this.disabledFeatures)}");
            args.Add($"--enable-features=${string.Join(",", this.enabledFeatures)}");
            args.Add($"--user-data-dir={this.userDataDirectory}");
            if (this.ConnectionType == ConnectionType.Pipes)
            {
                args.Add("--remote-debugging-pipe");
            }
            else
            {
                args.Add($"--remote-debugging-port={this.Port}");
            }

            if (this.IsBrowserHeadless)
            {
                args.Add("--headless=new");
                args.Add("--disable-dev-shm-usage");
                args.Add("--no-sandbox");
                args.Add("--disable-gpu");
            }

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
        this.connection = this.CreateConnection();
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

            this.browserProcess = new Process
            {
                StartInfo = this.CreateProcessStartInfo(),
            };
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
    public override async Task QuitBrowserAsync()
    {
        if (this.connection is not null && this.connection.IsActive && this.connection.ConnectionType == ConnectionType.Pipes && this.connection is PipeConnection pipeConnection)
        {
            await pipeConnection.StopAsync().ConfigureAwait(false);
            this.connection = null;
        }

        if (this.browserProcess is not null)
        {
            if (!this.browserProcess.HasExited)
            {
                this.browserProcess.Kill();
                this.browserProcess.WaitForExit();
            }

            this.browserProcess = null;
            this.RemoveUserDataDirectory();
        }
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
        return new ChromiumTransport(this.connection ?? this.CreateConnection());
    }

    /// <summary>
    /// Creates the <see cref="Connection"/> object to be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Connection"/> object to be used to communicate with the browser.</returns>
    protected override Connection CreateConnection()
    {
        if (this.ConnectionType == ConnectionType.Pipes)
        {
            return new PipeConnection();
        }

        return new WebSocketConnection();
    }

    private static string GetShellPath()
    {
        // Try common shell locations
        string[] shellPaths = ["/bin/bash", "/usr/bin/bash", "/bin/sh", "/usr/bin/sh"];

        foreach (string path in shellPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Fall back to bash and hope it's in PATH
        return "bash";
    }

    private static string EscapeShellArgument(string argument)
    {
        // Escape single quotes by ending the single-quoted string,
        // adding an escaped single quote, and starting a new single-quoted string
        return "'" + argument.Replace("'", "'\\''") + "'";
    }

    private static string EscapeWindowsArgument(string argument)
    {
        // If the argument contains spaces or special characters, wrap in quotes
        if (argument.Contains(' ') || argument.Contains('"'))
        {
            // Escape internal quotes and wrap in quotes
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
        }

        return argument;
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

    private ProcessStartInfo CreateProcessStartInfo()
    {
        string fileName = this.BrowserExecutableLocation;
        string args = string.Join(" ", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this.CommandLineArguments.Select(EscapeWindowsArgument) : this.CommandLineArguments);
        if (this.connection is not null && this.connection.ConnectionType == ConnectionType.Pipes && this.connection is PipeConnection pipeConnection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                List<string> browserArgs = [.. this.CommandLineArguments];
                browserArgs.Add($"--remote-debugging-io-pipes={pipeConnection.ReadPipeHandle},{pipeConnection.WritePipeHandle}");
                args = string.Join(" ", browserArgs.Select(EscapeWindowsArgument));
            }
            else
            {
                // Escape the browser path and arguments for shell usage
                string escapedBrowserPath = EscapeShellArgument(this.BrowserExecutableLocation);
                string escapedArgs = string.Join(" ", this.CommandLineArguments.Select(EscapeShellArgument));

                // For pipe connection in non-Windows OSes, create a bash command that:
                // 1. Redirects FD 3 to read from our write pipe (browser reads commands)
                // 2. Redirects FD 4 to write to our read pipe (browser writes responses)
                // 3. Closes the original pipe FDs to avoid leaking them
                // 4. Executes the browser with exec to replace the shell process
                //
                // The syntax is:
                //   exec 3<&{fd} 4>&{fd} {fd}<&- {fd}>&- ; exec browser args
                //
                // Where:
                //   3<&{fd}  - duplicate read pipe fd to fd 3
                //   4>&{fd}  - duplicate write pipe fd to fd 4
                //   {fd}<&-  - close the original read pipe fd
                //   {fd}>&-  - close the original write pipe fd
                string bashScript = $"exec 3<&{pipeConnection.ReadPipeHandle} 4>&{pipeConnection.WritePipeHandle} " +
                                $"{pipeConnection.ReadPipeHandle}<&- {pipeConnection.WritePipeHandle}>&-; " +
                                $"exec {escapedBrowserPath} {escapedArgs}";
                fileName = GetShellPath();
                args = $"-c \"{bashScript.Replace("\"", "\\\"")}\"";
            }
        }

        return new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
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

            if (this.browserProcess is not null && this.connection is not null && this.connection.ConnectionType == ConnectionType.Pipes && this.connection is PipeConnection pipeConnection)
            {
                pipeConnection.SetExternalProcess(this.browserProcess);
                this.WebSocketUrl = $"pipe://{this.BrowserExecutableLocation}:{this.browserProcess.Id}";
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
