// <copyright file="BrowserLauncherBuilder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Protocol;

/// <summary>
/// Provides a fluent API for configuring and creating a <see cref="BrowserLauncher"/> instance.
/// </summary>
public class BrowserLauncherBuilder
{
    private readonly BrowserKind browser;
    private readonly LaunchSettings launchSettings = new();
    private BrowserReleaseChannel channel = BrowserReleaseChannel.Stable;
    private BrowserVersion version = BrowserVersion.Latest;
    private FileLocationBehavior locationBehavior = FileLocationBehavior.AutoLocateAndDownload;
    private string? customBrowserLocation = null;
    private BrowserDownloadOptions? downloadOptions = null;
    private LaunchStrategy launchStrategy = LaunchStrategy.Direct;
    private ConnectionKind connectionType = ConnectionKind.WebSocket;
    private int port = 0;
    private bool headless = false;
    private string? remoteGridHostName = null;
    private bool remoteGridUseSsl = false;
    private Dictionary<string, object>? capabilities = null;
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
    /// <see cref="FirefoxLaunchOptions"/> for Firefox. Specifying options again replaces them.
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
    /// Specifies to connect to a browser on a remote WebDriver grid (e.g., Selenium Grid, cloud service).
    /// Use <see cref="WithPort"/> to specify the port (default is 4444 for standard Selenium Grid).
    /// </summary>
    /// <param name="hostName">The hostname of the remote grid endpoint (e.g., "selenium-hub", "localhost").</param>
    /// <param name="useSsl">A value indicating whether to connect using HTTPS instead of HTTP. Use <see langword="true"/> to use HTTPS; <see langword="false"/> to use HTTP. If omitted, defaults to <see langword="false"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when hostName is null or empty.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when a conflicting launch strategy has already been specified.</exception>
    public BrowserLauncherBuilder LaunchUsingRemoteGrid(string hostName, bool useSsl = false)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            throw new ArgumentException("Remote grid hostname cannot be null or empty.", nameof(hostName));
        }

        this.ValidateLaunchStrategyNotSet(LaunchStrategy.UsingRemoteGrid);
        this.launchStrategy = LaunchStrategy.UsingRemoteGrid;
        this.remoteGridHostName = hostName;
        this.remoteGridUseSsl = useSsl;

        // Set default port for Selenium Grid if not already set
        if (this.port == 0)
        {
            this.port = 4444;
        }

        return this;
    }

    /// <summary>
    /// Adds a capability to be used when creating a remote session on a WebDriver grid.
    /// Only applicable when using <see cref="LaunchUsingRemoteGrid"/>.
    /// </summary>
    /// <param name="name">The capability name.</param>
    /// <param name="value">The capability value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <remarks>
    /// The <c>browserName</c> and <c>webSocketUrl</c> capabilities are set by the launcher and cannot be overridden.
    /// </remarks>
    public BrowserLauncherBuilder WithCapability(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Capability name cannot be null or empty.", nameof(name));
        }

        this.capabilities ??= new Dictionary<string, object>();
        this.capabilities[name] = value;
        return this;
    }

    /// <summary>
    /// Configures multiple capabilities to be used when creating a remote session on a WebDriver grid.
    /// Only applicable when using <see cref="LaunchUsingRemoteGrid"/>.
    /// </summary>
    /// <param name="configure">Action to configure the capabilities dictionary.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    /// <remarks>
    /// The <c>browserName</c> and <c>webSocketUrl</c> capabilities are set by the launcher and cannot be overridden.
    /// </remarks>
    public BrowserLauncherBuilder WithCapabilities(Action<Dictionary<string, object>> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        this.capabilities ??= new Dictionary<string, object>();
        configure(this.capabilities);
        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="BrowserLauncher"/> instance.
    /// </summary>
    /// <returns>The configured browser launcher.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified browser is not yet supported.</exception>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when the configuration is invalid.</exception>
    public BrowserLauncher Build()
    {
        this.ValidateConfiguration();

        BrowserLauncher launcher = this.browser switch
        {
            BrowserKind.Chrome => this.CreateChromeLauncher(),
            BrowserKind.Firefox => this.CreateFirefoxLauncher(),
            BrowserKind.Safari => this.CreateSafariLauncher(),
            BrowserKind.Edge => throw new NotImplementedException(
                "Microsoft Edge browser support is not yet implemented. Currently supported browsers: Chrome, Firefox. " +
                "Edge support is planned for a future release."),
            _ => throw new WebDriverBiDiException($"Unknown browser type: {this.browser}"),
        };

        // Copied, so settings added to this builder after Build() do not reach an already-built launcher.
        launcher.LaunchSettings = this.launchSettings.Copy();
        if (this.browserOptions is FirefoxLaunchOptions firefoxLaunchOptions)
        {
            foreach (KeyValuePair<string, object> preference in firefoxLaunchOptions.Preferences)
            {
                launcher.LaunchSettings.FirefoxPreferences[preference.Key] = preference.Value;
            }
        }

        if (this.launchTimeout is TimeSpan timeout)
        {
            launcher.InitializationTimeout = timeout;
        }

        return launcher;
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
            string current = this.locationBehavior switch
            {
                FileLocationBehavior.UseSystemInstallLocation => "use the system-installed browser",
                FileLocationBehavior.UseCustomLocation => $"use a custom browser location ({this.customBrowserLocation})",
                _ => "auto-download the browser",
            };

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
            string current = this.launchStrategy switch
            {
                LaunchStrategy.UsingDriver => "launch via driver executable",
                LaunchStrategy.UsingRemoteGrid => $"connect to remote grid ({this.remoteGridHostName})",
                _ => "launch directly",
            };

            string requested = newStrategy switch
            {
                LaunchStrategy.UsingDriver => "launch via driver executable",
                LaunchStrategy.UsingRemoteGrid => "connect to remote grid",
                _ => "launch directly",
            };

            throw new BrowserLauncherConfigurationException($"Cannot specify to {requested}; you already specified to {current}.");
        }
    }

    private void ValidateConfiguration()
    {
        // Validate remote grid specific requirements
        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            this.ValidateRemoteGridConfiguration();
        }

        // Validate pipe connection requirements
        if (this.connectionType == ConnectionKind.Pipes)
        {
            if (this.browser != BrowserKind.Chrome)
            {
                throw new BrowserLauncherConfigurationException($"Pipe connections are only supported for Chrome browser, not {this.browser}.");
            }

            if (this.launchStrategy != LaunchStrategy.Direct)
            {
                throw new BrowserLauncherConfigurationException("Pipe connections are only supported with direct launch strategy.");
            }
        }

        if (this.browser == BrowserKind.Safari)
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

        if (this.launchStrategy != LaunchStrategy.UsingRemoteGrid && this.downloadOptions?.Platform is BrowserPlatform platform && platform != BrowserPlatform.Current)
        {
            throw new BrowserLauncherConfigurationException($"Cannot launch a browser for platform {platform} on platform {BrowserPlatform.Current}.");
        }

        // Validate capabilities are only used with remote grid
        if (this.capabilities is not null && this.capabilities.Count > 0 && this.launchStrategy != LaunchStrategy.UsingRemoteGrid)
        {
            throw new BrowserLauncherConfigurationException("Capabilities can only be specified when using LaunchUsingRemoteGrid.");
        }
    }

    private void ValidateRemoteGridConfiguration()
    {
        if (string.IsNullOrWhiteSpace(this.remoteGridHostName))
        {
            throw new BrowserLauncherConfigurationException("Remote grid hostname must be specified when using LaunchUsingRemoteGrid.");
        }

        if (this.connectionType == ConnectionKind.Pipes)
        {
            throw new BrowserLauncherConfigurationException("Pipe connections are not supported with remote grid launch strategy.");
        }

        // Validate that local browser configuration options are not used with remote grid
        if (this.locationBehavior != FileLocationBehavior.AutoLocateAndDownload)
        {
            string locationMethod = this.locationBehavior switch
            {
                FileLocationBehavior.UseSystemInstallLocation => "AtDefaultInstallationLocation()",
                FileLocationBehavior.UseCustomLocation => "AtLocation()",
                _ => "a browser location method",
            };
            throw new BrowserLauncherConfigurationException(
                $"Cannot use {locationMethod} with LaunchUsingRemoteGrid. " +
                "The remote grid manages its own browser installations.");
        }

        if (this.customBrowserLocation is not null)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify custom browser location with LaunchUsingRemoteGrid. " +
                "The remote grid manages its own browser installations.");
        }

        if (this.downloadOptions is not null)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify download options with LaunchUsingRemoteGrid. " +
                "The remote grid manages its own browser installations.");
        }

        if (this.launchSettings.ChangesBrowserConfiguration || this.launchSettings.EnvironmentVariables.Count > 0 || this.browserOptions is not null)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify arguments, omitted default arguments, environment variables, a user data directory, or browser options with LaunchUsingRemoteGrid. " +
                "Use capabilities to configure the browser on the remote grid.");
        }

        if (this.headless)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify headless mode with LaunchUsingRemoteGrid. " +
                "Use capabilities to configure browser options on the remote grid.");
        }

        if (this.version != BrowserVersion.Latest)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify browser version with LaunchUsingRemoteGrid. " +
                "Use capabilities (e.g., 'browserVersion') to specify version on the remote grid.");
        }

        if (this.channel != BrowserReleaseChannel.Stable)
        {
            throw new BrowserLauncherConfigurationException(
                "Cannot specify release channel with LaunchUsingRemoteGrid. " +
                "Use capabilities to configure browser options on the remote grid.");
        }
    }

    private BrowserLauncher CreateSafariLauncher()
    {
        if (this.launchStrategy == LaunchStrategy.UsingRemoteGrid)
        {
            // Safari enables BiDi only when this capability accompanies webSocketUrl. It is a default rather
            // than a launcher-owned capability, so a caller can still replace it once Safari no longer needs it.
            WebDriverClassicBrowserLauncher remoteLauncher = this.CreateRemoteLauncher("safari");
            if (!remoteLauncher.AdditionalCapabilities.ContainsKey(SafariLauncher.ExperimentalWebSocketUrlCapabilityName))
            {
                remoteLauncher.AdditionalCapabilities[SafariLauncher.ExperimentalWebSocketUrlCapabilityName] = true;
            }

            return remoteLauncher;
        }

        SafariChannel safariChannel = this.channel switch
        {
            BrowserReleaseChannel.Stable => SafariChannel.Stable,
            BrowserReleaseChannel.DeveloperPreview => SafariChannel.TechnologyPreview,
            _ => throw new WebDriverBiDiException($"Invalid browser release channel for Safari: {this.channel}"),
        };

        SafariBrowserLocatorSettings settings = new(safariChannel);
        SafariLauncher safariLauncher = new(settings);
        return safariLauncher;
    }

    private BrowserLauncher CreateChromeLauncher()
    {
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
            _ => throw new WebDriverBiDiException($"Invalid browser release channel for Chrome: {this.channel}"),
        };

        string versionString = this.version.Value;
        string browserLocation = this.customBrowserLocation ?? string.Empty;

        ChromeBrowserLocatorSettings settings = new(chromeChannel, this.locationBehavior, this.downloadOptions ?? new BrowserDownloadOptions(), browserLocation, versionString);

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
            _ => throw new WebDriverBiDiException($"Invalid browser release channel for Firefox: {this.channel}"),
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

    private WebDriverClassicBrowserLauncher CreateRemoteLauncher(string browserName)
    {
        // remoteGridHostName is guaranteed non-null by ValidateConfiguration() called in Build()
        string hostName = this.remoteGridHostName ?? throw new InvalidOperationException("Remote grid hostname should have been validated.");
        RemoteBrowserLocatorSettings settings = new(browserName, hostName, this.remoteGridUseSsl);

        WebDriverClassicBrowserLauncher launcher = new(settings, this.port)
        {
            HostName = hostName,
            UseSsl = this.remoteGridUseSsl,
        };

        if (this.capabilities is not null)
        {
            // Copied, so capabilities added to this builder after Build() do not reach an already-built
            // launcher. The copy is shallow: a nested value such as an options dictionary is still shared.
            launcher.AdditionalCapabilities = new Dictionary<string, object>(this.capabilities);
        }

        return launcher;
    }
}
