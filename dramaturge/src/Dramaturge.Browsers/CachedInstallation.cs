// <copyright file="CachedInstallation.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// A browser or driver version installed in the cache, as listed by <see cref="BrowserCache.List"/>.
/// </summary>
public sealed class CachedInstallation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CachedInstallation"/> class.
    /// </summary>
    /// <param name="name">The name of the browser or driver.</param>
    /// <param name="channel">The browser's release channel, or <see langword="null"/> for a driver.</param>
    /// <param name="version">The version.</param>
    /// <param name="directory">The directory the version is installed in.</param>
    /// <param name="size">The size of the installed files, in bytes.</param>
    /// <param name="lastResolved">When a request last resolved to the version, or <see langword="null"/> if none has.</param>
    internal CachedInstallation(string name, string? channel, string version, string directory, long size, DateTimeOffset? lastResolved)
    {
        this.Name = name;
        this.Channel = channel;
        this.Version = version;
        this.Directory = directory;
        this.Size = size;
        this.LastResolved = lastResolved;
    }

    /// <summary>
    /// Gets the name of the browser ("chrome", "chrome-headless-shell", or "firefox") or driver ("chromedriver",
    /// "geckodriver", or "msedgedriver").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the installation is a driver.
    /// </summary>
    public bool IsDriver => this.Channel is null;

    /// <summary>
    /// Gets the browser's release channel, in its own name (such as "stable", "canary", or "nightly"), or
    /// <see langword="null"/> for a driver, which is shared by every channel.
    /// </summary>
    public string? Channel { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the directory the version is installed in.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Gets the size of the installed files, in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets when a request for the latest version (of the channel, or of a milestone), or for the driver of a browser
    /// version, last resolved to this version, or <see langword="null"/> if none has, as for a version requested by number.
    /// </summary>
    public DateTimeOffset? LastResolved { get; }

    /// <summary>
    /// Returns the installation as "name@version".
    /// </summary>
    /// <returns>The installation as "name@version".</returns>
    public override string ToString() => $"{this.Name}@{this.Version}";
}
