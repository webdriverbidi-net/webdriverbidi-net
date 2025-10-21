// <copyright file="DownloadEndEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the browsingContext.downloadEnd event.
/// </summary>
public record DownloadEndEventArgs : NavigationEventArgs
{
    private string? filePath;
    private DownloadEndStatus status = DownloadEndStatus.Complete;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadEndEventArgs" /> class.
    /// </summary>
    [JsonConstructor]
    public DownloadEndEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the path of the downloaded item, if the download succeeded.
    /// </summary>
    [JsonPropertyName("filepath")]
    [JsonInclude]
    public string? FilePath { get => this.filePath; private set => this.filePath = value; }

    /// <summary>
    /// Gets the status of the download.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonInclude]
    public DownloadEndStatus Status { get => this.status; private set => this.status = value; }
}
