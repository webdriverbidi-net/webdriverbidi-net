// <copyright file="BrowserDownloadProgress.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// The progress of a browser or driver download, reported through <see cref="BrowserDownloadOptions.Progress"/>.
/// </summary>
public sealed class BrowserDownloadProgress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserDownloadProgress"/> class.
    /// </summary>
    /// <param name="name">The name and version of what is being downloaded.</param>
    /// <param name="bytesReceived">The number of bytes received so far.</param>
    /// <param name="totalBytes">The total number of bytes to download, or <see langword="null"/> if the server did not say.</param>
    public BrowserDownloadProgress(string name, long bytesReceived, long? totalBytes)
    {
        this.Name = name;
        this.BytesReceived = bytesReceived;
        this.TotalBytes = totalBytes;
    }

    /// <summary>
    /// Gets the name and version of what is being downloaded, such as "Chrome Stable 130.0.6723.58".
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the number of bytes received so far.
    /// </summary>
    public long BytesReceived { get; }

    /// <summary>
    /// Gets the total number of bytes to download, or <see langword="null"/> if the server did not say.
    /// </summary>
    public long? TotalBytes { get; }
}
