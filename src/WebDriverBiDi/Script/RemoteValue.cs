// <copyright file="RemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object representing a remote value in the browser.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public abstract record RemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValue"/> class.
    /// </summary>
    /// <param name="valueType">The string describing the type of this RemoteValue.</param>
    [JsonConstructor]
    internal RemoteValue(RemoteValueType valueType)
    {
        this.Type = valueType;
    }

    /// <summary>
    /// Gets the type of this RemoteValue.
    /// </summary>
    [JsonPropertyName("type")]
    public virtual RemoteValueType Type { get; internal set; }

    /// <summary>
    /// Converts this <see cref="RemoteValue"/> to a <see cref="LocalValue"/> for use with sending to the protocol remote end.
    /// </summary>
    /// <returns>A <see cref="LocalValue"/> representing the this <see cref="RemoteValue"/> value.</returns>
    public abstract LocalValue ToLocalValue();

    /// <summary>
    /// Attempts to convert this <see cref="RemoteValue"/> to the specified type-specific RemoteValue type.
    /// </summary>
    /// <typeparam name="T">The type-specific RemoteValue type to convert to.</typeparam>
    /// <param name="result">When this method returns, contains the converted value or null if the conversion failed.</param>
    /// <returns><see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryConvertTo<T>([NotNullWhen(true)] out T? result)
        where T : RemoteValue
    {
        if (this is T converted)
        {
            result = converted;
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Attempts to convert this <see cref="RemoteValue"/> to the specified type-specific RemoteValue type.
    /// </summary>
    /// <typeparam name="T">The type-specific RemoteValue type to convert to.</typeparam>
    /// <returns><see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RemoteValue cannot be converted to the specified type.</exception>
    public T ConvertTo<T>()
        where T : RemoteValue
    {
        if (this is T converted)
        {
            return converted;
        }

        throw new WebDriverBiDiException($"RemoteValue of type '{this.Type}' cannot be converted to type '{typeof(T).Name}'");
    }
}
