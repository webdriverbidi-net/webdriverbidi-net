// <copyright file="HistoryUpdatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Object containing event data for the browsingContext.historyUpdated event.
/// </summary>
public record HistoryUpdatedEventArgs : WebDriverBiDiEventArgs
{
    private string browsingContextId;
    private string url;
    private ulong epochTimestamp = 0;
    private DateTime timestamp = DateTimeUtilities.UnixEpoch;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryUpdatedEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context corresponding to the history item.</param>
    /// <param name="url">The URL of the history item.</param>
    [JsonConstructor]
    public HistoryUpdatedEventArgs(string browsingContextId, string url)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
    }

    /// <summary>
    /// Gets the ID of the browsing context in the history entry.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the URL of the history entry.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get => this.url; private set => this.url = value; }

    /// <summary>
    /// Gets the timestamp of the navigation in UTC.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
    public ulong EpochTimestamp
    {
        get
        {
            return this.epochTimestamp;
        }

        private set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }
}
