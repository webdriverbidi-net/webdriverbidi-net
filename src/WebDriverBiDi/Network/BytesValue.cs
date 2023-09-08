// <copyright file="BytesValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text;
using Newtonsoft.Json;

/// <summary>
/// The abstract base class for a value that can contain either a string or a byte array.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BytesValue
{
    private BytesValueType valueType;
    private string actualValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BytesValue"/> class.
    /// </summary>
    /// <param name="valueType">The type of value to initialize.</param>
    /// <param name="actualValue">The value to use in the object.</param>
    [JsonConstructor]
    internal BytesValue(BytesValueType valueType, string actualValue)
    {
        this.valueType = valueType;
        this.actualValue = actualValue;
    }

    /// <summary>
    /// Gets the type of the value object.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public BytesValueType Type { get => this.valueType; internal set => this.valueType = value; }

    /// <summary>
    /// Gets the value of the value object.
    /// </summary>
    [JsonProperty("value")]
    [JsonRequired]
    public string Value { get => this.actualValue; internal set => this.actualValue = value; }

    /// <summary>
    /// Gets the value of the value object as an array of bytes.
    /// </summary>
    [JsonIgnore]
    public byte[] ValueAsByteArray
    {
        get
        {
            if (this.valueType == BytesValueType.String)
            {
                return Encoding.UTF8.GetBytes(this.actualValue);
            }

            return Convert.FromBase64String(this.actualValue);
        }
    }

    /// <summary>
    /// Creates a BytesValue object from a string value.
    /// </summary>
    /// <param name="stringValue">The string value the BytesValue contains.</param>
    /// <returns>The BytesValue representing the string.</returns>
    public static BytesValue FromString(string stringValue)
    {
        return new BytesValue(BytesValueType.String, stringValue);
    }

    /// <summary>
    /// Creates a BytesValue object from a base64-encoded string value.
    /// </summary>
    /// <param name="base64Value">The value of the BytesValue as a base64-encoded string.</param>
    /// <returns>The BytesValue representing the value.</returns>
    public static BytesValue FromBase64String(string base64Value)
    {
        return new BytesValue(BytesValueType.Base64, base64Value);
    }

    /// <summary>
    /// Creates a BytesValue object containing a base64-encoded string from an array of bytes.
    /// </summary>
    /// <param name="bytes">The value of the BytesValue as a byte array.</param>
    /// <returns>The BytesValue representing the value.</returns>
    public static BytesValue FromByteArray(byte[] bytes)
    {
        return new BytesValue(BytesValueType.Base64, Convert.ToBase64String(bytes));
    }
}