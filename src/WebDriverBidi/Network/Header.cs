// <copyright file="Header.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// A header from a request.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Header
{
    private string name = string.Empty;
    private BytesValue value = new(BytesValueType.String, string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class.
    /// </summary>
    internal Header()
    {
    }

    /// <summary>
    /// Gets the name of the header.
    /// </summary>
    [JsonProperty("name")]
    [JsonRequired]
    public string Name { get => this.name; internal set => this.name = value; }

    /// <summary>
    /// Gets the value of the header.
    /// </summary>
    [JsonProperty("value")]
    [JsonRequired]
    public BytesValue Value { get => this.value; internal set => this.value = value; }
}