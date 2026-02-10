// <copyright file="MessageEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the event raised when a preload script sends a message to the client.
/// </summary>
public record MessageEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
    /// </summary>
    /// <param name="channelId">The ID of the channel used for this message.</param>
    /// <param name="data">The data for this message.</param>
    /// <param name="source">The source for this message.</param>
    [JsonConstructor]
    public MessageEventArgs(string channelId, RemoteValue data, Source source)
    {
        this.ChannelId = channelId;
        this.Data = data;
        this.Source = source;
    }

    /// <summary>
    /// Gets the ID of the channel used for this message.
    /// </summary>
    [JsonPropertyName("channel")]
    [JsonInclude]
    [JsonRequired]
    public string ChannelId { get; internal set; }

    /// <summary>
    /// Gets the data for this message.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonInclude]
    [JsonRequired]
    public RemoteValue Data { get; internal set; }

    /// <summary>
    /// Gets the source for this message.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonInclude]
    [JsonRequired]
    public Source Source { get; internal set; }
}
