// <copyright file="HistoryUpdatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the browsingContext.historyUpdated event.
/// </summary>
public class HistoryUpdatedEventArgs : WebDriverBiDiEventArgs
{
    private string browsingContextId;

    private string url;

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
}
