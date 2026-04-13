// <copyright file="BrowserVersion.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Represents a browser version specification. This is an abstract base class with
/// factory methods to create specific version types.
/// </summary>
public abstract class BrowserVersion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserVersion"/> class.
    /// Prevents external instantiation. Use factory methods to create instances.
    /// </summary>
    private protected BrowserVersion()
    {
    }

    /// <summary>
    /// Gets a version specification indicating the latest available version for the specified release channel.
    /// When used with auto-download, this will download the most recent published version.
    /// </summary>
    public static BrowserVersion Latest { get; } = new LatestVersion();

    /// <summary>
    /// Gets a version specification indicating the system-installed version of the browser.
    /// This will use the browser installed at the default system location.
    /// </summary>
    public static BrowserVersion SystemInstalled { get; } = new SystemInstalledVersion();

    /// <summary>
    /// Gets the string representation of this version for internal use.
    /// </summary>
    internal abstract string Value { get; }

    /// <summary>
    /// Creates a version specification for a specific browser version number.
    /// </summary>
    /// <param name="version">The specific version number (e.g., "120.0.6099.109").</param>
    /// <returns>A <see cref="BrowserVersion"/> representing the specific version.</returns>
    /// <exception cref="ArgumentException">Thrown when the version string is null or empty.</exception>
    public static BrowserVersion Specific(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version string cannot be null or empty.", nameof(version));
        }

        return new SpecificVersion(version);
    }

    /// <summary>
    /// Represents the latest available version for a release channel.
    /// </summary>
    private sealed class LatestVersion : BrowserVersion
    {
        internal override string Value => BrowserLocatorSettings.LatestVersionString;

        public override string ToString() => "Latest";
    }

    /// <summary>
    /// Represents the system-installed version of the browser.
    /// </summary>
    private sealed class SystemInstalledVersion : BrowserVersion
    {
        internal override string Value => BrowserLocatorSettings.SystemVersionString;

        public override string ToString() => "System Installed";
    }

    /// <summary>
    /// Represents a specific version number.
    /// </summary>
    private sealed class SpecificVersion : BrowserVersion
    {
        private readonly string version;

        internal SpecificVersion(string version)
        {
            this.version = version;
        }

        internal override string Value => this.version;

        public override string ToString() => this.version;
    }
}
