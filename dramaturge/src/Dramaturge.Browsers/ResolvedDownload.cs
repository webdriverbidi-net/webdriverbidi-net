// <copyright file="ResolvedDownload.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// The build of a browser or driver that a request resolves to, as returned by
/// <see cref="BrowserLocator.ResolveDownloadAsync"/> and <see cref="DriverLocator.ResolveDownloadAsync"/>.
/// </summary>
public sealed class ResolvedDownload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResolvedDownload"/> class.
    /// </summary>
    /// <param name="name">The name of the browser or driver.</param>
    /// <param name="version">The version.</param>
    /// <param name="url">The URL the build is downloaded from.</param>
    /// <param name="isCached">A value indicating whether the build is already in the cache.</param>
    internal ResolvedDownload(string name, string version, Uri url, bool isCached)
    {
        this.Name = name;
        this.Version = version;
        this.Url = url;
        this.IsCached = isCached;
    }

    /// <summary>
    /// Gets the name of the browser ("chrome", "chrome-headless-shell", or "firefox") or driver ("chromedriver",
    /// "geckodriver", or "msedgedriver"), as <see cref="CachedInstallation.Name"/> names it once installed.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the URL the build is downloaded from.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets a value indicating whether the build is already in the cache, so that locating it downloads nothing.
    /// </summary>
    public bool IsCached { get; }

    /// <summary>
    /// Returns the build as "name@version".
    /// </summary>
    /// <returns>The build as "name@version".</returns>
    public override string ToString() => $"{this.Name}@{this.Version}";
}
