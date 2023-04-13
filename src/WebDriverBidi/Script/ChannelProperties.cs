// <copyright file="ChannelProperties.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Properties of a channel used to initiate passing information back from the browser from a preload script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ChannelProperties
{
    private string channelId = string.Empty;
    private SerializationOptions? serializationOptions;
    private ResultOwnership? resultOwnership;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelProperties"/> class.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    public ChannelProperties(string channelId)
    {
        this.channelId = channelId;
    }

    /// <summary>
    /// Gets or sets the ID of the channel.
    /// </summary>
    [JsonProperty("channel")]
    public string ChannelId { get => this.channelId; set => this.channelId = value; }

    /// <summary>
    /// Gets or sets the serialization options for the channel.
    /// </summary>
    [JsonProperty("serializationOptions", NullValueHandling = NullValueHandling.Ignore)]
    public SerializationOptions? SerializationOptions { get => this.serializationOptions; set => this.serializationOptions = value; }

    /// <summary>
    /// Gets or sets the result ownership for the channel.
    /// </summary>
    [JsonProperty("resultOwnership", NullValueHandling = NullValueHandling.Ignore)]
    public ResultOwnership? ResultOwnership { get => this.resultOwnership; set => this.resultOwnership = value; }
}