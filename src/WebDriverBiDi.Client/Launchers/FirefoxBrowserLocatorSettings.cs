// <copyright file="FirefoxBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the locator settings for the Firefox browser, including properties such as the browser name,
/// channel, version, environment variable name, expected executable path, and installer file name.
/// This class also includes methods to retrieve browser download information based on the specified
/// channel or version, and to determine the appropriate download URL for the Firefox browser based
/// on the current platform and architecture. The locator settings are used by the BrowserLocator to
/// locate and download the correct version of Firefox for testing with WebDriver BiDi.
/// </summary>
internal class FirefoxBrowserLocatorSettings : BrowserLocatorSettings
{
    private readonly FirefoxChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="version">The specific released version of the Firefox browser.</param>
    public FirefoxBrowserLocatorSettings(string version)
        : this(FirefoxChannel.Stable, FileLocationBehavior.AutoLocateAndDownload)
    {
        this.Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Firefox browser.</param>
    public FirefoxBrowserLocatorSettings(FirefoxChannel channel)
        : this(channel, FileLocationBehavior.AutoLocateAndDownload)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Firefox browser.</param>
    /// <param name="locationBehavior">The location behavior for the Firefox browser.</param>
    /// <param name="expectedExecutablePath">The expected path to the Firefox executable.</param>
    /// <param name="version">The version of the Firefox browser to locate or download.</param>
    public FirefoxBrowserLocatorSettings(FirefoxChannel channel, FileLocationBehavior locationBehavior, string expectedExecutablePath = "", string version = LatestVersionString)
    {
        this.channelValue = channel;
        this.BrowserName = "firefox";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = $"Firefox {channel}";
        this.EnvironmentVariableName = "FIREFOX_EXECUTABLE";
        this.LocationBehavior = locationBehavior;
        this.InstallerFileName = this.InitializeInstallerFileName();
        this.ExpectedExecutablePath = this.InitializeExpectedExecutablePath(expectedExecutablePath);
        if (this.LocationBehavior == FileLocationBehavior.AutoLocateAndDownload)
        {
            this.Version = version;
        }
        else
        {
            this.Version = this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation ? SystemVersionString : LatestVersionString;
        }

        this.InitializeExtractors();
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "firefox").
    /// </summary>
    public override string BrowserName { get; } = "firefox";

    /// <summary>
    /// Gets the name of the driver executable (e.g., "geckodriver" or "geckodriver.exe").
    /// </summary>
    public override string DriverExecutableName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "geckodriver.exe" : "geckodriver";

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "GECKODRIVER_EXECUTABLE";

    /// <summary>
    /// Gets the browser download information for the Firefox browser version or channel specified..
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo()
    {
        BrowserDownloadInfo downloadInfo = new()
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = this.Version,
        };

        if (!this.IsLatestChannelVersion)
        {
            downloadInfo.DownloadUrl = this.GetDirectDownloadUrl();
            return downloadInfo;
        }

        string downloadServiceUrl = this.GetDownloadServiceUrl();

        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = false,
        };
        using HttpClient httpClient = new(handler);
        HttpResponseMessage response = await httpClient.GetAsync(downloadServiceUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (response.Headers.Location is not null)
        {
            string location = response.Headers.Location.ToString();
            downloadInfo.DownloadUrl = location;
            downloadInfo.Version = this.GetVersionNumberFromDownloadUrl(location);
            downloadInfo.IgnoreVersionMatch = this.channelValue == FirefoxChannel.Nightly;
            return downloadInfo;
        }

        downloadInfo.DownloadUrl = downloadServiceUrl;
        return downloadInfo;
    }

    /// <summary>
    /// Gets the driver download information for the geckodriver that is compatible with the Firefox browser.
    /// Uses the <see cref="BrowserLocatorSettings.DriverVersion"/> property to determine which driver version to download,
    /// or uses the latest version if <see cref="BrowserLocatorSettings.DriverVersion"/> is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public override async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo()
    {
        using HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "WebDriverBiDi.NET");

