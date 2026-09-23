// <copyright file="DownloadManifest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A mirror manifest, set by <see cref="BrowserDownloadOptions.ManifestUrl"/>, which lists the
/// browser and driver builds to download in place of the vendors' services.
/// </summary>
internal sealed class DownloadManifest
{
    private const int SupportedSchemaVersion = 1;
    private const int Sha256HexLength = 64;

    private readonly Uri url;
    private readonly ManifestDocument document;

    private DownloadManifest(Uri url, ManifestDocument document)
    {
        this.url = url;
        this.document = document;
    }

    /// <summary>
    /// Reads the manifest named by the download options.
    /// </summary>
    /// <param name="options">The download options, whose <see cref="BrowserDownloadOptions.ManifestUrl"/> is set.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The manifest.</returns>
    /// <exception cref="DownloadManifestException">Thrown when the manifest is not valid.</exception>
    public static async Task<DownloadManifest> LoadAsync(BrowserDownloadOptions options, CancellationToken cancellationToken)
    {
        Uri url = options.ManifestUrl!;
        string json = url.IsFile
            ? File.ReadAllText(url.LocalPath)
            : await DownloadHttpClient.GetStringAsync(options, url, cancellationToken).ConfigureAwait(false);
        ManifestDocument? document;
        try
        {
            document = JsonSerializer.Deserialize(json, DownloadManifestJsonSerializerContext.Default.ManifestDocument);
        }
        catch (JsonException ex)
        {
            throw new DownloadManifestException($"The download manifest {url} is not valid JSON: {ex.Message}");
        }

        if (document is null || document.SchemaVersion != SupportedSchemaVersion)
        {
            throw new DownloadManifestException($"The download manifest {url} has schemaVersion {document?.SchemaVersion}; only {SupportedSchemaVersion} is supported.");
        }

        // A URL made from a file path resolves relative URLs without unescaping them; one made from its own text does not.
        return new DownloadManifest(url.IsFile ? new Uri(url.AbsoluteUri) : url, document);
    }

    /// <summary>
    /// Gets the manifest's name for a platform, such as "linux-x64" or "macos-arm64".
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <returns>The platform's name.</returns>
    public static string GetPlatformKey(BrowserPlatform platform)
    {
        string operatingSystem = platform.OperatingSystem.ToString().ToLowerInvariant();
        string architecture = platform.Architecture.ToString().ToLowerInvariant();
        return $"{operatingSystem}-{architecture}";
    }

    /// <summary>
    /// Finds the browser build that the locator settings request.
    /// </summary>
    /// <param name="settings">The locator settings.</param>
    /// <returns>The browser download.</returns>
    /// <exception cref="DownloadManifestException">Thrown when the manifest does not list the build.</exception>
    public BrowserDownloadInfo ResolveBrowser(BrowserLocatorSettings settings)
    {
        ManifestProduct product = this.GetProduct(this.document.Browsers, settings.BrowserName);
        string version = this.ResolveBrowserVersion(product, settings);
        (Uri downloadUrl, ManifestBuild build) = this.GetBuild(product, settings.BrowserName, version, settings.DownloadOptions.ResolvedPlatform);
        return new BrowserDownloadInfo()
        {
            BrowserName = settings.BrowserName,
            Channel = settings.Channel,
            Version = version,
            DownloadUrl = downloadUrl.AbsoluteUri,
            Sha256 = build.Sha256,
            Size = build.Size,
        };
    }

    /// <summary>
    /// Finds the driver build for the browser that the locator settings request.
    /// </summary>
    /// <param name="settings">The locator settings.</param>
    /// <param name="browserVersion">The version of the located browser, or <see langword="null"/> if it is not known.</param>
    /// <returns>The driver download.</returns>
    /// <exception cref="DownloadManifestException">Thrown when the manifest does not list the build.</exception>
    public DriverDownloadInfo ResolveDriver(BrowserLocatorSettings settings, string? browserVersion)
    {
        string driverName = settings.DriverName;
        ManifestProduct product = this.GetProduct(this.document.Drivers, driverName);
        string? version = settings.GetRequiredDriverVersion(browserVersion);
        if (version is null && settings.DriverVersionFollowsBrowser)
        {
            version = this.ResolveBrowserVersion(this.GetProduct(this.document.Browsers, settings.BrowserName), settings);
        }

        version ??= product.Latest ?? throw new DownloadManifestException($"The download manifest {this.url} lists no latest version of {driverName}.");
        (Uri downloadUrl, ManifestBuild build) = this.GetBuild(product, driverName, version, settings.DownloadOptions.ResolvedPlatform);
        string path = Uri.UnescapeDataString(downloadUrl.AbsolutePath);
        return new DriverDownloadInfo()
        {
            DriverName = driverName,
            Version = version,
            BrowserVersion = browserVersion ?? string.Empty,
            DownloadUrl = downloadUrl.AbsoluteUri,
            InstallerFileName = path.Substring(path.LastIndexOf('/') + 1),
            Sha256 = build.Sha256,
            Size = build.Size,
        };
    }

