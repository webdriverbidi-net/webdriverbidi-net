// <copyright file="DownloadCompleteEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the browsingContext.downloadEnd event when the download completed.
/// </summary>
public record DownloadCompleteEventArgs : DownloadEndEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadCompleteEventArgs" /> class.
    /// </summary>
    [JsonConstructor]
    public DownloadCompleteEventArgs()
        : base()
    {
        this.Status = DownloadEndStatus.Complete;
    }

    /// <summary>
    /// Gets the path of the downloaded file, or <see langword="null"/> when the remote end cannot supply one.
    /// </summary>
    [JsonPropertyName("filepath")]
    [JsonRequired]
    [JsonInclude]
    public string? FilePath { get; internal set; }
}