        string apiUrl = string.IsNullOrEmpty(this.DriverVersion) || this.DriverVersion == LatestVersionString
            ? "https://api.github.com/repos/mozilla/geckodriver/releases/latest"
            : $"https://api.github.com/repos/mozilla/geckodriver/releases/tags/v{this.DriverVersion}";

        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
        string json = await response.Content.ReadAsStringAsync();

        GeckoDriverRelease? release = JsonSerializer.Deserialize(json, GeckoDriverJsonSerializerContext.Default.GeckoDriverRelease);
        if (release is null)
        {
            throw new InvalidOperationException($"Failed to deserialize geckodriver release information from {apiUrl}.");
        }

        string platformIdentifier = this.GetDriverPlatformIdentifier();
        GeckoDriverAsset? matchingAsset = null;
        foreach (GeckoDriverAsset asset in release.Assets)
        {
            if (asset.Name.Contains(platformIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                matchingAsset = asset;
                break;
            }
        }

        if (matchingAsset is null)
        {
            throw new InvalidOperationException($"Failed to find geckodriver asset for platform {platformIdentifier}.");
        }

        DriverDownloadInfo driverDownloadInfo = new()
        {
            DriverName = "geckodriver",
            Version = release.TagName.TrimStart('v'),
            BrowserVersion = this.Version,
            DownloadUrl = matchingAsset.BrowserDownloadUrl,
            InstallerFileName = matchingAsset.Name,
        };

        return driverDownloadInfo;
    }

