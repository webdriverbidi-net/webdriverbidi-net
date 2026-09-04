// <copyright file="ChannelValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// A channel, used as an argument to a script. Because a channel is a <see cref="LocalValue"/>, it can be
/// passed to <c>script.callFunction</c> as well as to a preload script.
/// </summary>
public record ChannelValue : LocalValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelValue"/> class.
    /// </summary>
    /// <param name="value">The properties for this ChannelValue.</param>
    public ChannelValue(ChannelProperties value)
        : base()
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the type of this ChannelValue.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "channel";

    /// <summary>
    /// Gets the value of this ChannelValue.
    /// </summary>
    [JsonPropertyName("value")]
    public ChannelProperties Value { get; }
}
