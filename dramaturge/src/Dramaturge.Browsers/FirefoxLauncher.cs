// <copyright file="FirefoxLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Object for launching a Firefox browser to connect to using a WebDriverBiDi session.
/// This browser launcher does not rely on any external executable except for the browser itself.
/// </summary>
public class FirefoxLauncher : BrowserLauncher
{
    // Firefox announces that its WebDriver BiDi endpoint is accepting connections with a
    // line of the form "WebDriver BiDi listening on ws://127.0.0.1:9222" on its standard
    // error stream, naming the port it chose if it was given port 0. This differs from the
    // "DevTools listening on ..." line matched by the base class implementation, which
    // Firefox never emits.
    private static readonly Regex BiDiEndpointReadyMatcher = new(@"WebDriver BiDi listening on ws:\/\/[^\s\/]+:(\d+)", RegexOptions.IgnoreCase);

    private readonly List<string> firefoxArguments = [
      "--no-remote",
    ];

    private readonly ProcessOutputTail outputTail = new();

    private Process? browserProcess;
    private TemporaryProfile? profile;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int reportedPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The <see cref="FirefoxBrowserLocatorSettings"/> settings to use for locating the Firefox browser executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    internal FirefoxLauncher(FirefoxBrowserLocatorSettings browserLocatorSettings, int port = 0)
        : base(browserLocatorSettings, port)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the service is running.
    /// </summary>
    public override bool IsRunning => this.browserProcess is not null && !this.browserProcess.HasExited;

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("firefoxLauncher.logMessage");

