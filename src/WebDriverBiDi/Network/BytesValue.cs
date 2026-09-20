// <copyright file="BytesValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a value that can contain either a string or a byte array.
/// </summary>
public record BytesValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BytesValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal BytesValue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BytesValue"/> class.
    /// </summary>
    /// <param name="type">The type of value to initialize.</param>
    /// <param name="value">The value to use in the object.</param>
    private BytesValue(BytesValueType type, string value)
    {
        this.Type = type;
        this.Value = value;
    }

    /// <summary>
    /// Gets the type of the value object.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public BytesValueType Type { get; internal set; } = BytesValueType.String;

    /// <summary>
    /// Gets the value of the value object.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    [JsonInclude]
    public string Value { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the value of the value object as an array of bytes.
    /// </summary>
    [JsonIgnore]
    public byte[] ValueAsByteArray
    {
        get
        {
            if (this.Type == BytesValueType.String)
            {
                return Encoding.UTF8.GetBytes(this.Value);
            }

            return Convert.FromBase64String(this.Value);
        }
    }

    /// <summary>
    /// Gets an empty <see cref="BytesValue"/> object.
    /// </summary>
    internal static BytesValue Empty => new();

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

    /// <summary>
    /// Prints the members of this value for <see cref="object.ToString"/>: its <see cref="Type"/> and <see cref="Value"/>.
    /// </summary>
    /// <param name="builder">The builder to print the members to.</param>
    /// <returns><see langword="true"/>, because members were printed.</returns>
    /// <remarks>
    /// <see cref="ValueAsByteArray"/> is deliberately not printed. It decodes the whole value each time it is read, which
    /// would make printing a large body expensive only to print the name of the array type, and it throws for a
    /// <see cref="BytesValueType.Base64"/> value that is not valid base64, which would make <see cref="object.ToString"/>
    /// throw. <see cref="Value"/> already shows the content.
    /// </remarks>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Type = ").Append(this.Type).Append(", Value = ").Append(this.Value);
        return true;
    }
}