    private string GetDriverPlatformIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "macos-aarch64" : "macos";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "win64" : "win32";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux64";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for geckodriver download.");
        }
    }

    private void InitializeExtractors()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            this.BrowserExtractor = new DiskImageFileExtractor();
            this.DriverExtractor = new TarballFileExtractor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            this.BrowserExtractor = new TarballFileExtractor();
            this.DriverExtractor = new TarballFileExtractor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Driver distribution for Windows is a .zip file, so the base class
            // ZipFileExtractor can be used.
            this.BrowserExtractor = new SelfExtractingExecutableFileExtractor();
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }
    }

    private string InitializeInstallerFileName()
    {
        string baseName = $"{this.BrowserName}-{this.Channel}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"{baseName}.dmg";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"{baseName}.tar.xz";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{baseName}-installer.exe";
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
    }

    private string InitializeExpectedExecutablePath(string expectedExecutablePath)
    {
        if (this.LocationBehavior == FileLocationBehavior.UseSystemInstallLocation)
        {
            return this.GetDefaultSystemInstalledLocation();
        }

        if (this.LocationBehavior == FileLocationBehavior.UseCustomLocation)
        {
            if (string.IsNullOrEmpty(expectedExecutablePath))
            {
                throw new ArgumentException("Executable path must be provided when using custom location behavior.", nameof(expectedExecutablePath));
            }

            return expectedExecutablePath;
        }

        return this.GetCachedRelativeLocation();
    }

    private string GetVersionNumberFromDownloadUrl(string downloadUrl)
    {
        string versionStartMarker = this.channelValue == FirefoxChannel.Nightly ? "firefox-" : "releases/";
        string versionEndMarker = this.channelValue == FirefoxChannel.Nightly ? ".en-US" : "/";

        int versionStartIndex = downloadUrl.IndexOf(versionStartMarker) + versionStartMarker.Length;
        int versionEndIndex = downloadUrl.IndexOf(versionEndMarker, versionStartIndex);
        return downloadUrl.Substring(versionStartIndex, versionEndIndex - versionStartIndex);
    }

    private string GetDirectDownloadUrl()
    {
        string osMarker;
        string fileName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osMarker = "mac";
            fileName = $"Firefox {this.Version}.dmg";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osMarker = "linux-x86_64";
            fileName = $"firefox-{this.Version}.tar.xz";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osMarker = "win64";
            fileName = $"Firefox Setup {this.Version}.exe";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }

        return $"https://download-installer.cdn.mozilla.net/pub/firefox/releases/{this.Version}/{osMarker}/en-US/{fileName}";
    }

    private string GetDownloadServiceUrl()
    {
        string osMarker;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osMarker = "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osMarker = "linux64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osMarker = "win64";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }

        string product = this.channelValue switch
        {
            FirefoxChannel.Stable => "firefox-latest",
            FirefoxChannel.Beta => "firefox-beta-latest",
            FirefoxChannel.Dev => "firefox-devedition-latest",
            FirefoxChannel.Nightly => "firefox-nightly-latest",
            _ => throw new InvalidOperationException($"Unsupported Firefox channel: {this.channelValue}."),
        };

        return $"https://download.mozilla.org/?product={product}-ssl&os={osMarker}&lang=en-US";
    }

    private string GetCachedRelativeLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string appBundleName = "Firefox.app";
            if (this.channelValue == FirefoxChannel.Dev)
            {
                appBundleName = "Firefox Developer Edition.app";
            }
            else if (this.channelValue == FirefoxChannel.Nightly)
            {
                appBundleName = "Firefox Nightly.app";
            }

            return Path.Combine(appBundleName, "Contents", "MacOS", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine("firefox", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine("firefox", "firefox.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
    }

    private string GetDefaultSystemInstalledLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Note carefully that Stable and Beta channels share the same default
            // install location on MacOS.
            string applicationBundleName = this.channelValue switch
            {
                FirefoxChannel.Dev => "Firefox Developer Edition",
                FirefoxChannel.Nightly => "Firefox Nightly",
                _ => "Firefox",
            };
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                $"{applicationBundleName}.app",
                "Contents",
                "MacOS",
                "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string executableName = this.channelValue switch
            {
                FirefoxChannel.Beta => "firefox-beta",
                FirefoxChannel.Nightly => "firefox-nightly",
                _ => "firefox",
            };
            return Path.Combine(
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
                "usr",
                "bin",
                executableName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Note carefully that Stable and Beta channels share the same default
            // install location on Windows.
            string applicationSubdirectory = this.channelValue switch
            {
                FirefoxChannel.Dev => "Firefox Developer Edition",
                FirefoxChannel.Nightly => "Mozilla Firefox Nightly",
                _ => "Mozilla Firefox",
            };
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                applicationSubdirectory,
                "firefox.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} system install location.");
    }

    /// <summary>
    /// Represents a geckodriver release from the GitHub API.
    /// </summary>
    internal record GeckoDriverRelease
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeckoDriverRelease"/> class.
        /// </summary>
        [JsonConstructor]
        public GeckoDriverRelease()
        {
        }

        /// <summary>
        /// Gets or sets the tag name of the release (e.g., "v0.35.0").
        /// </summary>
        [JsonPropertyName("tag_name")]
        [JsonInclude]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the release.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonInclude]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of assets (downloadable files) for this release.
        /// </summary>
        [JsonPropertyName("assets")]
        [JsonInclude]
        public List<GeckoDriverAsset> Assets { get; set; } = [];
    }

    /// <summary>
    /// Represents a downloadable asset from a geckodriver GitHub release.
    /// </summary>
    internal record GeckoDriverAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeckoDriverAsset"/> class.
        /// </summary>
        [JsonConstructor]
        public GeckoDriverAsset()
        {
        }

        /// <summary>
        /// Gets or sets the name of the asset file.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonInclude]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the direct download URL for this asset.
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        [JsonInclude]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}

#pragma warning disable SA1402 // File may only contain a single type
/// <summary>
/// A source generation context for JSON serialization of geckodriver release information.
/// This is used to enable serialization and deserialization of the geckodriver release information
/// when used in AOT environments.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(FirefoxBrowserLocatorSettings.GeckoDriverRelease))]
[JsonSerializable(typeof(List<FirefoxBrowserLocatorSettings.GeckoDriverAsset>))]
internal partial class GeckoDriverJsonSerializerContext : JsonSerializerContext
{
}
