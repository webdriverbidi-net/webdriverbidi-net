// <copyright file="RemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object representing a remote value in the browser.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record RemoteValue
{
    private static readonly List<string> KnownRemoteValueTypes =
    [
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
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValue"/> class.
    /// </summary>
    /// <param name="valueType">The string describing the type of this RemoteValue.</param>
    [JsonConstructor]
    internal RemoteValue(string valueType)
    {
        this.Type = valueType;
    }

    /// <summary>
    /// Gets the type of this RemoteValue.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; internal set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    [JsonPropertyName("handle")]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    [JsonPropertyName("internalId")]
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Gets the shared ID of this RemoteValue.
    /// </summary>
    [JsonPropertyName("sharedId")]
    public string? SharedId { get; internal set; }

    /// <summary>
    /// Gets the object that contains the value of this RemoteValue.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this RemoteValue has a value.
    /// </summary>
    [JsonIgnore]
    public bool HasValue => this.Value is not null;

    /// <summary>
    /// Gets a value indicating whether this RemoteValue contains a primitive value.
    /// </summary>
    [JsonIgnore]
    internal bool IsPrimitive => this.Type == "string" || this.Type == "number" || this.Type == "boolean" || this.Type == "bigint" || this.Type == "null" || this.Type == "undefined";

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
    /// <remarks>
    /// <para>
    ///   The valid types that a value can be assigned to are:
    ///   <list type="bulletted">
    ///     <item>
    ///       <description>
    ///         <see cref="string"/> for values returning strings
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="double"/> for values returning floating point numbers
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="long"/> for values returning integers
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="bool"/> for values returning <see langword="true"/> or <see langword="false"/>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="BigInteger"/> for large integer values exceeding <see cref="long.MaxValue"/>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="DateTime"/> for values returning date objects
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RegularExpressionValue"/> for values returning regular expression objects
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="NodeProperties"/> for values returning DOM nodes
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="WindowProxyProperties"/> for values returning references to a JavaScript window object
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RemoteValueList"/> for values returning arrays, sets, node lists, or HTML element collections
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RemoteValueDictionary"/> for values returning objects with key-value pairs, including objects and maps
    ///       </description>
    ///     </item>
    ///   </list>
    /// </para>
    /// <para>
    ///   If the value of this RemoteValue is <see langword="null"/>, then this method will
    ///   return <see langword="null"/> for reference types. This method will also return
    ///   <see langword="null"/> if the remote value being represented is null or undefined.
    ///   However, it will throw a <see cref="WebDriverBiDiException"/> for value types since
    ///   they cannot be assigned a <see langword="null"/> value.
    /// </para>
    /// <para>
    ///   If the value of this RemoteValue represents a DOM node, this method can be used to
    ///   convert it to a <see cref="NodeProperties"/> instance. To use the returned value as
    ///   an argument to a subsequent call to the remote end, you should use the
    ///   <see cref="ToSharedReference"/> method to convert this <see cref="RemoteValue"/> to a
    ///   <see cref="SharedReference"/>.
    /// </para>
    /// <para>
    ///   If the value of this RemoteValue is <see langword="null"/>, then this method will
    ///   return <see langword="null"/> for reference types. This method will also return
    ///   <see langword="null"/> if the remote value being represented is null or undefined.
    ///   However, it will throw a <see cref="WebDriverBiDiException"/> for value types since
    ///   they cannot be assigned a <see langword="null"/> value.
    /// </para>
    /// <para>
    ///   For non-primitive <see cref="RemoteValue"/> instances that do not directly map to
    ///   one of the types listed above, this method will return <see langword="null"/>. To
    ///   use the returned value as an argument to a subsequent call to the remote end, you
    ///   should use the <see cref="ToRemoteReference"/> method to convert this
    ///   <see cref="RemoteValue"/> to a <see cref="RemoteReference"/>.
    /// </para>
    /// </remarks>
    public T? ValueAs<T>()
    {
        T? result = default;
        Type type = typeof(T);
        if (this.Value == null)
        {
            if (type.IsValueType)
            {
                throw new WebDriverBiDiException("RemoteValue has null value, but desired type is a value type");
            }
        }
        else if (!type.IsInstanceOfType(this.Value))
        {
            throw new WebDriverBiDiException("RemoteValue could not be cast to the desired type");
        }
        else
        {
            result = (T)this.Value;
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

        if (this.Type == "node")
        {
            if (this.SharedId is null)
            {
                throw new WebDriverBiDiException("Node remote values must have a valid shared ID to be used as remote references");
            }

            return new SharedReference(this.SharedId) { Handle = this.Handle };
        }

        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("Remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.Handle) { SharedId = this.SharedId };
    }

    /// <summary>
    /// Converts this RemoteValue to a SharedReference.
    /// </summary>
    /// <returns>The SharedReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RemoteValue cannot be converted to a SharedReference.</exception>
    public SharedReference ToSharedReference()
    {
        if (this.ToRemoteReference() is not SharedReference reference)
        {
            throw new WebDriverBiDiException("Remote value cannot be converted to SharedReference");
        }

        return reference;
    }
}
