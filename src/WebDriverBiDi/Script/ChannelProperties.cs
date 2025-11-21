// <copyright file="ChannelProperties.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Properties of a channel used to initiate passing information back from the browser from a preload script.
/// </summary>
public class ChannelProperties
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelProperties"/> class.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    public ChannelProperties(string channelId)
    {
        this.ChannelId = channelId;
    }

    /// <summary>
    /// Gets or sets the ID of the channel.
    /// </summary>
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialization options for the channel.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public SerializationOptions? SerializationOptions { get; set; }

    /// <summary>
    /// Gets or sets the result ownership for the channel.
    /// </summary>
    [JsonPropertyName("resultOwnership")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public ResultOwnership? ResultOwnership { get; set; }
}
