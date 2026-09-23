// <copyright file="FirefoxBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

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
    /// <param name="channel">The distribution channel of the Firefox browser.</param>
    /// <param name="locationBehavior">The location behavior for the Firefox browser.</param>
    /// <param name="downloadOptions">The options controlling where Firefox is cached and downloaded from.</param>
    /// <param name="expectedExecutablePath">The expected path to the Firefox executable.</param>
    /// <param name="version">The version of the Firefox browser to locate or download.</param>
    public FirefoxBrowserLocatorSettings(FirefoxChannel channel, FileLocationBehavior locationBehavior, BrowserDownloadOptions downloadOptions, string expectedExecutablePath = "", string version = LatestVersionString)
        : base(downloadOptions)
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
    public override string DriverExecutableName => this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? "geckodriver.exe" : "geckodriver";

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "GECKODRIVER_EXECUTABLE";

    private BrowserPlatform Platform => this.DownloadOptions.ResolvedPlatform;

    /// <summary>
    /// Gets the browser download information for the Firefox browser version or channel specified..
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo(CancellationToken cancellationToken)
    {
        BrowserDownloadInfo downloadInfo = new()
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = this.Version,
        };

        if (!this.IsLatestChannelVersion)
        {
            downloadInfo.DownloadUrl = this.GetDirectDownloadUrl().AbsoluteUri;
            return downloadInfo;
        }

        Uri downloadServiceUrl = this.GetDownloadServiceUrl();
        Uri redirectTarget = await DownloadHttpClient.GetRedirectTargetAsync(this.DownloadOptions, downloadServiceUrl, cancellationToken).ConfigureAwait(false);
        if (redirectTarget != downloadServiceUrl)
        {
            string location = redirectTarget.AbsoluteUri;
            downloadInfo.DownloadUrl = location;
            downloadInfo.Version = this.GetVersionNumberFromDownloadUrl(location);
            downloadInfo.IgnoreVersionMatch = this.channelValue == FirefoxChannel.Nightly;
            return downloadInfo;
        }

        downloadInfo.DownloadUrl = downloadServiceUrl.AbsoluteUri;
        return downloadInfo;
    }

    /// <summary>
    /// Gets the driver download information for the geckodriver that is compatible with the Firefox browser.
    /// Uses the <see cref="BrowserLocatorSettings.DriverVersion"/> property to determine which driver version to download,
    /// or uses the latest version if <see cref="BrowserLocatorSettings.DriverVersion"/> is null.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser; not used, as geckodriver releases are independent of Firefox releases.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public override async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo(string? browserVersion, CancellationToken cancellationToken)
    {
        Uri apiUrl = new(
            this.DownloadOptions.GeckoDriverReleasesEndpoint,
            string.IsNullOrEmpty(this.DriverVersion) || this.DriverVersion == LatestVersionString ? "latest" : $"tags/v{this.DriverVersion}");

        string json = await DownloadHttpClient.GetStringAsync(this.DownloadOptions, apiUrl, cancellationToken).ConfigureAwait(false);

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
        return this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => this.Platform.Architecture == Architecture.Arm64 ? "macos-aarch64" : "macos",
            OperatingSystemFamily.Windows => this.Platform.Architecture == Architecture.X64 ? "win64" : "win32",
            _ => "linux64",
        };
    }

    private void InitializeExtractors()
    {
        switch (this.Platform.OperatingSystem)
        {
            case OperatingSystemFamily.MacOS:
                this.BrowserExtractor = new DiskImageFileExtractor();
                this.DriverExtractor = new TarballFileExtractor();
                break;

            case OperatingSystemFamily.Windows:
                // Driver distribution for Windows is a .zip file, so the base class
                // ZipFileExtractor can be used.
                this.BrowserExtractor = new SelfExtractingExecutableFileExtractor("core", this.BrowserName);
                break;

            default:
                this.BrowserExtractor = new TarballFileExtractor();
                this.DriverExtractor = new TarballFileExtractor();
                break;
        }
    }

    private string InitializeInstallerFileName()
    {
        string baseName = $"{this.BrowserName}-{this.Channel}";
        return this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => $"{baseName}.dmg",
            OperatingSystemFamily.Windows => $"{baseName}-installer.exe",
            _ => $"{baseName}.tar.xz",
        };
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

    private Uri GetDirectDownloadUrl()
    {
        (string osMarker, string fileName) = this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => ("mac", $"Firefox {this.Version}.dmg"),
            OperatingSystemFamily.Windows => ("win64", $"Firefox Setup {this.Version}.exe"),
            _ => ("linux-x86_64", $"firefox-{this.Version}.tar.xz"),
        };

        return new Uri(this.DownloadOptions.FirefoxReleaseArchiveEndpoint, $"{this.Version}/{osMarker}/en-US/{fileName}");
    }

    private Uri GetDownloadServiceUrl()
    {
        string osMarker = this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => "osx",
            OperatingSystemFamily.Windows => "win64",
            _ => "linux64",
        };

        string product = this.channelValue switch
        {
            FirefoxChannel.Stable => "firefox-latest",
            FirefoxChannel.Beta => "firefox-beta-latest",
            FirefoxChannel.Dev => "firefox-devedition-latest",
            FirefoxChannel.Nightly => "firefox-nightly-latest",
            _ => throw new InvalidOperationException($"Unsupported Firefox channel: {this.channelValue}."),
        };

        return new Uri(this.DownloadOptions.FirefoxProductEndpoint, $"?product={product}-ssl&os={osMarker}&lang=en-US");
    }

    private string GetCachedRelativeLocation()
    {
        if (this.Platform.OperatingSystem == OperatingSystemFamily.MacOS)
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

        return this.Platform.OperatingSystem == OperatingSystemFamily.Windows
            ? Path.Combine("firefox", "firefox.exe")
            : Path.Combine("firefox", "firefox");
    }

    private string GetDefaultSystemInstalledLocation()
    {
        if (this.Platform.OperatingSystem == OperatingSystemFamily.MacOS)
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

        if (this.Platform.OperatingSystem == OperatingSystemFamily.Linux)
        {
            string executableName = this.channelValue switch
            {
                FirefoxChannel.Beta => "firefox-beta",
                FirefoxChannel.Nightly => "firefox-nightly",
                _ => "firefox",
            };
            return Path.Combine(
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))!,
                "usr",
                "bin",
                executableName);
        }

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
