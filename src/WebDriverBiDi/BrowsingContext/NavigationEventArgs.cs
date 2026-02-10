// <copyright file="NavigationEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Object containing event data for events raised during navigation.
/// </summary>
public record NavigationEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
    /// </summary>
    [JsonConstructor]
    public NavigationEventArgs()
    {
        this.EpochTimestamp = 0;
    }

    /// <summary>
    /// Gets the ID of the navigation operation.
    /// </summary>
    [JsonPropertyName("navigation")]
    [JsonRequired]
    [JsonInclude]
    public string? NavigationId { get; internal set; }

    /// <summary>
    /// Gets the ID of the browsing context being navigated.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the URL to which the browsing context is being navigated.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the navigation in UTC.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp { get; internal set; }

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
    public long EpochTimestamp
    {
        get;
        private set
        {
            field = value;
            this.Timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }
}
