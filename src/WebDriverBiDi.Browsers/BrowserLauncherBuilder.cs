// <copyright file="BrowserLauncherBuilder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;

/// <summary>
/// Provides a fluent API for configuring and creating a <see cref="BrowserLauncher"/> instance.
/// </summary>
public class BrowserLauncherBuilder
{
    private const string ProxyCapabilityName = "proxy";
    private const string UserPromptHandlerCapabilityName = "unhandledPromptBehavior";

    // The capabilities the core library's capability request types, which take a value only of their type.
    private static readonly Dictionary<string, Type> TypedCapabilities = new()
    {
        ["acceptInsecureCerts"] = typeof(bool),
        ["browserName"] = typeof(string),
        ["browserVersion"] = typeof(string),
        ["platformName"] = typeof(string),
        [ProxyCapabilityName] = typeof(ProxyConfiguration),
        [UserPromptHandlerCapabilityName] = typeof(UserPromptHandler),
    };

    private readonly BrowserKind browser;
    private readonly LaunchSettings launchSettings = new();
    private readonly Dictionary<string, object?> sessionCapabilities = [];
    private BrowserReleaseChannel channel = BrowserReleaseChannel.Stable;
    private BrowserVersion version = BrowserVersion.Latest;
    private FileLocationBehavior locationBehavior = FileLocationBehavior.AutoLocateAndDownload;
    private string? customBrowserLocation = null;
    private BrowserDownloadOptions? downloadOptions = null;
    private LaunchStrategy launchStrategy = LaunchStrategy.Direct;
    private ConnectionKind connectionType = ConnectionKind.WebSocket;
    private int port = 0;
    private bool headless = false;
    private Uri? remoteUrl = null;
    private RemoteGridOptions? remoteGridOptions = null;
    private TimeSpan? launchTimeout = null;
    private BrowserLaunchOptions? browserOptions = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherBuilder"/> class for the specified browser.
    /// </summary>
    /// <param name="browser">The browser to launch.</param>
    internal BrowserLauncherBuilder(BrowserKind browser)
    {
        this.browser = browser;
    }

    private bool UseHeadlessShell => this.browserOptions is ChromeLaunchOptions { UseHeadlessShell: true };

    private bool IsRemote => this.launchStrategy == LaunchStrategy.UsingRemoteGrid || this.launchStrategy == LaunchStrategy.ConnectToExisting;

    /// <summary>
    /// Specifies the release channel to use for the browser (e.g., Stable, Beta, Alpha).
    /// </summary>
    /// <param name="channel">The release channel.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public BrowserLauncherBuilder WithReleaseChannel(BrowserReleaseChannel channel)
    {
        this.channel = channel;
        return this;
    }

    /// <summary>
    /// Specifies the browser version to use.
    /// </summary>
    /// <param name="version">The browser version specification.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when version is null.</exception>
    public BrowserLauncherBuilder WithVersion(BrowserVersion version)
    {
        this.version = version ?? throw new ArgumentNullException(nameof(version));
        return this;
    }

    /// <summary>
    /// Specifies the port on which the browser should listen for connections. Specifying zero
    /// indicates to use a random available port, which is equivalent to calling <see cref="WithRandomPort"/>.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when port is less than 0 or greater than 65535.</exception>
    public BrowserLauncherBuilder WithPort(int port)
    {
        if (port < 0 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 0 and 65535.");
        }

        this.port = port;
        return this;
    }

    /// <summary>
    /// Specifies to use a random available port for the browser connection.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public BrowserLauncherBuilder WithRandomPort()
    {
        this.port = 0;
        return this;
    }

    /// <summary>
    /// Specifies to launch the browser in headless (invisible) mode.
    /// </summary>
    /// <param name="headless">
    /// Use <see langword="true"/> to enable headless mode; <see langword="false"/> to disable.
    /// If omitted, defaults to <see langword="true"/>.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    public BrowserLauncherBuilder WithHeadlessOption(bool headless = true)
    {
        this.headless = headless;
        return this;
    }

