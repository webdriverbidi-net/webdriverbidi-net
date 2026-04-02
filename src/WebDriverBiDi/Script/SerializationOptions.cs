// <copyright file="SerializationOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Options for serialization of script objects.
/// </summary>
public class SerializationOptions
{
    /// <summary>
    /// Gets a sentinal value indicating that there should be no limit on
    /// the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    public static readonly long InfiniteMaxDomDepth = -1;

    /// <summary>
    /// Gets or sets the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    [JsonPropertyName("maxDomDepth")]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxDomDepth { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth when serializing script objects from script execution.
    /// </summary>
    [JsonPropertyName("maxObjectDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxObjectDepth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating which shadow trees to serialize when serializing nodes from script execution.
    /// </summary>
    [JsonPropertyName("includeShadowTree")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IncludeShadowTreeSerializationOption? IncludeShadowTree { get; set; }
}