    // Only numeric versions (e.g., Chrome's "131.0.6778.204") can be the release of a milestone.
    private static Version? ParseNumericVersion(string version)
    {
        return System.Version.TryParse(version, out Version? parsed) ? parsed : null;
    }

    private ManifestProduct GetProduct(Dictionary<string, ManifestProduct> products, string name)
    {
        return products.TryGetValue(name, out ManifestProduct? product)
            ? product
            : throw new DownloadManifestException($"The download manifest {this.url} lists no {name} builds.");
    }

    private string ResolveBrowserVersion(ManifestProduct product, BrowserLocatorSettings settings)
    {
        if (settings.Milestone is int milestone)
        {
            return product.Versions.Keys
                .Select(version => (Version: version, Parsed: ParseNumericVersion(version)))
                .Where(candidate => candidate.Parsed?.Major == milestone)
                .OrderByDescending(candidate => candidate.Parsed)
                .Select(candidate => candidate.Version)
                .FirstOrDefault()
                ?? throw new DownloadManifestException($"The download manifest {this.url} lists no {settings.BrowserName} version of milestone {milestone}.");
        }

        if (settings.IsLatestChannelVersion)
        {
            return product.Channels.TryGetValue(settings.Channel, out string? version)
                ? version
                : throw new DownloadManifestException($"The download manifest {this.url} lists no {settings.Channel} channel of {settings.BrowserName}.");
        }

        return settings.Version;
    }

    private (Uri DownloadUrl, ManifestBuild Build) GetBuild(ManifestProduct product, string name, string version, BrowserPlatform platform)
    {
        string platformKey = GetPlatformKey(platform);
        if (!product.Versions.TryGetValue(version, out Dictionary<string, ManifestBuild>? builds))
        {
            throw new DownloadManifestException($"The download manifest {this.url} lists no {name} {version}.");
        }

        if (!builds.TryGetValue(platformKey, out ManifestBuild? build))
        {
            throw new DownloadManifestException($"The download manifest {this.url} lists no {name} {version} build for {platformKey}.");
        }

        if (string.IsNullOrEmpty(build.Url) || build.Sha256.Length != Sha256HexLength || !build.Sha256.All(Uri.IsHexDigit))
        {
            throw new DownloadManifestException($"The download manifest {this.url} lists the {name} {version} build for {platformKey} without a URL and a 64-digit hexadecimal sha256.");
        }

        return (new Uri(this.url, build.Url), build);
    }

    /// <summary>
    /// The manifest document.
    /// </summary>
    internal sealed class ManifestDocument
    {
        /// <summary>
        /// Gets or sets the version of the manifest format.
        /// </summary>
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the browsers, keyed by name (e.g., "chrome", "chrome-headless-shell", "firefox").
        /// </summary>
        [JsonPropertyName("browsers")]
        public Dictionary<string, ManifestProduct> Browsers { get; set; } = [];

        /// <summary>
        /// Gets or sets the drivers, keyed by name (e.g., "chromedriver", "geckodriver").
        /// </summary>
        [JsonPropertyName("drivers")]
        public Dictionary<string, ManifestProduct> Drivers { get; set; } = [];
    }

    /// <summary>
    /// The builds of one browser or driver.
    /// </summary>
    internal sealed class ManifestProduct
    {
        /// <summary>
        /// Gets or sets the version each channel resolves to, keyed by channel (e.g., "stable").
        /// </summary>
        [JsonPropertyName("channels")]
        public Dictionary<string, string> Channels { get; set; } = [];

        /// <summary>
        /// Gets or sets the latest version of a driver whose version does not follow the browser's.
        /// </summary>
        [JsonPropertyName("latest")]
        public string? Latest { get; set; }

        /// <summary>
        /// Gets or sets the builds of each version, keyed by version and then by platform (e.g., "linux-x64").
        /// </summary>
        [JsonPropertyName("versions")]
        public Dictionary<string, Dictionary<string, ManifestBuild>> Versions { get; set; } = [];
    }

    /// <summary>
    /// One downloadable build.
    /// </summary>
    internal sealed class ManifestBuild
    {
        /// <summary>
        /// Gets or sets the URL of the build, which may be relative to the manifest.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SHA-256 hash of the build in hexadecimal.
        /// </summary>
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the build in bytes, if listed.
        /// </summary>
        [JsonPropertyName("size")]
        public long? Size { get; set; }
    }
}

#pragma warning disable SA1402 // File may only contain a single type
/// <summary>
/// A source generation context for JSON deserialization of the download manifest.
/// </summary>
[JsonSourceGenerationOptions(ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
[JsonSerializable(typeof(DownloadManifest.ManifestDocument))]
internal partial class DownloadManifestJsonSerializerContext : JsonSerializerContext
{
}
