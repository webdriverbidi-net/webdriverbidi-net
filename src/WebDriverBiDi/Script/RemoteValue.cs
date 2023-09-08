// <copyright file="RemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object representing a remote value in the browser.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RemoteValueJsonConverter))]
public class RemoteValue
{
    private static readonly List<string> KnownRemoteValueTypes = new()
    {
        "undefined",
        "null",
        "string",
        "number",
        "boolean",
        "bigint",
        "symbol",
        "array",
        "object",
        "function",
        "regexp",
        "date",
        "map",
        "set",
        "weakmap",
        "weakset",
        "iterator",
        "generator",
        "error",
        "proxy",
        "promise",
        "typedarray",
        "arraybuffer",
        "nodelist",
        "htmlcollection",
        "node",
        "window",
    };

    private string valueType;
    private string? handle;
    private ulong? internalId;
    private object? valueObject;
    private string? sharedId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValue"/> class.
    /// </summary>
    /// <param name="valueType">The string describing the type of this RemoteValue.</param>
    internal RemoteValue(string valueType)
    {
        this.valueType = valueType;
    }

    /// <summary>
    /// Gets the type of this RemoteValue.
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
    public ulong? InternalId { get => this.internalId; internal set => this.internalId = value; }

    /// <summary>
    /// Gets the shared ID of this RemoteValue.
    /// </summary>
    [JsonProperty("sharedId")]
    public string? SharedId { get => this.sharedId; internal set => this.sharedId = value; }

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
    internal bool IsPrimitive => this.valueType == "string" || this.valueType == "number" || this.valueType == "boolean" || this.valueType == "bigint" || this.valueType == "null" || this.valueType == "undefined";

    /// <summary>
    /// Gets a value indicating whether the specified type is valid for creating a RemoteValue.
    /// </summary>
    /// <param name="type">The type to check for validity.</param>
    /// <returns><see langword="true" /> if the value is valid for creating a RemoteValue; otherwise, <see langword="false" />.</returns>
    public static bool IsValidRemoteValueType(string type)
    {
        return KnownRemoteValueTypes.Contains(type);
    }

    /// <summary>
    /// Gets the value of this RemoteValue cast to the desired type.
    /// </summary>
    /// <typeparam name="T">The type to which to cast the value object.</typeparam>
    /// <returns>The value cast to the desired type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RemoteValue cannot be cast to the desired type.</exception>
    public T? ValueAs<T>()
    {
        T? result = default;
        Type type = typeof(T);
        if (this.valueObject == null)
        {
            if (type.IsValueType)
            {
                throw new WebDriverBiDiException("RemoteValue has null value, but desired type is a value type");
            }
        }
        else if (!type.IsInstanceOfType(this.valueObject))
        {
            throw new WebDriverBiDiException("RemoteValue could not be cast to the desired type");
        }
        else
        {
            result = (T)this.valueObject;
        }

        return result;
    }

    /// <summary>
    /// Converts this RemoteValue into a RemoteReference.
    /// </summary>
    /// <returns>The RemoteReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the RemoteValue meets one of the following conditions:
    /// <list type="bulleted">
    ///   <item>
    ///     <description>
    ///       The RemoteValue is a primitive value (string, number, boolean, bigint, null, or undefined)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The RemoteValue has a type of "node", but there is no shared ID set
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The RemoteValue does not have a handle set
    ///     </description>
    ///   </item>
    /// </list>
    /// </exception>
    public RemoteReference ToRemoteReference()
    {
        if (this.IsPrimitive)
        {
            throw new WebDriverBiDiException("Primitive values cannot be used as remote references");
        }

        if (this.valueType == "node")
        {
            if (this.sharedId is null)
            {
                throw new WebDriverBiDiException("Node remote values must have a valid shared ID to be used as remote references");
            }

            return new SharedReference(this.sharedId) { Handle = this.handle };
        }

        if (this.handle is null)
        {
            throw new WebDriverBiDiException("Remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.handle) { SharedId = this.sharedId };
    }

    /// <summary>
    /// Converts this RemoteReference to a SharedReference.
    /// </summary>
    /// <returns>The SharedReference object representing this RemoteValue.</returns>
    public SharedReference ToSharedReference()
    {
        if (this.ToRemoteReference() is not SharedReference reference)
        {
            throw new WebDriverBiDiException("Remote value cannot be converted to SharedReference");
        }

        return reference;
    }
}