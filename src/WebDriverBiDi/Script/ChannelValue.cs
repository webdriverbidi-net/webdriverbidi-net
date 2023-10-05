// <copyright file="ChannelValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Value to be used as argument to a preload script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ChannelValue : ArgumentValue
{
    private readonly string type = "channel";
    private readonly ChannelProperties value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelValue"/> class.
    /// </summary>
    /// <param name="value">The properties for this ChannelValue.</param>
    public ChannelValue(ChannelProperties value)
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the type of this ChannelValue.
    /// </summary>
    [JsonProperty("type")]
    public string Type => this.type;

    /// <summary>
    /// Gets the value of this ChannelValue.
    /// </summary>
    [JsonProperty("value")]
    public ChannelProperties Value => this.value;
}
