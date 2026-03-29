// <copyright file="CollectionRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a list-like object, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for
/// script execution on the remote end.
/// </summary>
public record CollectionRemoteValue : RemoteValue, IObjectReferenceRemoteValue, ITypeSafeRemoteValue<RemoteValueList?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal CollectionRemoteValue()
    {
        this.Type = RemoteValueType.Array;
    }

    /// <summary>
    /// Gets the RemoteValueList containing the values of this remote value.
    /// </summary>
    /// <remarks>
    /// This value may be null if the remote value was deserialized without a value property.
    /// </remarks>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(RemoteValueListJsonConverter))]
    public RemoteValueList? Value { get; internal set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    [JsonPropertyName("internalId")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the list-like value.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the Value property is null.</exception>
    public override LocalValue ToLocalValue()
    {
        if (this.Value is null)
        {
            throw new WebDriverBiDiException("Cannot convert CollectionRemoteValue to LocalValue when Value is null");
        }

        RemoteValueList originalList = this.Value;
        List<LocalValue> localValues = new();
        foreach (RemoteValue item in originalList)
        {
            localValues.Add(item.ToLocalValue());
        }

        if (this.Type == RemoteValueType.Set)
        {
            return LocalValue.Set(localValues);
        }

        // If type is "array", "htmlcollection", or "nodelist", create an array local value.
        return LocalValue.Array(localValues);
    }

    /// <summary>
    /// Converts this RemoteValue into a RemoteReference.
    /// </summary>
    /// <returns>The RemoteObjectReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the RemoteValue does not have a handle set.</exception>
    public RemoteObjectReference ToRemoteObjectReference()
    {
        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("Collection remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.Handle);
    }
}
