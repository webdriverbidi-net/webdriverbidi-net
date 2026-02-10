// <copyright file="PrefetchStatusUpdatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Speculation;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events the prefetch status of a resource is updated.
/// </summary>
public record PrefetchStatusUpdatedEventArgs : WebDriverBiDiEventArgs
{
    [JsonConstructor]
    internal PrefetchStatusUpdatedEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the prefetch status is updated.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the URL of the resource for which the prefetch status is updated..
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the value of the updated prefetch status.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonRequired]
    [JsonInclude]
    public PreloadingStatus Status { get; internal set; } = PreloadingStatus.Pending;
}
