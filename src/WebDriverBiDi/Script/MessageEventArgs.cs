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
    private readonly string channelId;
    private readonly RemoteValue data;
    private readonly Source source;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
    /// </summary>
    /// <param name="channelId">The ID of the channel used for this message.</param>
    /// <param name="data">The data for this message.</param>
    /// <param name="source">The source for this message.</param>
    [JsonConstructor]
    public MessageEventArgs(string channelId, RemoteValue data, Source source)
    {
        this.channelId = channelId;
        this.data = data;
        this.source = source;
    }

    /// <summary>
    /// Gets the ID of the channel used for this message.
    /// </summary>
    [JsonPropertyName("channel")]
    public string ChannelId => this.channelId;

    /// <summary>
    /// Gets the data for this message.
    /// </summary>
    [JsonPropertyName("data")]
    public RemoteValue Data => this.data;

    /// <summary>
    /// Gets the source for this message.
    /// </summary>
    [JsonPropertyName("source")]
    public Source Source => this.source;
}
