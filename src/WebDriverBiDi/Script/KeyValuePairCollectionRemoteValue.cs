// <copyright file="KeyValuePairCollectionRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a key-value pair object from the remote end, providing
/// type-safe access to the dictionary containing the values and the ability to convert
/// to a local value for use as an argument for script execution on the remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record KeyValuePairCollectionRemoteValue : ValueHoldingRemoteValue<RemoteValueDictionary>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairCollectionRemoteValue"/> class.
    /// </summary>
    /// <param name="type">The type of the key-value pair object from the remote end.</param>
    /// <param name="value">The RemoteValueDictionary value holding the key-value pairs.</param>
    internal KeyValuePairCollectionRemoteValue(RemoteValueType type, RemoteValueDictionary value)
        : base(type)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the RemoteValueDictionary containing the key-value pairs of this remote value.
    /// </summary>
    public override RemoteValueDictionary Value { get; protected set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the key-value pair container.</returns>
    public override LocalValue ToLocalValue()
    {
        RemoteValueDictionary mapping = this.Value;
        Dictionary<object, LocalValue> dict = new();
        foreach (KeyValuePair<object, RemoteValue> entry in mapping)
        {
            object mappedKey = entry.Key;
            if (mappedKey is RemoteValue keyRemoteValue)
            {
                mappedKey = keyRemoteValue.ToLocalValue();
            }

            dict[mappedKey] = entry.Value.ToLocalValue();
        }

        if (this.Type == RemoteValueType.Map)
        {
            return LocalValue.Map(dict);
        }

        return LocalValue.Object(dict);
    }
}
