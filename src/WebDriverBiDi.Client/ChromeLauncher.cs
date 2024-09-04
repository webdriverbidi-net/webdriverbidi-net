// <copyright file="ChromeLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// A browser launcher that does not rely on any external executable except for the browser itself.
/// </summary>
public class ChromeLauncher : BrowserLauncher
{
    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly ObservableEvent<LogMessageEventArgs> onLogMessageEvent = new();

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
            await this.InitializeBiDi().ConfigureAwait(false);
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

    private async Task CloseBrowser()
    {
        // Fire and forget the close browser command.
        Connection connection = new();
        await connection.StartAsync(this.WebSocketUrl);
        await connection.SendDataAsync(@"{ ""id"": 0, ""method"": ""Browser.close"", ""params"": {} }");
        await connection.StopAsync();
    }

    private void CreateUserDataDirectory()
    {
        string tempPath = Path.GetTempPath();
        string directoryName = Path.Combine(tempPath, $"webdriverbidi-net-chrome-data-{Guid.NewGuid()}");
        DirectoryInfo info = Directory.CreateDirectory(directoryName);
        this.userDataDirectory = info.FullName;
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
            List<string> linuxLocations = new()
            {
                "/usr/local/sbin",
                "/usr/local/bin",
                "/usr/sbin",
                "/usr/bin",
                "/sbin",
                "/bin",
                "/opt/google/chrome",
            };
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

    private async Task InitializeBiDi()
    {
        Connection connection = new();
        ManualResetEventSlim syncEvent = new(false);
        JsonDocument? document = null;
        string targetId = string.Empty;
        EventObserver<ConnectionDataReceivedEventArgs> observer = connection.OnDataReceived.AddObserver((e) =>
        {
            document = JsonDocument.Parse(e.Data);
            Console.WriteLine(e.Data);
            if (!document.RootElement.TryGetProperty("id", out _))
            {
                // Only return data from command responses; ignore events.
                return;
            }

            syncEvent.Set();
        });

        // First, get the list of available targets, capturing its ID.
        // This will become the tab that hosts the BiDi-to-CDP mapper.
        // Note: There should be only one blank page target at this point.
        long commandId = 0;
        DevToolsProtocolCommand command = new(commandId, "Target.getTargets");
        await connection.StartAsync(this.WebSocketUrl).ConfigureAwait(false);
        await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
        syncEvent.Wait(TimeSpan.FromSeconds(3));
        syncEvent.Reset();
        commandId++;
        if (document is not null)
        {
            targetId = document.RootElement.GetProperty("result").GetProperty("targetInfos")[0].GetProperty("targetId").GetString()!;
            Console.WriteLine($"Found target id {targetId}");
        }

        if (!string.IsNullOrEmpty(targetId))
        {
            // Attach to the target, and capture the session ID.
            command = new DevToolsProtocolCommand(commandId, "Target.attachToTarget");
            command.Parameters["targetId"] = targetId;
            command.Parameters["flatten"] = true;
            await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();
            commandId++;

            JsonElement result = document!.RootElement.GetProperty("result");
            string sessionId = result.GetProperty("sessionId").GetString()!;

            // Send a click event to the target so that the beforeunload event
            // will not be fired upon close.
            command = new DevToolsProtocolCommand(commandId, "Runtime.evaluate");
            command.Parameters["expression"] = "document.body.click()";
            command.Parameters["userGesture"] = true;
            await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();
            commandId++;

            // Enable the Runtime CDP domain.
            command = new DevToolsProtocolCommand(commandId, "Runtime.enable");
            command.SessionId = sessionId;
            await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();
            commandId++;

            // Expose CDP for the mapper tab.
            command = new DevToolsProtocolCommand(commandId, "Target.exposeDevToolsProtocol");
            command.Parameters["bindingName"] = "cdp";
            command.Parameters["targetId"] = targetId;
            await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();
            commandId++;

            // Load the mapper tab source code from the resources of this assembly.
            // This source code can be generated by building the chromium-bidi project
            // (https://github.com/GoogleChromeLabs/chromium-bidi). It is stored for
            // convenience in this project in the third_party directory.
            string mapperScript = string.Empty;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream("chromium-bidi-mapper"))
            {
                using StreamReader reader = new(resourceStream);
                mapperScript = reader.ReadToEnd();
            }

            if (!string.IsNullOrEmpty(mapperScript))
            {
                // Load the source code for the mapper tab into the target tab.
                command = new DevToolsProtocolCommand(commandId, "Runtime.evaluate");
                command.Parameters["expression"] = mapperScript;
                command.SessionId = sessionId;
                await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
                syncEvent.Wait(TimeSpan.FromSeconds(3));
                syncEvent.Reset();
                commandId++;

                // Start the BiDi-to-CDP mapper code.
                command = new DevToolsProtocolCommand(commandId, "Runtime.evaluate");
                command.Parameters["expression"] = @$"window.runMapperInstance(""{targetId}"")";
                command.Parameters["awaitPromise"] = true;
                command.SessionId = sessionId;
                await connection.SendDataAsync(JsonSerializer.Serialize(command)).ConfigureAwait(false);
                syncEvent.Wait(TimeSpan.FromSeconds(3));
                syncEvent.Reset();
                commandId++;
            }
        }

        observer.Unobserve();
        await connection.StopAsync();
    }

    private class DevToolsProtocolCommand
    {
        private readonly long id;
        private readonly string method;
        private Dictionary<string, object> parameters = new();
        private string? sessionId = null;

        public DevToolsProtocolCommand(long id, string method)
        {
            this.id = id;
            this.method = method;
        }

        [JsonPropertyName("id")]
        public long Id => this.id;

        [JsonPropertyName("method")]
        public string Method => this.method;

        [JsonPropertyName("params")]
        public Dictionary<string, object> Parameters => this.parameters;

        [JsonPropertyName("sessionId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SessionId { get => this.sessionId; set => this.sessionId = value; }
    }
}
