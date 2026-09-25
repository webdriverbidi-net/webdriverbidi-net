// <copyright file="IBrowserDownloadSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// The locator settings of a browser that is downloaded.
/// </summary>
internal interface IBrowserDownloadSource
{
    /// <summary>
    /// Gets the browser download information.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    Task<BrowserDownloadInfo> GetBrowserDownloadInfo(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the SHA-256 hash the browser's publisher lists for a download.
    /// </summary>
    /// <param name="downloadInfo">The download.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The hash in hexadecimal, or <see langword="null"/> if the publisher lists none.</returns>
    /// <exception cref="DownloadVerificationException">Thrown when the publisher lists hashes, but none for the download.</exception>
    Task<string?> GetBrowserSha256Async(BrowserDownloadInfo downloadInfo, CancellationToken cancellationToken);
}
