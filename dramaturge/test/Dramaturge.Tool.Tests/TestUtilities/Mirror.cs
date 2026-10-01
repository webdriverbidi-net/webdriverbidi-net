// <copyright file="Mirror.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool.TestUtilities;

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

/// <summary>
/// A download mirror in a directory: a manifest listing Linux x64 builds beside it, which the tool downloads as it
/// would from the vendors, with no server.
/// </summary>
public sealed class Mirror : IDisposable
{
    /// <summary>
    /// The version of Chrome and chromedriver the mirror lists as stable.
    /// </summary>
    public const string ChromeVersion = "131.0.6778.204";

    /// <summary>
    /// The version of Chrome the mirror lists as beta.
    /// </summary>
    public const string ChromeBetaVersion = "132.0.6834.57";

    /// <summary>
    /// The version of Firefox the mirror lists as stable.
    /// </summary>
    public const string FirefoxVersion = "134.0";

    /// <summary>
    /// The version of Firefox the mirror lists as nightly.
    /// </summary>
    public const string FirefoxNightlyVersion = "136.0a1";

    /// <summary>
    /// The version of geckodriver the mirror lists as latest.
    /// </summary>
    public const string GeckoDriverVersion = "0.36.0";

    /// <summary>
    /// The version of msedgedriver the mirror lists.
    /// </summary>
    public const string EdgeDriverVersion = "131.0.2903.51";

    /// <summary>
    /// The size of the incompressible file in the Chrome beta build, large enough for its download to report progress several times.
    /// </summary>
    public const int LargeBuildSize = 5 * 1024 * 1024;

    private const string Platform = "linux-x64";
    private readonly TemporaryDirectory directory = new();
    private readonly JsonObject browsers = [];
    private readonly JsonObject drivers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Mirror"/> class.
    /// </summary>
    public Mirror()
    {
        this.AddBuild(this.browsers, "chrome", ChromeVersion, Zip("chrome-linux64/chrome"));
        this.AddBuild(this.browsers, "chrome", ChromeBetaVersion, Zip("chrome-linux64/chrome", LargeBuildSize));
        this.AddBuild(this.browsers, "chrome-headless-shell", ChromeVersion, Zip("chrome-headless-shell-linux64/chrome-headless-shell"));
        this.AddBuild(this.browsers, "firefox", FirefoxVersion, TarGz("firefox/firefox"));
        this.AddBuild(this.browsers, "firefox", FirefoxNightlyVersion, TarGz("firefox/firefox"));
        this.AddBuild(this.drivers, "chromedriver", ChromeVersion, Zip("chromedriver-linux64/chromedriver"));
        this.AddBuild(this.drivers, "chromedriver", ChromeBetaVersion, Zip("chromedriver-linux64/chromedriver"));
        this.AddBuild(this.drivers, "geckodriver", GeckoDriverVersion, TarGz("geckodriver"));
        this.AddBuild(this.drivers, "msedgedriver", EdgeDriverVersion, Zip("msedgedriver"));
        this.browsers["chrome"]!["channels"] = new JsonObject() { ["stable"] = ChromeVersion, ["beta"] = ChromeBetaVersion };
        this.browsers["chrome-headless-shell"]!["channels"] = new JsonObject() { ["stable"] = ChromeVersion };
        this.browsers["firefox"]!["channels"] = new JsonObject() { ["stable"] = FirefoxVersion, ["nightly"] = FirefoxNightlyVersion };
        this.drivers["geckodriver"]!["latest"] = GeckoDriverVersion;
        File.WriteAllText(this.ManifestPath, new JsonObject() { ["schemaVersion"] = 1, ["browsers"] = this.browsers, ["drivers"] = this.drivers }.ToJsonString());
    }

    /// <summary>
    /// Gets the path of the manifest.
    /// </summary>
    public string ManifestPath => System.IO.Path.Combine(this.directory.Path, "manifest.json");

    /// <summary>
    /// Gets the URL of a product's build of a version.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="version">The version.</param>
    /// <returns>The URL.</returns>
    public Uri BuildUrl(string product, string version) => new(System.IO.Path.Combine(this.directory.Path, product, version, "build"));

    /// <inheritdoc/>
    public void Dispose() => this.directory.Dispose();

    /// <summary>
    /// Creates a zip archive holding a file, and optionally an incompressible file of the given size.
    /// </summary>
    /// <param name="entryPath">The '/'-separated path of the file.</param>
    /// <param name="paddingSize">The size of the incompressible file, or 0 for none.</param>
    /// <returns>The archive contents.</returns>
    public static byte[] Zip(string entryPath, int paddingSize = 0)
    {
        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (StreamWriter writer = new(archive.CreateEntry(entryPath).Open()))
            {
                writer.Write(entryPath);
            }

            if (paddingSize > 0)
            {
                using Stream padding = archive.CreateEntry("padding", CompressionLevel.NoCompression).Open();
                padding.Write(RandomNumberGenerator.GetBytes(paddingSize));
            }
        }

        return stream.ToArray();
    }

    private static byte[] TarGz(string entryPath)
    {
        using MemoryStream stream = new();
        using (GZipStream gzip = new(stream, CompressionLevel.Fastest, leaveOpen: true))
        using (TarWriter tar = new(gzip, TarEntryFormat.Pax))
        {
            PaxTarEntry entry = new(TarEntryType.RegularFile, entryPath) { DataStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(entryPath)) };
            tar.WriteEntry(entry);
        }

        return stream.ToArray();
    }

    private void AddBuild(JsonObject section, string product, string version, byte[] content)
    {
        string relativeUrl = $"{product}/{version}/build";
        string path = System.IO.Path.Combine(this.directory.Path, product, version, "build");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        if (section[product] is not JsonObject entry)
        {
            entry = new JsonObject() { ["channels"] = new JsonObject(), ["versions"] = new JsonObject() };
            section[product] = entry;
        }

        entry["versions"]![version] = new JsonObject()
        {
            [Platform] = new JsonObject() { ["url"] = relativeUrl, ["sha256"] = Convert.ToHexStringLower(SHA256.HashData(content)), ["size"] = content.Length },
        };
    }
}