    /// <summary>
    /// Specifies the type of connection to use for communicating with the browser.
    /// Note that only Chromium-based browser support pipe connections, and only
    /// when launched directly (i.e., not via a driver executable or remote grid).
    /// The default is WebSocket connections.
    /// </summary>
    /// <param name="connectionType">The connection type (WebSocket or Pipes).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public BrowserLauncherBuilder WithConnection(ConnectionKind connectionType)
    {
        this.connectionType = connectionType;
        return this;
    }

    /// <summary>
    /// Specifies to use the system-installed browser at its default installation location.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting location behavior has already been specified.</exception>
    public BrowserLauncherBuilder AtDefaultInstallationLocation()
    {
        this.ValidateLocationBehaviorNotSet(FileLocationBehavior.UseSystemInstallLocation);
        this.locationBehavior = FileLocationBehavior.UseSystemInstallLocation;
        this.version = BrowserVersion.SystemInstalled;
        return this;
    }

    /// <summary>
    /// Specifies the custom location of the browser executable.
    /// </summary>
    /// <param name="path">The full path to the browser executable.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting location behavior has already been specified.</exception>
    public BrowserLauncherBuilder AtLocation(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Browser location path cannot be null or empty.", nameof(path));
        }

        this.ValidateLocationBehaviorNotSet(FileLocationBehavior.UseCustomLocation);
        this.locationBehavior = FileLocationBehavior.UseCustomLocation;
        this.customBrowserLocation = path;
        return this;
    }

    /// <summary>
    /// Specifies to automatically locate the browser, downloading it if necessary to a cache directory.
    /// Use <see cref="WithDownloadOptions"/> to change the cache directory or download sources.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting location behavior has already been specified.</exception>
    public BrowserLauncherBuilder AtAutomaticallyDownloadedLocation()
    {
        this.ValidateLocationBehaviorNotSet(FileLocationBehavior.AutoLocateAndDownload);
        this.locationBehavior = FileLocationBehavior.AutoLocateAndDownload;
        return this;
    }

    /// <summary>
    /// Specifies the options controlling where the browser and driver are cached and downloaded from.
    /// </summary>
    /// <param name="options">The download options.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public BrowserLauncherBuilder WithDownloadOptions(BrowserDownloadOptions options)
    {
        this.downloadOptions = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Adds arguments to the browser's command line, after the launcher's own. When launching through
    /// a driver, they are passed in the driver's browser options capability.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when arguments or any of its elements is null.</exception>
    public BrowserLauncherBuilder WithArguments(params string[] arguments)
    {
        this.launchSettings.Arguments.AddRange(ValidateArguments(arguments, nameof(arguments)));
        return this;
    }

    /// <summary>
    /// Omits default arguments the launcher adds to the browser's command line. Arguments the launch
    /// depends on, such as the profile directory and the remote debugging port, are not defaults.
    /// </summary>
    /// <param name="arguments">
    /// The default arguments to omit, each matching an argument either exactly or by name, so that
    /// "--disable-features" omits "--disable-features=...". If none are given, every default argument is omitted.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when arguments or any of its elements is null.</exception>
    public BrowserLauncherBuilder WithoutDefaultArguments(params string[] arguments)
    {
        string[] omitted = ValidateArguments(arguments, nameof(arguments));
        if (omitted.Length == 0)
        {
            this.launchSettings.OmitAllDefaultArguments = true;
        }

        this.launchSettings.OmittedDefaultArguments.AddRange(omitted);
        return this;
    }

    /// <summary>
    /// Sets an environment variable for the launched browser, or, when launching through a driver,
    /// for the driver, from which the browser inherits it.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The value, or <see langword="null"/> to remove a variable the launched process would otherwise inherit.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains '='.</exception>
    public BrowserLauncherBuilder WithEnvironmentVariable(string name, string? value)
    {
        if (string.IsNullOrEmpty(name) || name.IndexOf('=') >= 0)
        {
            throw new ArgumentException("Environment variable name cannot be null or empty, or contain '='.", nameof(name));
        }

        this.launchSettings.EnvironmentVariables[name] = value;
        return this;
    }

    /// <summary>
    /// Specifies a profile directory owned by the caller, which is used in place of a temporary one and
    /// is never deleted. For Firefox, the automation preferences are written to the profile's user.js file.
    /// </summary>
    /// <param name="path">The profile directory, which is created if it does not exist.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
    public BrowserLauncherBuilder WithUserDataDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("User data directory cannot be null or empty.", nameof(path));
        }

        this.launchSettings.UserDataDirectory = path;
        return this;
    }

    /// <summary>
    /// Specifies settings that apply only to the browser being launched, such as
    /// <see cref="ChromeLaunchOptions"/> for Chrome or <see cref="FirefoxLaunchOptions"/> for Firefox.
    /// Specifying options again replaces them.
    /// </summary>
    /// <param name="options">The browser-specific options, which must be for the browser being launched.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public BrowserLauncherBuilder WithBrowserOptions(BrowserLaunchOptions options)
    {
        this.browserOptions = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Adds a capability to the request for the new session, such as <c>browserVersion</c>, <c>proxy</c>,
    /// <c>unhandledPromptBehavior</c>, or a grid vendor's options. Adding a capability again replaces its value.
    /// With <see cref="LaunchUsingDriver"/> and <see cref="LaunchUsingRemoteGrid(Uri, RemoteGridOptions?)"/>, the
    /// launcher sends them when it creates the session; otherwise, whoever sends the session.new command sends
    /// them, from <see cref="BrowserLauncher.CreateCapabilityRequest"/>.
    /// </summary>
    /// <param name="name">The capability name.</param>
    /// <param name="value">
    /// The value. For <c>proxy</c>, a <see cref="ProxyConfiguration"/>; for <c>unhandledPromptBehavior</c>, a
    /// <see cref="UserPromptHandler"/>; for <c>acceptInsecureCerts</c>, a <see cref="bool"/>; for
    /// <c>browserName</c>, <c>browserVersion</c>, and <c>platformName</c>, a <see cref="string"/>. Otherwise,
    /// <see langword="null"/>, a <see cref="string"/>, a <see cref="bool"/>, a number, a dictionary with string
    /// keys whose values follow these rules, or a sequence of such values. Any other value is rejected when the
    /// launcher is built, as is a capability the launcher sets itself.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public BrowserLauncherBuilder WithSessionCapability(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Capability name cannot be null or empty.", nameof(name));
        }

        this.sessionCapabilities[name] = value;
        return this;
    }

    /// <summary>
    /// Specifies how long to wait for the browser, or the driver, to become ready. Defaults to 20 seconds.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is not positive.</exception>
    public BrowserLauncherBuilder WithLaunchTimeout(TimeSpan timeout)
    {
        this.launchTimeout = timeout > TimeSpan.Zero ? timeout : throw new ArgumentOutOfRangeException(nameof(timeout), "Launch timeout must be positive.");
        return this;
    }

    /// <summary>
    /// Specifies to launch the browser via a WebDriver Classic driver executable (e.g., chromedriver, geckodriver).
    /// Uses default settings, automatically downloading the driver if needed.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting launch strategy has already been specified.</exception>
    public BrowserLauncherBuilder LaunchUsingDriver()
    {
        this.ValidateLaunchStrategyNotSet(LaunchStrategy.UsingDriver);
        this.launchStrategy = LaunchStrategy.UsingDriver;
        return this;
    }

    /// <summary>
    /// Specifies to create the browser session on a remote WebDriver grid (e.g., Selenium Grid or a cloud service).
    /// </summary>
    /// <param name="gridUrl">
    /// The http or https URL of the grid, including any port and path prefix (e.g., "http://selenium-hub:4444"
    /// or "https://user:key@hub.example.com/wd/hub"). Credentials in the URL are sent as Basic authorization.
    /// </param>
    /// <param name="options">Headers for the requests sent to the grid, or <see langword="null"/> for none.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when gridUrl is not an absolute http or https URL.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting launch strategy has already been specified.</exception>
    public BrowserLauncherBuilder LaunchUsingRemoteGrid(Uri gridUrl, RemoteGridOptions? options = null)
    {
        if (gridUrl is null || !gridUrl.IsAbsoluteUri || (gridUrl.Scheme != Uri.UriSchemeHttp && gridUrl.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Remote grid URL must be an absolute http or https URL.", nameof(gridUrl));
        }

        this.ValidateLaunchStrategyNotSet(LaunchStrategy.UsingRemoteGrid);
        this.launchStrategy = LaunchStrategy.UsingRemoteGrid;
        this.remoteUrl = gridUrl;
        this.remoteGridOptions = options;
        return this;
    }

    /// <summary>
    /// Specifies to connect to a browser that is already running and listening on a WebSocket URL,
    /// rather than launching one. Launching attaches to the browser, and closing the
    /// <see cref="BrowserInstance"/> only detaches from it; the browser keeps running.
    /// </summary>
    /// <param name="webSocketUrl">
    /// The ws or wss URL of the browser: its WebDriver BiDi endpoint (e.g., "ws://127.0.0.1:9222/session"
    /// for Firefox), or, for Chrome, its DevTools endpoint (e.g., "ws://127.0.0.1:9222/devtools/browser/…"),
    /// which is reached through the WebDriver BiDi mapper.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when webSocketUrl is not an absolute ws or wss URL.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting launch strategy has already been specified.</exception>
    public BrowserLauncherBuilder ConnectToExisting(Uri webSocketUrl)
    {
        if (webSocketUrl is null || !webSocketUrl.IsAbsoluteUri || (webSocketUrl.Scheme != "ws" && webSocketUrl.Scheme != "wss"))
        {
            throw new ArgumentException("Browser URL must be an absolute ws or wss URL.", nameof(webSocketUrl));
        }

        this.ValidateLaunchStrategyNotSet(LaunchStrategy.ConnectToExisting);
        this.launchStrategy = LaunchStrategy.ConnectToExisting;
        this.remoteUrl = webSocketUrl;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="BrowserLauncher"/> instance.
    /// </summary>
    /// <returns>The configured browser launcher.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when the specified browser is not yet supported.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when the configuration is invalid.</exception>
    public BrowserLauncher Build()
    {
        this.ValidateConfiguration();
        Dictionary<string, object?> capabilities = this.PrepareSessionCapabilities();

        BrowserLauncher launcher = this.browser switch
        {
            BrowserKind.Chrome => this.CreateChromeLauncher(),
            BrowserKind.Firefox => this.CreateFirefoxLauncher(),
            BrowserKind.Safari => this.CreateSafariLauncher(),
            BrowserKind.Edge => this.CreateEdgeLauncher(),
            _ => throw new BrowserLauncherConfigurationException($"Unknown browser type: {this.browser}"),
        };

        launcher.SessionCapabilities = new Dictionary<string, object?>(this.sessionCapabilities);
        if (launcher is WebDriverClassicBrowserLauncher classicLauncher)
        {
            if (capabilities.Keys.FirstOrDefault(classicLauncher.LaunchCapabilityNames.Contains) is string launcherCapability)
            {
                throw new BrowserLauncherConfigurationException($"The {launcherCapability} capability cannot be added; it is {DescribeLaunchCapability(launcherCapability)}.");
            }

            classicLauncher.AdditionalCapabilities = capabilities;
        }

        // Taken from the browser options as they are now, then copied, so settings changed after Build()
        // do not reach an already-built launcher.
        this.launchSettings.FirefoxPreferences.Clear();
        if (this.browserOptions is FirefoxLaunchOptions firefoxLaunchOptions)
        {
            foreach (KeyValuePair<string, object> preference in firefoxLaunchOptions.Preferences)
            {
                this.launchSettings.FirefoxPreferences[preference.Key] = preference.Value;
            }
        }

        this.launchSettings.UseHeadlessShell = this.UseHeadlessShell;
        launcher.LaunchSettings = this.launchSettings.Copy();

        if (this.launchTimeout is TimeSpan timeout)
        {
            launcher.InitializationTimeout = timeout;
        }

        return launcher;
    }

    private static string DescribeLaunchCapability(string name)
    {
        return name switch
        {
            "browserName" => "set from the browser passed to BrowserLauncher.Configure",
            "webSocketUrl" => "always requested, because the launcher needs a WebDriver BiDi session",
            SafariLauncher.ExperimentalWebSocketUrlCapabilityName => "required for Safari to create a WebDriver BiDi session",
            _ => "built by the launcher from settings such as WithArguments, WithHeadlessOption, WithUserDataDirectory, WithReleaseChannel, and WithBrowserOptions",
        };
    }

    private static string[] ValidateArguments(string[] arguments, string parameterName)
    {
        if (arguments is null || arguments.Any(argument => argument is null))
        {
            throw new ArgumentNullException(parameterName, "Arguments cannot be null.");
        }

        return arguments;
    }

    private static void ValidateFirefoxPreferences(FirefoxLaunchOptions options)
    {
        foreach (KeyValuePair<string, object> preference in options.Preferences)
        {
            if (string.IsNullOrWhiteSpace(preference.Key))
            {
                throw new BrowserLauncherConfigurationException("Firefox preference names cannot be null or empty.");
            }

            if (preference.Value is not (string or bool or int))
            {
                throw new BrowserLauncherConfigurationException($"Firefox preference '{preference.Key}' has a value of type {preference.Value?.GetType().Name ?? "null"}; preferences must be strings, Booleans, or integers.");
            }
        }
    }

    private void ValidateLocationBehaviorNotSet(FileLocationBehavior newBehavior)
    {
        if (this.locationBehavior != FileLocationBehavior.AutoLocateAndDownload && this.locationBehavior != newBehavior)
        {
            // Automatic download is the default, so it is never the conflicting earlier choice.
            string current = this.locationBehavior == FileLocationBehavior.UseSystemInstallLocation
                ? "use the system-installed browser"
                : $"use a custom browser location ({this.customBrowserLocation})";

            string requested = newBehavior switch
            {
                FileLocationBehavior.UseSystemInstallLocation => "use the system-installed browser",
                FileLocationBehavior.UseCustomLocation => "use a custom browser location",
                _ => "auto-download the browser",
            };

            throw new BrowserLauncherConfigurationException($"Cannot specify to {requested}; you already specified to {current}.");
        }
    }

    private void ValidateLaunchStrategyNotSet(LaunchStrategy newStrategy)
    {
        if (this.launchStrategy != LaunchStrategy.Direct && this.launchStrategy != newStrategy)
        {
            // Direct launch is the default, so it is neither requested nor the conflicting earlier choice.
            string current = this.launchStrategy switch
            {
                LaunchStrategy.UsingDriver => "launch via driver executable",
                LaunchStrategy.UsingRemoteGrid => $"connect to remote grid ({this.remoteUrl})",
                _ => $"connect to an existing browser ({this.remoteUrl})",
            };

            string requested = newStrategy switch
            {
                LaunchStrategy.UsingDriver => "launch via driver executable",
                LaunchStrategy.UsingRemoteGrid => "connect to remote grid",
                _ => "connect to an existing browser",
            };

            throw new BrowserLauncherConfigurationException($"Cannot specify to {requested}; you already specified to {current}.");
        }
    }

    // Copied, so capabilities changed after Build() do not reach an already-built launcher. The copy is shallow:
    // a nested value such as an options dictionary is still shared. A proxy is serialized now, so it is a snapshot.
    private Dictionary<string, object?> PrepareSessionCapabilities()
    {
        Dictionary<string, object?> capabilities = [];
        foreach (KeyValuePair<string, object?> capability in this.sessionCapabilities)
        {
            if (TypedCapabilities.TryGetValue(capability.Key, out Type? type))
            {
                if (capability.Value is not null && !type.IsInstanceOfType(capability.Value))
                {
                    throw new BrowserLauncherConfigurationException($"The {capability.Key} capability must be a {type.Name}.");
                }

                capabilities[capability.Key] = capability.Value switch
                {
                    ProxyConfiguration proxy => CapabilityWriter.SerializeProxy(proxy),
                    UserPromptHandler handler => CapabilityWriter.SerializeUserPromptHandler(handler),
                    _ => capability.Value,
                };
                continue;
            }

            if (capability.Value is ProxyConfiguration or UserPromptHandler)
            {
                string ownCapability = capability.Value is ProxyConfiguration ? ProxyCapabilityName : UserPromptHandlerCapabilityName;
                throw new BrowserLauncherConfigurationException($"A {capability.Value.GetType().Name} can only be the value of the {ownCapability} capability, not of {capability.Key}.");
            }

            if (CapabilityWriter.FindUnsupportedValue(capability.Value, capability.Key) is string unsupported)
            {
                throw new BrowserLauncherConfigurationException($"Capability {unsupported}.");
            }

            capabilities[capability.Key] = capability.Value;
        }

        return capabilities;
    }

    private void ValidateConfiguration()
    {
        if (this.IsRemote)
        {
            this.ValidateRemoteConfiguration();
        }

        // Validate pipe connection requirements
        if (this.connectionType == ConnectionKind.Pipes)
        {
            if (this.browser != BrowserKind.Chrome && this.browser != BrowserKind.Edge)
            {
                throw new BrowserLauncherConfigurationException($"Pipe connections are only supported for Chrome and Edge, not {this.browser}.");
            }

            if (this.launchStrategy != LaunchStrategy.Direct)
            {
                throw new BrowserLauncherConfigurationException("Pipe connections are only supported with direct launch strategy.");
            }
        }

        if (this.browser == BrowserKind.Safari && !this.IsRemote)
        {
            if (this.launchStrategy == LaunchStrategy.Direct)
            {
                throw new BrowserLauncherConfigurationException("Safari must be launched using a driver; you must use the .LaunchUsingDriver() method with the builder.");
            }

            if (this.locationBehavior != FileLocationBehavior.UseSystemInstallLocation)
            {
                throw new BrowserLauncherConfigurationException("Safari cannot be launched from a custom location; you must use the .AtDefaultInstallationLocation() method.");
            }

            if (this.launchSettings.ChangesBrowserConfiguration)
            {
                throw new BrowserLauncherConfigurationException("Safari cannot be launched with arguments, omitted default arguments, or a user data directory.");
            }
        }

        if (this.browserOptions is not null && this.browserOptions.Browser != this.browser)
        {
            throw new BrowserLauncherConfigurationException($"{this.browserOptions.GetType().Name} cannot be used to launch {this.browser}.");
        }

        if (this.browserOptions is FirefoxLaunchOptions firefoxOptions)
        {
            ValidateFirefoxPreferences(firefoxOptions);
        }

        if (this.UseHeadlessShell && this.locationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            throw new BrowserLauncherConfigurationException("chrome-headless-shell is available only from Chrome for Testing; it cannot be used with .AtDefaultInstallationLocation().");
        }

        if (this.browser == BrowserKind.Edge && !this.IsRemote && this.version != BrowserVersion.Latest)
        {
            throw new BrowserLauncherConfigurationException("Edge is never downloaded, so a version cannot be requested; the installed Edge of the release channel is used.");
        }

        if (this.version.IsMilestone && this.browser != BrowserKind.Chrome)
        {
            throw new BrowserLauncherConfigurationException($"Only Chrome versions can be requested by milestone, not {this.browser} versions.");
        }

        if (this.browser == BrowserKind.Firefox && this.channel == BrowserReleaseChannel.Alpha && this.locationBehavior == FileLocationBehavior.AutoLocateAndDownload && this.version != BrowserVersion.Latest)
        {
            throw new BrowserLauncherConfigurationException("Firefox Nightly builds are not archived by version, so only the latest can be downloaded.");
        }

        if (this.downloadOptions?.Platform is BrowserPlatform platform && platform != BrowserPlatform.Current)
        {
            throw new BrowserLauncherConfigurationException($"Cannot launch a browser for platform {platform} on platform {BrowserPlatform.Current}.");
        }
    }

    // A browser on a grid, or one already running, was not started by this library, so settings for starting one do not apply.
    private void ValidateRemoteConfiguration()
    {
        string method = this.launchStrategy == LaunchStrategy.UsingRemoteGrid ? "LaunchUsingRemoteGrid" : "ConnectToExisting";
        string remedy = this.launchStrategy == LaunchStrategy.UsingRemoteGrid
            ? "Use WithSessionCapability to configure the browser on the grid."
            : "Configure the browser when starting it.";
        if (this.locationBehavior != FileLocationBehavior.AutoLocateAndDownload || this.downloadOptions is not null)
        {
            throw new BrowserLauncherConfigurationException($"Cannot specify a browser location or download options with {method}; no browser is located or downloaded on this machine.");
        }

        if (this.launchSettings.ChangesBrowserConfiguration || this.launchSettings.EnvironmentVariables.Count > 0 || this.browserOptions is not null || this.headless)
        {
            throw new BrowserLauncherConfigurationException($"Cannot specify arguments, omitted default arguments, environment variables, a user data directory, browser options, or headless mode with {method}. {remedy}");
        }

        if (this.version != BrowserVersion.Latest || this.channel != BrowserReleaseChannel.Stable)
        {
            throw new BrowserLauncherConfigurationException($"Cannot specify a browser version or release channel with {method}. {remedy}");
        }

        if (this.port != 0)
        {
            throw new BrowserLauncherConfigurationException($"Cannot specify a port with {method}; the port is part of the URL.");
        }

        if (this.connectionType == ConnectionKind.Pipes)
        {
            throw new BrowserLauncherConfigurationException($"Pipe connections are not supported with {method}.");
        }
    }

    private BrowserLauncher CreateSafariLauncher()
    {
        if (this.launchStrategy == LaunchStrategy.ConnectToExisting)
        {
            return this.CreateExistingBrowserLauncher("safari");
        }

        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            // Safari enables BiDi only when this capability accompanies webSocketUrl. It is a default rather
            // than a launcher-owned capability, so a caller can still replace it once Safari no longer needs it.
            WebDriverClassicBrowserLauncher remoteLauncher = this.CreateRemoteLauncher("safari");
            remoteLauncher.DefaultCapabilities[SafariLauncher.ExperimentalWebSocketUrlCapabilityName] = true;
            return remoteLauncher;
        }

        SafariChannel safariChannel = this.channel switch
        {
            BrowserReleaseChannel.Stable => SafariChannel.Stable,
            BrowserReleaseChannel.DeveloperPreview => SafariChannel.TechnologyPreview,
            _ => throw new BrowserLauncherConfigurationException($"Invalid browser release channel for Safari: {this.channel}"),
        };

        SafariBrowserLocatorSettings settings = new(safariChannel);
        SafariLauncher safariLauncher = new(settings);
        return safariLauncher;
    }

    private BrowserLauncher CreateChromeLauncher()
    {
        if (this.launchStrategy == LaunchStrategy.ConnectToExisting)
        {
            return this.CreateExistingBrowserLauncher("chrome");
        }

        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            return this.CreateRemoteLauncher("chrome");
        }

        ChromeChannel chromeChannel = this.channel switch
        {
            BrowserReleaseChannel.Stable => ChromeChannel.Stable,
            BrowserReleaseChannel.Beta => ChromeChannel.Beta,
            BrowserReleaseChannel.DeveloperPreview => ChromeChannel.Dev,
            BrowserReleaseChannel.Alpha => ChromeChannel.Canary,
            _ => throw new BrowserLauncherConfigurationException($"Invalid browser release channel for Chrome: {this.channel}"),
        };

        string versionString = this.version.Value;
        string browserLocation = this.customBrowserLocation ?? string.Empty;

        ChromeBrowserLocatorSettings settings = new(chromeChannel, this.locationBehavior, this.downloadOptions ?? new BrowserDownloadOptions(), browserLocation, versionString, this.UseHeadlessShell);

        if (this.launchStrategy == LaunchStrategy.UsingDriver)
        {
            settings.IncludeDriver = true;

            ChromeDriverLauncher launcher = new(settings)
            {
                IsBrowserHeadless = this.headless,
                Port = this.port,
            };

            return launcher;
        }

        ChromeLauncher chromeLauncher = new(settings)
        {
            IsBrowserHeadless = this.headless,
            Port = this.port,
            ConnectionType = this.connectionType,
        };

        return chromeLauncher;
    }

    private BrowserLauncher CreateFirefoxLauncher()
    {
        if (this.launchStrategy == LaunchStrategy.ConnectToExisting)
        {
            return this.CreateExistingBrowserLauncher("firefox");
        }

        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            return this.CreateRemoteLauncher("firefox");
        }

        FirefoxChannel firefoxChannel = this.channel switch
        {
            BrowserReleaseChannel.Stable => FirefoxChannel.Stable,
            BrowserReleaseChannel.Beta => FirefoxChannel.Beta,
            BrowserReleaseChannel.DeveloperPreview => FirefoxChannel.Dev,
            BrowserReleaseChannel.Alpha => FirefoxChannel.Nightly,
            BrowserReleaseChannel.ExtendedSupport => FirefoxChannel.Esr,
            _ => throw new BrowserLauncherConfigurationException($"Invalid browser release channel for Firefox: {this.channel}"),
        };

        string versionString = this.version.Value;
        string browserLocation = this.customBrowserLocation ?? string.Empty;

        FirefoxBrowserLocatorSettings settings = new(firefoxChannel, this.locationBehavior, this.downloadOptions ?? new BrowserDownloadOptions(), browserLocation, versionString);

        if (this.launchStrategy == LaunchStrategy.UsingDriver)
        {
            settings.IncludeDriver = true;

            GeckoDriverLauncher launcher = new(settings)
            {
                IsBrowserHeadless = this.headless,
                Port = this.port,
            };

            return launcher;
        }

        FirefoxLauncher firefoxLauncher = new(settings)
        {
            IsBrowserHeadless = this.headless,
            Port = this.port,
        };

        return firefoxLauncher;
    }

    private BrowserLauncher CreateEdgeLauncher()
    {
        if (this.launchStrategy == LaunchStrategy.ConnectToExisting)
        {
            return this.CreateExistingBrowserLauncher("MicrosoftEdge");
        }

        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            return this.CreateRemoteLauncher("MicrosoftEdge");
        }

        EdgeChannel edgeChannel = this.channel switch
        {
            BrowserReleaseChannel.Stable => EdgeChannel.Stable,
            BrowserReleaseChannel.Beta => EdgeChannel.Beta,
            BrowserReleaseChannel.DeveloperPreview => EdgeChannel.Dev,
            BrowserReleaseChannel.Alpha => EdgeChannel.Canary,
            _ => throw new BrowserLauncherConfigurationException($"Invalid browser release channel for Edge: {this.channel}"),
        };

        // Edge is never downloaded, so the default location is the installed Edge of the channel.
        string? customPath = this.locationBehavior == FileLocationBehavior.UseCustomLocation ? this.customBrowserLocation : null;
        EdgeBrowserLocatorSettings settings = new(edgeChannel, this.downloadOptions ?? new BrowserDownloadOptions(), customPath);
        if (this.launchStrategy == LaunchStrategy.UsingDriver)
        {
            settings.IncludeDriver = true;
            return new EdgeDriverLauncher(settings)
            {
                IsBrowserHeadless = this.headless,
                Port = this.port,
            };
        }

        return new EdgeLauncher(settings)
        {
            IsBrowserHeadless = this.headless,
            Port = this.port,
            ConnectionType = this.connectionType,
        };
    }

    private WebDriverClassicBrowserLauncher CreateRemoteLauncher(string browserName)
    {
        Uri gridUrl = this.remoteUrl!;
        WebDriverClassicBrowserLauncher launcher = new(new RemoteBrowserLocatorSettings(browserName, gridUrl))
        {
            RemoteEndUrl = gridUrl,
        };

        if (this.remoteGridOptions is not null)
        {
            foreach (KeyValuePair<string, string> header in this.remoteGridOptions.Headers)
            {
                if (!launcher.TryAddRequestHeader(header.Key, header.Value))
                {
                    throw new BrowserLauncherConfigurationException($"Remote grid header '{header.Key}' cannot be sent as a request header.");
                }
            }
        }

        return launcher;
    }

    private ExistingBrowserLauncher CreateExistingBrowserLauncher(string browserName)
    {
        Uri webSocketUrl = this.remoteUrl!;
        return new ExistingBrowserLauncher(new RemoteBrowserLocatorSettings(browserName, webSocketUrl), webSocketUrl);
    }
}
