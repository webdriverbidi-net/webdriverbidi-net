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
public record KeyValuePairCollectionRemoteValue : ValueHoldingRemoteValue<RemoteValueDictionary>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairCollectionRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal KeyValuePairCollectionRemoteValue()
        : base(RemoteValueType.Object)
    {
    }

    /// <summary>
    /// Gets the RemoteValueDictionary containing the key-value pairs of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonConverter(typeof(RemoteValueDictionaryJsonConverter))]
    public override RemoteValueDictionary Value { get; internal set; } = new RemoteValueDictionary([]);

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
