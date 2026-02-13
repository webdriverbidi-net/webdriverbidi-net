// <copyright file="DownloadWillBeginEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for for the browsingContext.downloadWillBegin event.
/// </summary>
public record DownloadWillBeginEventArgs : NavigationEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadWillBeginEventArgs" /> class.
    /// </summary>
    [JsonConstructor]
    public DownloadWillBeginEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the suggested file name of the item to be downloaded.
    /// </summary>
    [JsonPropertyName("suggestedFileName")]
    [JsonRequired]
    [JsonInclude]
    public string SuggestedFileName { get; internal set; } = string.Empty;
}