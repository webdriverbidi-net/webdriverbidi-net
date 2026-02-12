// <copyright file="FirefoxLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Object for launching a Firefox browser to connect to using a WebDriverBiDi session.
/// This browser launcher does not rely on any external executable except for the browser itself.
/// </summary>
public class FirefoxLauncher : BrowserLauncher
{
    private static readonly SemaphoreSlim LockObject = new(1, 1);
    private readonly ObservableEvent<LogMessageEventArgs> onLogMessageEvent = new("firefoxLauncher.logMessage");
    private string userDataDirectory = string.Empty;
    private Process? browserProcess;

    private List<string> firefoxArguments = [
      "--no-remote",
    ];

    private Dictionary<string, object> userPreferences = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxLauncher"/> class.
    /// </summary>
    public FirefoxLauncher()
        : this(string.Empty, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxLauncher"/> class.
    /// </summary>
    /// <param name="browserExecutableLocation">The path and executable name of the browser executable.</param>
    public FirefoxLauncher(string browserExecutableLocation)
        : this(browserExecutableLocation, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxLauncher"/> class.
    /// </summary>
    /// <param name="browserExecutableLocation">The path and executable name of the browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    public FirefoxLauncher(string browserExecutableLocation, int port)
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
            List<string> args = [.. this.firefoxArguments];
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                args.Add("--foreground");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                args.Add("--wait-for-browser");
            }

            args.Add("--profile");
            args.Add(this.userDataDirectory);
            args.Add($"--remote-debugging-port");
            args.Add($"{this.Port}");
            if (this.IsBrowserHeadless)
            {
                args.Add("--headless");
            }

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
            this.CreateProfile();

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

            this.WebSocketUrl = $"ws://localhost:{this.Port}/session";
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
        // TODO: Find a way to close the browser more gracefully.
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
        return new Transport();
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

    private static Dictionary<string, object> GetDefaultPreferences(Dictionary<string, object> preferences)
    {
        const string server = "dummy.test";
        Dictionary<string, object> prefs = new()
        {
            // Make sure Shield doesn't hit the network.
            ["app.normandy.api_url"] = string.Empty,

            // Disable Firefox old build background check
            ["app.update.checkInstallTime"] = false,

            // Disable automatically upgrading Firefox
            ["app.update.disabledForTesting"] = true,

            // Increase the APZ content response timeout to 1 minute
            ["apz.content_response_timeout"] = 60000,

            // Prevent various error message on the console
            // jest-puppeteer asserts that no error message is emitted by the console
            ["browser.contentblocking.features.standard"] = "-tp,tpPrivate,cookieBehavior0,-cm,-fp",

            // Enable the dump function: which sends messages to the system
            // console
            // https://bugzilla.mozilla.org/show_bug.cgi?id=1543115
            ["browser.dom.window.dump.enabled"] = true,

            // Disable topstories
            ["browser.newtabpage.activity-stream.feeds.system.topstories"] = false,

            // Always display a blank page
            ["browser.newtabpage.enabled"] = false,

            // Background thumbnails in particular cause grief: and disabling
            // thumbnails in general cannot hurt
            ["browser.pagethumbnails.capturing_disabled"] = true,

            // Disable safebrowsing components.
            ["browser.safebrowsing.blockedURIs.enabled"] = false,
            ["browser.safebrowsing.downloads.enabled"] = false,
            ["browser.safebrowsing.malware.enabled"] = false,
            ["browser.safebrowsing.passwords.enabled"] = false,
            ["browser.safebrowsing.phishing.enabled"] = false,

            // Disable updates to search engines.
            ["browser.search.update"] = false,

            // Do not restore the last open set of tabs if the browser has crashed
            ["browser.sessionstore.resume_from_crash"] = false,

            // Skip check for default browser on startup
            ["browser.shell.checkDefaultBrowser"] = false,

            // Disable newtabpage
            ["browser.startup.homepage"] = "about:blank",

            // Do not redirect user when a milstone upgrade of Firefox is detected
            ["browser.startup.homepage_override.mstone"] = "ignore",

            // Start with a blank page about:blank
            ["browser.startup.page"] = 0,

            // Do not allow background tabs to be zombified on Android: otherwise for
            // tests that open additional tabs: the test harness tab itself might get
            // unloaded
            ["browser.tabs.disableBackgroundZombification"] = false,

            // Do not warn when closing all other open tabs
            ["browser.tabs.warnOnCloseOtherTabs"] = false,

            // Do not warn when multiple tabs will be opened
            ["browser.tabs.warnOnOpen"] = false,

            // Disable the UI tour.
            ["browser.uitour.enabled"] = false,

            // Turn off search suggestions in the location bar so as not to trigger
            // network connections.
            ["browser.urlbar.suggest.searches"] = false,

            // Disable first run splash page on Windows 10
            ["browser.usedOnWindows10.introURL"] = string.Empty,

            // Do not warn on quitting Firefox
            ["browser.warnOnQuit"] = false,

            // Defensively disable data reporting systems
            ["datareporting.healthreport.documentServerURI"] = $"http://{server}/dummy/healthreport/",
            ["datareporting.healthreport.logging.consoleEnabled"] = false,
            ["datareporting.healthreport.service.enabled"] = false,
            ["datareporting.healthreport.service.firstRun"] = false,
            ["datareporting.healthreport.uploadEnabled"] = false,

            // Do not show datareporting policy notifications which can interfere with tests
            ["datareporting.policy.dataSubmissionEnabled"] = false,
            ["datareporting.policy.dataSubmissionPolicyBypassNotification"] = true,

            // DevTools JSONViewer sometimes fails to load dependencies with its require.js.
            // This doesn"t affect Puppeteer but spams console (Bug 1424372)
            ["devtools.jsonview.enabled"] = false,

            // Disable popup-blocker
            ["dom.disable_open_during_load"] = false,

            // Enable the support for File object creation in the content process
            // Required for |Page.setFileInputFiles| protocol method.
            ["dom.file.createInChild"] = true,

            // Disable the ProcessHangMonitor
            ["dom.ipc.reportProcessHangs"] = false,

            // Disable slow script dialogues
            ["dom.max_chrome_script_run_time"] = 0,
            ["dom.max_script_run_time"] = 0,

            // Only load extensions from the application and user profile
            // AddonManager.SCOPE_PROFILE + AddonManager.SCOPE_APPLICATION
            ["extensions.autoDisableScopes"] = 0,
            ["extensions.enabledScopes"] = 5,

            // Disable metadata caching for installed add-ons by default
            ["extensions.getAddons.cache.enabled"] = false,

            // Disable installing any distribution extensions or add-ons.
            ["extensions.installDistroAddons"] = false,

            // Disabled screenshots extension
            ["extensions.screenshots.disabled"] = true,

            // Turn off extension updates so they do not bother tests
            ["extensions.update.enabled"] = false,

            // Turn off extension updates so they do not bother tests
            ["extensions.update.notifyUser"] = false,

            // Make sure opening about:addons will not hit the network
            ["extensions.webservice.discoverURL"] = $"http://{server}/dummy/discoveryURL",

            // Temporarily force disable BFCache in parent (https://bit.ly/bug-1732263)
            ["fission.bfcacheInParent"] = false,

            // Force all web content to use a single content process
            ["fission.webContentIsolationStrategy"] = 0,

            // Allow the application to have focus even it runs in the background
            ["focusmanager.testmode"] = true,

            // Disable useragent updates
            ["general.useragent.updates.enabled"] = false,

            // Always use network provider for geolocation tests so we bypass the
            // macOS dialog raised by the corelocation provider
            ["geo.provider.testing"] = true,

            // Do not scan Wifi
            ["geo.wifi.scan"] = false,

            // No hang monitor
            ["hangmonitor.timeout"] = 0,

            // Show chrome errors and warnings in the error console
            ["javascript.options.showInConsole"] = true,

            // Disable download and usage of OpenH264: and Widevine plugins
            ["media.gmp-manager.updateEnabled"] = false,

            // Disable the GFX sanity window
            ["media.sanity-test.disabled"] = true,

            // Prevent various error message on the console
            // jest-puppeteer asserts that no error message is emitted by the console
            ["network.cookie.cookieBehavior"] = 0,

            // Disable experimental feature that is only available in Nightly
            ["network.cookie.sameSite.laxByDefault"] = false,

            // Do not prompt for temporary redirects
            ["network.http.prompt-temp-redirect"] = false,

            // Disable speculative connections so they are not reported as leaking
            // when they are hanging around
            ["network.http.speculative-parallel-limit"] = 0,

            // Do not automatically switch between offline and online
            ["network.manage-offline-status"] = false,

            // Make sure SNTP requests do not hit the network
            ["network.sntp.pools"] = server,

            // Disable Flash.
            ["plugin.state.flash"] = 0,
            ["privacy.trackingprotection.enabled"] = false,

            // Enable Remote Agent
            // https://bugzilla.mozilla.org/show_bug.cgi?id=1544393
            ["remote.enabled"] = true,

            // Don"t do network connections for mitm priming
            ["security.certerrors.mitm.priming.enabled"] = false,

            // Local documents have access to all other local documents,
            // including directory listings
            ["security.fileuri.strict_origin_policy"] = false,

            // Do not wait for the notification button security delay
            ["security.notification_enable_delay"] = 0,

            // Ensure blocklist updates do not hit the network
            ["services.settings.server"] = $"http://{server}/dummy/blocklist/",

            // Do not automatically fill sign-in forms with known usernames and
            // passwords
            ["signon.autofillForms"] = false,

            // Disable password capture, so that tests that include forms are not
            // influenced by the presence of the persistent doorhanger notification
            ["signon.rememberSignons"] = false,

            // Disable first-run welcome page
            ["startup.homepage_welcome_url"] = "about:blank",

            // Disable first-run welcome page
            ["startup.homepage_welcome_url.additional"] = string.Empty,

            // Disable browser animations (tabs, fullscreen, sliding alerts)
            ["toolkit.cosmeticAnimations.enabled"] = false,

            // Prevent starting into safe mode after application crashes
            ["toolkit.startup.max_resumed_crashes"] = -1,

            // Additional preferences always added.
            ["browser.tabs.closeWindowWithLastTab"] = false,
            ["network.cookie.cookieBehavior"] = 0,
            ["fission.bfcacheInParent"] = false,
            ["remote.active-protocols"] = 3,
            ["fission.webContentIsolationStrategy"] = 0,
        };

        if (preferences != null)
        {
            foreach (KeyValuePair<string, object> kv in preferences)
            {
                prefs[kv.Key] = kv.Value;
            }
        }

        return prefs;
    }

    private void CreateProfile()
    {
        // If the tempUserDataDirectory begins and ends with a quote, remove the quote
        if (this.userDataDirectory.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && this.userDataDirectory.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
        {
            this.userDataDirectory = this.userDataDirectory.Substring(1, this.userDataDirectory.Length - 2);
        }

        Dictionary<string, object> defaultPreferences = GetDefaultPreferences(this.userPreferences);
        List<string> preferenceList = [];
        foreach (KeyValuePair<string, object> preferencePair in defaultPreferences)
        {
            preferenceList.Add($"user_pref({FormatJsValue(preferencePair.Key)}, {FormatJsValue(preferencePair.Value)});");
        }

        File.WriteAllText(Path.Combine(this.userDataDirectory, "user.js"), string.Join("\n", preferenceList));
        File.WriteAllText(Path.Combine(this.userDataDirectory, "prefs.js"), string.Empty);
    }

    private static string FormatJsValue(object value)
    {
        return value switch
        {
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            bool b => b ? "true" : "false",
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentException($"Unsupported preference value type: {value.GetType().Name}", nameof(value)),
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
                await Task.Delay(100);
            }
            else
            {
                isInitialized = true;
                break;
            }
        }

        return isInitialized;
    }

    private void CreateUserDataDirectory()
    {
        string tempPath = Path.GetTempPath();
        string directoryName = Path.Combine(tempPath, $"webdriverbidi-net-firefox-data-{Guid.NewGuid()}");
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
        // NOTE: This is a naive algorithm for demonstration purposes only.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "/Applications/Firefox.app/Contents/MacOS/firefox";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "C:\\Program Files\\Mozilla Firefox\\firefox.exe";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "/usr/bin/firefox";
        }

        return string.Empty;
    }
}
