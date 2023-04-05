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
    private string? value;
    private byte[]? binaryValue;

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
    public string? Value { get => this.value; internal set => this.value = value; }

    /// <summary>
    /// Gets the binary value of a cookie as an array of bytes. Property
    /// is used when the cookie value cannot be expressed as a UTF-8 string.
    /// </summary>
    [JsonProperty("binaryValue", NullValueHandling = NullValueHandling.Ignore)]
    public byte[]? BinaryValue { get => this.binaryValue; internal set => this.binaryValue = value; }
}