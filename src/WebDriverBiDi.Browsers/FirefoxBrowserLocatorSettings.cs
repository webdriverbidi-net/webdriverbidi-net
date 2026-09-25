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
internal class FirefoxBrowserLocatorSettings : BrowserLocatorSettings, IBrowserDownloadSource, IDriverDownloadSource
{
    // Firefox for Linux has been distributed as .tar.xz, rather than .tar.bz2, since version 135.
    private const int FirstXzCompressedLinuxVersion = 135;

    private const string Sha256DigestPrefix = "sha256:";
    private const string ReleaseChecksumsFileName = "SHA256SUMS";
    private const string NightlyChecksumsExtension = ".checksums";

    // Nightly build file names end with one of these; the checksums file beside a build replaces it.
    private static readonly string[] NightlyBuildSuffixes = [".installer.exe", ".tar.xz", ".tar.bz2", ".dmg"];

    private readonly FirefoxChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Firefox browser.</param>
    /// <param name="locationBehavior">The location behavior for the Firefox browser.</param>
    /// <param name="downloadOptions">The options controlling where Firefox is cached and downloaded from.</param>
    /// <param name="expectedExecutablePath">The expected path to the Firefox executable.</param>
    /// <param name="version">The version of the Firefox browser to locate or download.</param>
    /// <exception cref="ArgumentException">Thrown when the version is a milestone, or a specific Nightly version.</exception>
    public FirefoxBrowserLocatorSettings(FirefoxChannel channel, FileLocationBehavior locationBehavior, BrowserDownloadOptions downloadOptions, string expectedExecutablePath = "", string version = LatestVersionString)
        : base(downloadOptions)
    {
        if (locationBehavior == FileLocationBehavior.AutoLocateAndDownload)
        {
            if (version.StartsWith(MilestoneVersionPrefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Only Chrome versions can be requested by milestone.", nameof(version));
            }

            if (channel == FirefoxChannel.Nightly && version != LatestVersionString)
            {
                throw new ArgumentException("Firefox Nightly builds are not archived by version, so only the latest can be downloaded.", nameof(version));
            }
        }

        this.channelValue = channel;
        this.BrowserName = "firefox";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = $"Firefox {channel}";
        this.EnvironmentVariableName = "FIREFOX_EXECUTABLE";
        this.DriverExecutableName = this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? "geckodriver.exe" : "geckodriver";
        this.DriverEnvironmentVariableName = "GECKODRIVER_EXECUTABLE";
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

    private BrowserPlatform Platform => this.DownloadOptions.ResolvedPlatform;

    /// <summary>
    /// Gets the browser download information for the Firefox browser version or channel specified..
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public async Task<BrowserDownloadInfo> GetBrowserDownloadInfo(CancellationToken cancellationToken)
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

        Uri downloadServiceUrl = this.GetDownloadServiceUrl(this.GetRequiredPlatformIdentifiers().ProductOs);
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
    /// Gets the SHA-256 hash Mozilla lists for a Firefox download: in the release's SHA256SUMS file,
    /// or, for Nightly, in the checksums file published beside the build.
    /// </summary>
    /// <param name="downloadInfo">The download.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The hash in hexadecimal.</returns>
    /// <exception cref="DownloadVerificationException">Thrown when no hash is listed for the download.</exception>
    public async Task<string?> GetBrowserSha256Async(BrowserDownloadInfo downloadInfo, CancellationToken cancellationToken)
    {
        Uri downloadUrl = new(downloadInfo.DownloadUrl);
        string path = Uri.UnescapeDataString(downloadUrl.AbsolutePath);
        string fileName = path.Substring(path.LastIndexOf('/') + 1);
        string checksumsPath;
        string listedName;
        if (this.channelValue == FirefoxChannel.Nightly)
        {
            string suffix = NightlyBuildSuffixes.FirstOrDefault(suffix => fileName.EndsWith(suffix, StringComparison.Ordinal))
                ?? throw new DownloadVerificationException($"Cannot find the checksums file for the Firefox Nightly build {downloadUrl}.");
            checksumsPath = path.Substring(0, path.Length - suffix.Length) + NightlyChecksumsExtension;
            listedName = fileName;
        }
        else
        {
            string releaseDirectory = $"/releases/{downloadInfo.Version}/";
            int releaseDirectoryIndex = path.IndexOf(releaseDirectory, StringComparison.Ordinal);
            if (releaseDirectoryIndex < 0)
            {
                throw new DownloadVerificationException($"Cannot find the {ReleaseChecksumsFileName} file for the Firefox download {downloadUrl}.");
            }

            int releasePathLength = releaseDirectoryIndex + releaseDirectory.Length;
            checksumsPath = path.Substring(0, releasePathLength) + ReleaseChecksumsFileName;
            listedName = path.Substring(releasePathLength);
        }

        Uri checksumsUrl = new UriBuilder(downloadUrl) { Path = checksumsPath, Query = string.Empty }.Uri;
        string checksums = await DownloadHttpClient.GetStringAsync(this.DownloadOptions, checksumsUrl, cancellationToken).ConfigureAwait(false);
        return FindSha256(checksums, listedName) ?? throw new DownloadVerificationException($"{checksumsUrl} lists no SHA-256 hash for {listedName}.");
    }

    /// <summary>
    /// Gets the driver download information for the latest geckodriver, whose releases are independent of Firefox's.
    /// </summary>
    /// <param name="browserVersion">The version of the located browser; not used, as geckodriver releases are independent of Firefox releases.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    public async Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo(string? browserVersion, CancellationToken cancellationToken)
    {
        Uri apiUrl = new(this.DownloadOptions.GeckoDriverReleasesEndpoint, "latest");

        string json = await DownloadHttpClient.GetStringAsync(this.DownloadOptions, apiUrl, cancellationToken).ConfigureAwait(false);

        GeckoDriverRelease? release = JsonSerializer.Deserialize(json, GeckoDriverJsonSerializerContext.Default.GeckoDriverRelease);
        if (release is null)
        {
            throw new InvalidOperationException($"Failed to deserialize geckodriver release information from {apiUrl}.");
        }

        string platformIdentifier = this.GetDriverPlatformIdentifier();
        string assetName = $"geckodriver-{release.TagName}-{platformIdentifier}{(this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? ".zip" : ".tar.gz")}";
        GeckoDriverAsset matchingAsset = release.Assets.FirstOrDefault(asset => asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Failed to find geckodriver asset {assetName} for platform {this.Platform}.");

        DriverDownloadInfo driverDownloadInfo = new()
        {
            DriverName = "geckodriver",
            Version = release.TagName.TrimStart('v'),
            BrowserVersion = this.Version,
            DownloadUrl = matchingAsset.BrowserDownloadUrl,
            InstallerFileName = matchingAsset.Name,
            Sha256 = matchingAsset.Digest is string digest && digest.StartsWith(Sha256DigestPrefix, StringComparison.OrdinalIgnoreCase) ? digest.Substring(Sha256DigestPrefix.Length) : null,
            Size = matchingAsset.Size,
        };

        return driverDownloadInfo;
    }

    // SHA256SUMS lines are "<hash>  <path>"; Nightly checksums lines are "<hash> <algorithm> <size> <name>".
    private static string? FindSha256(string checksums, string listedName)
    {
        const int Sha256HexLength = 64;
        foreach (string line in checksums.Split('\n'))
        {
            string entry = line.TrimEnd('\r');
            if (entry.IndexOf(' ') != Sha256HexLength)
            {
                continue;
            }

            string rest = entry.Substring(Sha256HexLength).TrimStart(' ');
            string[] fields = rest.Split([' '], 3);
            if (rest == listedName || (fields.Length == 3 && fields[0] == "sha256" && fields[2] == listedName))
            {
                return entry.Substring(0, Sha256HexLength);
            }
        }

        return null;
    }

    private static int GetMajorVersion(string version)
    {
        int digits = 0;
        while (digits < version.Length && char.IsDigit(version[digits]))
        {
            digits++;
        }

        return digits > 0 ? int.Parse(version.Substring(0, digits), System.Globalization.CultureInfo.InvariantCulture) : throw new FormatException($"'{version}' is not a Firefox version.");
    }

    private string GetDriverPlatformIdentifier()
    {
        return (this.Platform.OperatingSystem, this.Platform.Architecture) switch
        {
            (OperatingSystemFamily.MacOS, Architecture.Arm64) => "macos-aarch64",
            (OperatingSystemFamily.MacOS, Architecture.X64) => "macos",
            (OperatingSystemFamily.Windows, Architecture.X64) => "win64",
            (OperatingSystemFamily.Windows, Architecture.X86) => "win32",
            (OperatingSystemFamily.Windows, Architecture.Arm64) => "win-aarch64",
            (OperatingSystemFamily.Linux, Architecture.X64) => "linux64",
            (OperatingSystemFamily.Linux, Architecture.X86) => "linux32",
            (OperatingSystemFamily.Linux, Architecture.Arm64) => "linux-aarch64",
            _ => throw new PlatformNotSupportedException($"geckodriver is not published for {this.Platform}."),
        };
    }

    // The download service's "os" value, and the name of the archive's platform directory.
    private (string ProductOs, string ArchiveDirectory) GetRequiredPlatformIdentifiers()
    {
        return (this.Platform.OperatingSystem, this.Platform.Architecture) switch
        {
            (OperatingSystemFamily.MacOS, Architecture.X64 or Architecture.Arm64) => ("osx", "mac"),
            (OperatingSystemFamily.Windows, Architecture.X64) => ("win64", "win64"),
            (OperatingSystemFamily.Windows, Architecture.X86) => ("win", "win32"),
            (OperatingSystemFamily.Windows, Architecture.Arm64) => ("win64-aarch64", "win64-aarch64"),
            (OperatingSystemFamily.Linux, Architecture.X64) => ("linux64", "linux-x86_64"),
            (OperatingSystemFamily.Linux, Architecture.X86) => ("linux", "linux-i686"),
            (OperatingSystemFamily.Linux, Architecture.Arm64) => ("linux64-aarch64", "linux-aarch64"),
            _ => throw new PlatformNotSupportedException($"Firefox is not published for {this.Platform}."),
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
            _ => $"{baseName}.tar",
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
            return expectedExecutablePath;
        }

        return this.GetCachedRelativeLocation();
    }

    private string GetVersionNumberFromDownloadUrl(string downloadUrl)
    {
        string versionStartMarker = this.channelValue == FirefoxChannel.Nightly ? "firefox-" : "releases/";
        string versionEndMarker = this.channelValue == FirefoxChannel.Nightly ? ".en-US" : "/";

        int versionStartIndex = downloadUrl.IndexOf(versionStartMarker, StringComparison.Ordinal);
        int versionEndIndex = versionStartIndex < 0 ? -1 : downloadUrl.IndexOf(versionEndMarker, versionStartIndex + versionStartMarker.Length, StringComparison.Ordinal);
        if (versionEndIndex < 0)
        {
            throw new FormatException($"Could not determine the Firefox version from the download URL {downloadUrl}.");
        }

        versionStartIndex += versionStartMarker.Length;
        return downloadUrl.Substring(versionStartIndex, versionEndIndex - versionStartIndex);
    }

    private Uri GetDirectDownloadUrl()
    {
        string archiveDirectory = this.GetRequiredPlatformIdentifiers().ArchiveDirectory;
        string fileName = this.Platform.OperatingSystem switch
        {
            OperatingSystemFamily.MacOS => $"Firefox {this.Version}.dmg",
            OperatingSystemFamily.Windows => $"Firefox Setup {this.Version}.exe",
            _ => GetMajorVersion(this.Version) >= FirstXzCompressedLinuxVersion ? $"firefox-{this.Version}.tar.xz" : $"firefox-{this.Version}.tar.bz2",
        };

        string product = this.channelValue == FirefoxChannel.Dev ? "devedition" : "firefox";
        return new Uri(this.DownloadOptions.FirefoxArchiveEndpoint, $"{product}/releases/{this.Version}/{archiveDirectory}/en-US/{fileName}");
    }

    private Uri GetDownloadServiceUrl(string osMarker)
    {
        string product = this.channelValue switch
        {
            FirefoxChannel.Stable => "firefox-latest",
            FirefoxChannel.Beta => "firefox-beta-latest",
            FirefoxChannel.Dev => "firefox-devedition-latest",
            FirefoxChannel.Nightly => "firefox-nightly-latest",
            _ => "firefox-esr-latest",
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
                FirefoxChannel.Esr => "firefox-esr",
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
        return GetWindowsProgramFilesPath(Path.Combine(applicationSubdirectory, "firefox.exe"));
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

        /// <summary>
        /// Gets or sets the digest of the asset (e.g., "sha256:..."), which GitHub lists for assets uploaded since mid-2025.
        /// </summary>
        [JsonPropertyName("digest")]
        [JsonInclude]
        public string? Digest { get; set; }

        /// <summary>
        /// Gets or sets the size of the asset in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        [JsonInclude]
        public long? Size { get; set; }
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
