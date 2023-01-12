// <copyright file="RemoteValue.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object representing a remote value in the browser.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RemoteValueJsonConverter))]
public class RemoteValue
{
    private string valueType;
    private string? handle;
    private uint? internalId;
    private object? valueObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValue"/> class.
    /// </summary>
    /// <param name="valueType">The string describing the type of this RemoteValue.</param>
    internal RemoteValue(string valueType)
    {
        this.valueType = valueType;
    }

    /// <summary>
    /// Gets the type of thie RemoteValue.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get => this.valueType; internal set => this.valueType = value; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    [JsonProperty("handle")]
    public string? Handle { get => this.handle; internal set => this.handle = value; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    [JsonProperty("internalId")]
    public uint? InternalId { get => this.internalId; internal set => this.internalId = value; }

    /// <summary>
    /// Gets the object that contains the value of this RemoteValue.
    /// </summary>
    [JsonProperty("value")]
    public object? Value { get => this.valueObject; internal set => this.valueObject = value; }

    /// <summary>
    /// Gets a value indicating whether this RemoteValue has a value.
    /// </summary>
    public bool HasValue => this.valueObject is not null;

    /// <summary>
    /// Gets a value indicating whether this RemoteValue contains a primitive value.
    /// </summary>
    public bool IsPrimitive => this.valueType == "string" || this.valueType == "number" || this.valueType == "boolean" || this.valueType == "bigint" || this.valueType == "null" || this.valueType == "undefined";

    /// <summary>
    /// Gets the value of this RemoteValue cast to the desired type.
    /// </summary>
    /// <typeparam name="T">The type to which to cast the value object.</typeparam>
    /// <returns>The value cast to the desired type.</returns>
    /// <exception cref="WebDriverBidiException">Thrown if this RemoteValue cannot be cast to the desired type.</exception>
    public T? ValueAs<T>()
    {
        var result = default(T);
        Type type = typeof(T);
        if (this.valueObject == null)
        {
            if (type.IsValueType && (Nullable.GetUnderlyingType(type) == null))
            {
                throw new WebDriverBidiException("RemoteValue has null value, but desired type is a value type");
            }
        }
        else if (!type.IsInstanceOfType(this.valueObject))
        {
            throw new WebDriverBidiException("RemoteValue could not be cast to the desired type");
        }
        else
        {
            result = (T)this.valueObject;
        }

        return result;
    }
}