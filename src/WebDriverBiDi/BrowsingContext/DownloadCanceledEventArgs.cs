// <copyright file="DownloadCanceledEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the browsingContext.downloadEnd event when the download was canceled.
/// A canceled download has no file path.
/// </summary>
public record DownloadCanceledEventArgs : DownloadEndEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadCanceledEventArgs" /> class.
    /// </summary>
    [JsonConstructor]
    public DownloadCanceledEventArgs()
        : base()
    {
        this.Status = DownloadEndStatus.Canceled;
    }
}
