// <copyright file="ChromeForTestingService.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Text.Json.Nodes;

/// <summary>
/// Serves a fake Chrome for Testing version information service, and the Chrome and
/// chromedriver archives it lists, from a <see cref="DownloadServer"/>.
/// </summary>
public static class ChromeForTestingService
{
    /// <summary>
    /// The base path of the service on the server.
    /// </summary>
    public const string BasePath = "/cft/";

    /// <summary>
    /// The path of the document listing the latest version of each channel.
    /// </summary>
    public const string ChannelDocumentPath = BasePath + "last-known-good-versions-with-downloads.json";

    /// <summary>
    /// The path of the document listing every version.
    /// </summary>
    public const string AllVersionsDocumentPath = BasePath + "known-good-versions-with-downloads.json";

    /// <summary>
    /// The platform identifiers Chrome for Testing publishes.
    /// </summary>
    public static readonly string[] PlatformIdentifiers = ["linux64", "mac-arm64", "mac-x64", "win32", "win64"];

    /// <summary>
    /// Serves both version documents, listing <paramref name="version"/> as the latest release of
    /// <paramref name="channel"/>, and the Chrome and chromedriver archives for every platform.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="channel">The channel name, as Chrome for Testing spells it (e.g., "Stable").</param>
    /// <param name="version">The version.</param>
    /// <param name="otherVersions">Additional versions listed only in the all-versions document.</param>
    public static void Serve(DownloadServer server, string channel, string version, params string[] otherVersions)
    {
        Serve(server, channel, version, null, otherVersions);
    }

    /// <summary>
    /// Serves both version documents, listing <paramref name="version"/> as the latest release of
    /// <paramref name="channel"/>, and archives for every platform, serving
    /// <paramref name="chromeArchiveContent"/> in place of each Chrome archive.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="channel">The channel name, as Chrome for Testing spells it (e.g., "Stable").</param>
    /// <param name="version">The version.</param>
    /// <param name="chromeArchiveContent">The content served for each Chrome archive, or <see langword="null"/> for a valid archive.</param>
    /// <param name="otherVersions">Additional versions listed only in the all-versions document.</param>
    public static void Serve(DownloadServer server, string channel, string version, byte[]? chromeArchiveContent, params string[] otherVersions)
    {
        JsonObject channelDocument = new()
        {
            ["timestamp"] = "2026-09-23T00:00:00.000Z",
            ["channels"] = new JsonObject() { [channel] = CreateVersionEntry(server, version, channel, chromeArchiveContent) },
        };
        JsonArray versions = [];
        foreach (string listedVersion in otherVersions.Append(version))
        {
            versions.Add(CreateVersionEntry(server, listedVersion, null, chromeArchiveContent));
        }

        server.AddText(ChannelDocumentPath, channelDocument.ToJsonString());
        server.AddText(AllVersionsDocumentPath, new JsonObject() { ["timestamp"] = "2026-09-23T00:00:00.000Z", ["versions"] = versions }.ToJsonString());
    }

    /// <summary>
    /// Gets the path of the Chrome archive for a version and platform.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="platformIdentifier">The platform identifier.</param>
    /// <returns>The archive path.</returns>
    public static string ChromeArchivePath(string version, string platformIdentifier) => $"{BasePath}{version}/{platformIdentifier}/chrome-{platformIdentifier}.zip";

    /// <summary>
    /// Gets the path of the chromedriver archive for a version and platform.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="platformIdentifier">The platform identifier.</param>
    /// <returns>The archive path.</returns>
    public static string DriverArchivePath(string version, string platformIdentifier) => $"{BasePath}{version}/{platformIdentifier}/chromedriver-{platformIdentifier}.zip";

    /// <summary>
    /// Gets the path, relative to the version's install directory, at which the Chrome executable is extracted.
    /// </summary>
    /// <param name="platformIdentifier">The platform identifier.</param>
    /// <returns>The '/'-separated relative path.</returns>
    public static string ChromeExecutablePath(string platformIdentifier) => platformIdentifier switch
    {
        "linux64" => "chrome-linux64/chrome",
        "mac-arm64" or "mac-x64" => $"chrome-{platformIdentifier}/Google Chrome for Testing.app/Contents/MacOS/Google Chrome for Testing",
        _ => $"chrome-{platformIdentifier}/chrome.exe",
    };

    private static JsonObject CreateVersionEntry(DownloadServer server, string version, string? channel, byte[]? chromeArchiveContent)
    {
        JsonArray chromeDownloads = [];
        JsonArray driverDownloads = [];
        foreach (string platformIdentifier in PlatformIdentifiers)
        {
            string driverFileName = platformIdentifier.StartsWith("win", StringComparison.Ordinal) ? "chromedriver.exe" : "chromedriver";
            server.AddFile(ChromeArchivePath(version, platformIdentifier), chromeArchiveContent ?? TestArchives.Zip(ChromeExecutablePath(platformIdentifier)));
            server.AddFile(DriverArchivePath(version, platformIdentifier), TestArchives.Zip($"chromedriver-{platformIdentifier}/{driverFileName}"));
            chromeDownloads.Add(new JsonObject() { ["platform"] = platformIdentifier, ["url"] = server.UrlFor(ChromeArchivePath(version, platformIdentifier)).AbsoluteUri });
            driverDownloads.Add(new JsonObject() { ["platform"] = platformIdentifier, ["url"] = server.UrlFor(DriverArchivePath(version, platformIdentifier)).AbsoluteUri });
        }

        JsonObject entry = new()
        {
            ["version"] = version,
            ["revision"] = "1000000",
            ["downloads"] = new JsonObject() { ["chrome"] = chromeDownloads, ["chromedriver"] = driverDownloads },
        };
        if (channel is not null)
        {
            entry["channel"] = channel;
        }

        return entry;
    }
}
