// <copyright file="FileDownloadProgressEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Provides data for the file download progress event.
/// </summary>
public record FileDownloadProgressEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloadProgressEventArgs"/> class.
    /// </summary>
    /// <param name="percentComplete">The percentage of the download that is complete.</param>
    public FileDownloadProgressEventArgs(int percentComplete)
    {
        this.PercentComplete = percentComplete;
    }

    /// <summary>
    /// Gets the percentage of the download that is complete.
    /// </summary>
    public int PercentComplete { get; }
}