    [ExcludeFromCodeCoverage] // Takes only the branch for the operating system it runs on.
    private static IEnumerable<string> PlatformDefaultArguments
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                yield return "--foreground";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return "--wait-for-browser";
            }
        }
    }

    private IList<string> CommandLineArguments
    {
        get
        {
            List<string> defaultArguments = [.. this.firefoxArguments, .. PlatformDefaultArguments];
            List<string> args = [.. this.LaunchSettings.FilterDefaultArguments(defaultArguments)];
            args.Add("--profile");
            args.Add(this.ProfileDirectory);
            args.Add($"--remote-debugging-port");
            args.Add($"{this.Port}");
            if (this.IsBrowserHeadless)
            {
                args.Add("--headless");
            }

            args.AddRange(this.LaunchSettings.Arguments);
            return args.AsReadOnly();
        }
    }

    // The temporary profile is created before the profile directory is first needed.
    private string ProfileDirectory => this.LaunchSettings.UserDataDirectory ?? this.profile!.Path;

    // The port Firefox reported its WebDriver BiDi endpoint listening on, or 0 before it has.
    // Set from the browser process's output handler, which runs on a thread pool thread, and
    // read by the initialization wait loop, which may resume on a different one. Reads and
    // writes go through Interlocked so both threads are guaranteed to agree on the value.
    private int ReportedPort
    {
        get => Interlocked.CompareExchange(ref this.reportedPort, 0, 0);
        set => Interlocked.Exchange(ref this.reportedPort, value);
    }

    /// <summary>
    /// Asynchronously starts the browser launcher if it is not already running.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();

        // No operation required to start the launcher.
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
        if (this.browserProcess is not null)
        {
            throw new InvalidOperationException("A browser launched by this launcher is still running; quit it before launching another.");
        }

        string browserExecutableLocation = await this.BrowserLocator.LocateBrowserAsync(cancellationToken).ConfigureAwait(false);
        await this.LogAsync($"Launching Firefox browser from {browserExecutableLocation}").ConfigureAwait(false);
        if (this.LaunchSettings.UserDataDirectory is null && ConfinedExecutable.IsConfined(browserExecutableLocation))
        {
            throw new BrowserLaunchException(
                $"Firefox at {browserExecutableLocation} runs in a Snap or Flatpak sandbox, which cannot read a profile in the temporary directory. " +
                "Use a downloaded Firefox, or specify a profile directory the sandbox can read with WithUserDataDirectory().");
        }

        // With port 0, Firefox chooses a free port itself and reports it when its endpoint is ready.
        this.ConnectionString = string.Empty;
        this.ReportedPort = 0;
        this.profile = this.LaunchSettings.UserDataDirectory is null ? TemporaryProfile.Create("firefox") : null;
        try
        {
            this.CreateProfile(this.ProfileDirectory, isTemporary: this.profile is not null);
            Process process = new()
            {
                StartInfo = this.CreateProcessStartInfo(browserExecutableLocation),
            };
            process.ErrorDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.OutputDataReceived += this.ReadConsoleOutputForWebSocketUrl;
            process.ErrorDataReceived += this.RecordProcessOutput;
            process.OutputDataReceived += this.RecordProcessOutput;
            this.outputTail.Clear();
            StartProcess(process, "Firefox");
            this.browserProcess = process;
            this.profile?.SetOwner(this.browserProcess);
            this.browserProcess.BeginOutputReadLine();
            this.browserProcess.BeginErrorReadLine();
            bool launcherAvailable = await this.WaitForInitializationAsync(cancellationToken).ConfigureAwait(false);
            if (!launcherAvailable)
            {
                int? exitCode = null;
                string reason = $"Browser process did not report its WebDriver BiDi endpoint as ready within {this.InitializationTimeout.TotalSeconds} seconds.";
                if (!this.IsRunning)
                {
                    await ProcessTermination.WaitForOutputAsync(this.browserProcess).ConfigureAwait(false);
                    exitCode = this.browserProcess.ExitCode;
                    reason = $"Browser process exited with code {exitCode} before reporting its WebDriver BiDi endpoint as ready.";
                }

                throw new BrowserLaunchException($"Unable to launch Firefox browser. {reason}", exitCode, this.outputTail.ToList());
            }
        }
        catch (Exception)
        {
            // Whatever was started is killed, and the profile deleted, however the launch failed.
            await this.TerminateBrowserProcessAsync(requestExit: false, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        this.Port = this.ReportedPort;
        this.ConnectionString = $"ws://localhost:{this.Port}/session";

        return this.CreateBrowserInstance(this.ConnectionString, this.GetProcessId());
    }

    /// <summary>
    /// Asynchronously quits the browser: asks it to exit, waits up to <see cref="BrowserLauncher.ShutdownTimeout"/>,
    /// then kills it and every process it started, and deletes its temporary profile.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the browser to exit; the browser is then killed, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override Task QuitBrowserAsync(CancellationToken cancellationToken = default)
    {
        return this.TerminateBrowserProcessAsync(requestExit: true, cancellationToken);
    }

    /// <summary>
    /// Asynchronously forces the browser to terminate, for use when <see cref="QuitBrowserAsync"/> has failed.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the killed browser to exit, after which an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override Task KillBrowserAsync(CancellationToken cancellationToken = default)
    {
        return this.TerminateBrowserProcessAsync(requestExit: false, cancellationToken);
    }

    /// <summary>
    /// Asynchronously stops the browser launcher.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the launcher to stop.</param>
    /// <returns>A Task representing the result of the asynchronous operation.</returns>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        // No operation required to stop the launcher.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a Transport object that can be used to communicate with the browser.
    /// </summary>
    /// <returns>The <see cref="Transport"/> to be used in instantiating the driver.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Transport CreateTransport()
    {
        this.ThrowIfDisposed();
        return new Transport(this.CreateConnection());
    }

    /// <summary>
    /// Gets the process ID of the browser process just launched.
    /// </summary>
    /// <returns>The process ID.</returns>
    protected override int GetProcessId()
    {
        return this.browserProcess!.Id;
    }

    /// <summary>
    /// Provides a handler for reading the console output of the browser process, detecting
    /// the message emitted by Firefox once its WebDriver BiDi endpoint is accepting connections.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    /// <remarks>
    /// This records only the announced port. Firefox announces the endpoint's origin without a
    /// path, whereas creating a session requires the "/session" path, so <see cref="LaunchBrowserAsync"/>
    /// constructs the session URL from the port.
    /// </remarks>
    protected override void ReadConsoleOutputForWebSocketUrl(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            Match match = BiDiEndpointReadyMatcher.Match(e.Data);
            if (match.Success)
            {
                this.ReportedPort = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }
        }
    }

    private static Dictionary<string, object> GetPreferences(Dictionary<string, object> preferences)
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

            // Prevent various error messages on the console
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

            // DevTools JSONViewer sometimes fails to load dependencies with its require.js,
            // which spams the console (Bug 1424372)
            ["devtools.jsonview.enabled"] = false,

            // Disable popup-blocker
            ["dom.disable_open_during_load"] = false,

            // Enable the support for File object creation in the content process,
            // which setting the files of a file input requires
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

            // Prevent various error messages on the console
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

        foreach (KeyValuePair<string, object> kv in preferences)
        {
            prefs[kv.Key] = kv.Value;
        }

        return prefs;
    }

    private static string FormatJsValue(object value)
    {
        return value switch
        {
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            bool b => b ? "true" : "false",

            // Building the launcher rejects preferences that are not strings, Booleans, or integers.
            _ => ((int)value).ToString(CultureInfo.InvariantCulture),
        };
    }

    private ProcessStartInfo CreateProcessStartInfo(string browserExecutableLocation)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = browserExecutableLocation,
            Arguments = CommandLine.JoinArguments(this.CommandLineArguments),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        this.LaunchSettings.ApplyEnvironmentVariables(startInfo);
        return startInfo;
    }

    // Preferences go in user.js, which Firefox applies at every start. A temporary profile also gets
    // an empty prefs.js; a caller's profile keeps its own, so its other settings survive.
    private void CreateProfile(string profileDirectory, bool isTemporary)
    {
        Directory.CreateDirectory(profileDirectory);
        Dictionary<string, object> defaultPreferences = GetPreferences(this.LaunchSettings.FirefoxPreferences);
        List<string> preferenceList = [];
        foreach (KeyValuePair<string, object> preferencePair in defaultPreferences)
        {
            preferenceList.Add($"user_pref({FormatJsValue(preferencePair.Key)}, {FormatJsValue(preferencePair.Value)});");
        }

        File.WriteAllText(Path.Combine(profileDirectory, "user.js"), string.Join("\n", preferenceList));
        if (isTemporary)
        {
            File.WriteAllText(Path.Combine(profileDirectory, "prefs.js"), string.Empty);
        }
    }

    /// <summary>
    /// Asynchronously waits for the initialization of the browser launcher.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// Waiting for the browser process to merely be running is not sufficient: the process is
    /// running the instant it is started, but its WebDriver BiDi endpoint is not yet listening,
    /// and a connection attempt made in that window fails. Returning early leaves the race to
    /// be absorbed by the connection's startup retries, which is not always enough on a cold or
    /// heavily loaded machine.
    /// </remarks>
    private async Task<bool> WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        // A browser process that has exited ends the wait early.
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (initializationStopwatch.Elapsed <= this.InitializationTimeout && this.IsRunning)
        {
            if (this.ReportedPort != 0)
            {
                return true;
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private void RecordProcessOutput(object sender, DataReceivedEventArgs e)
    {
        this.outputTail.Add(e.Data);
    }

    private async Task TerminateBrowserProcessAsync(bool requestExit, CancellationToken cancellationToken)
    {
        bool isCancelled = false;
        Process? process = this.browserProcess;
        if (process is not null)
        {
            isCancelled = await ProcessTermination.StopAsync(process, requestExit, this.ShutdownTimeout, cancellationToken).ConfigureAwait(false);
            process.Dispose();
            this.browserProcess = null;
        }

        if (this.profile is not null)
        {
            await this.profile.DeleteAsync().ConfigureAwait(false);
            this.profile = null;
        }

        if (isCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }
}
