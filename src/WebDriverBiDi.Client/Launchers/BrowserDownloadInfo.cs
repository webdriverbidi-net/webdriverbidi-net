// <copyright file="BrowserDownloadInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Represents information about a browser download, including the browser name, channel, version, and download URL.
/// </summary>
public record BrowserDownloadInfo
{
    /// <summary>
    /// Gets or sets the name of the browser (e.g., "chrome", "firefox").
    /// </summary>
    public string BrowserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel of the browser (e.g., "stable", "beta", "dev", "nightly").
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL from which the browser installer can be directly downloaded.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore version matching when using cached browser installations.
    /// If true, the locator will delete any cached installation and redownload the browser, even if the version matches.
    /// </summary>
    public bool IgnoreVersionMatch { get; set; } = false;
}
