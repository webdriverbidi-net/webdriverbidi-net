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
public record CollectionRemoteValue : ValueHoldingRemoteValue<RemoteValueList>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal CollectionRemoteValue()
        : base(RemoteValueType.Array)
    {
    }

    /// <summary>
    /// Gets the RemoteValueList containing the values of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonConverter(typeof(RemoteValueListJsonConverter))]
    public override RemoteValueList Value { get; internal set; } = new RemoteValueList([]);

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
    public override LocalValue ToLocalValue()
    {
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

        // If type is "array", "htmlcolletion", or "nodelist", create an array local value.
        return LocalValue.Array(localValues);
    }
}
