// <copyright file="BrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Base class for browser locator settings, which define the properties and methods
/// needed to locate and download a specific browser for testing.
/// </summary>
public abstract class BrowserLocatorSettings
{
    /// <summary>
    /// Gets the base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    protected static readonly string BaseCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome", "firefox").
    /// </summary>
    public abstract string BrowserName { get; }

    /// <summary>
    /// Gets or sets the channel of the browser (e.g., "stable", "beta", "dev", "nightly").
    /// </summary>
    public string Channel { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the browser, which is used for logging and user-facing messages.
    /// </summary>
    public string BrowserDisplayName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    public string Version { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the environment variable that can be used to override the browser executable path.
    /// </summary>
    public string EnvironmentVariableName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected path to the browser executable relative to the installation directory.
    /// </summary>
    public string ExpectedExecutablePath { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the installer file.
    /// </summary>
    public string InstallerFileName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the version specified in the locator settings
    /// is the most recently published version for the channel.
    /// </summary>
    public bool IsLatestChannelVersion { get; protected set; } = false;

    /// <summary>
    /// Gets or sets the browser extractor used to extract the browser from the installer.
    /// Defaults to a ZipFileBrowserExtractor, but can be overridden for browsers that use
    /// different installer formats (e.g., DMG, self-extracting EXE, tarball).
    /// </summary>
    public BrowserExtractor Extractor { get; protected set; } = new ZipFileBrowserExtractor();

    /// <summary>
    /// Gets the browser download information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public abstract Task<BrowserDownloadInfo> GetBrowserDownloadInfo();
}
