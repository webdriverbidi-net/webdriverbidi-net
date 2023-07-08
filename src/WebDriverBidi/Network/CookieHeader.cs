// <copyright file="CookieHeader.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// A header from a request.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CookieHeader
{
    private string name = string.Empty;
    private BytesValue value = new(BytesValueType.String, string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="CookieHeader"/> class.
    /// </summary>
    [JsonConstructor]
    public CookieHeader()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CookieHeader"/> class with the specified name and string value.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The string value of the header.</param>
    public CookieHeader(string name, string value)
    {
        this.name = name;
        this.value = new BytesValue(BytesValueType.String, value);
    }

    /// <summary>
    /// Gets or sets the name of the header.
    /// </summary>
    [JsonProperty("name")]
    [JsonRequired]
    public string Name { get => this.name; set => this.name = value; }

    /// <summary>
    /// Gets or sets the value of the header.
    /// </summary>
    [JsonProperty("value")]
    [JsonRequired]
    public BytesValue Value { get => this.value; set => this.value = value; }
}