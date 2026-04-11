// <copyright file="DriverDownloadInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Represents information about a driver download, including the driver name, version, matched browser version, and download URL.
/// </summary>
public record DriverDownloadInfo
{
    /// <summary>
    /// Gets or sets the name of the driver (e.g., "chromedriver", "geckodriver").
    /// </summary>
    public string DriverName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the driver.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the browser that this driver is compatible with.
    /// </summary>
    public string BrowserVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL from which the driver can be directly downloaded.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the installer or archive file.
    /// </summary>
    public string InstallerFileName { get; set; } = string.Empty;
}
