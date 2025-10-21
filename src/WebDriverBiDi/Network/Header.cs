// <copyright file="Header.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// A header from a request.
/// </summary>
public class Header
{
    private string name = string.Empty;
    private BytesValue value = BytesValue.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class.
    /// </summary>
    [JsonConstructor]
    public Header()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class with the specified name and string value.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The string value of the header.</param>
    public Header(string name, string value)
    {
        this.name = name;
        this.value = BytesValue.FromString(value);
    }

    /// <summary>
    /// Gets or sets the name of the header.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public string Name { get => this.name; set => this.name = value; }

    /// <summary>
    /// Gets or sets the value of the header.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    public BytesValue Value { get => this.value; set => this.value = value; }
}
