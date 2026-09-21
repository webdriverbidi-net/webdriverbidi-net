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
    /// Gets a sentinel value indicating that there should be no limit on
    /// the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    public const long InfiniteMaxDomDepth = -1;

    /// <summary>
    /// Gets or sets the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Valid values for this property are greater than or equal to zero (or <see cref="InfiniteMaxDomDepth"/>
    /// for no limit). This property does not validate its value. A value of zero or greater is sent as-is; any
    /// negative value is sent as JSON <c>null</c>, which the protocol reads as no limit, so a negative value
    /// other than the sentinel asks for the same thing rather than being rejected.
    /// </para>
    /// <para>
    /// Any negative value means no limit, but only <see cref="InfiniteMaxDomDepth"/> is declared as the
    /// sentinel of the <see cref="SpecRangeAttribute"/>, so tooling reports another negative value as out
    /// of range. That is deliberate, and matches how every other resettable numeric property in the
    /// library is declared: the named sentinel is the supported way to ask for no limit.
    /// </para>
    /// </remarks>
    [JsonPropertyName("maxDomDepth")]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity, HasSentinel = true, SentinelValue = InfiniteMaxDomDepth)]
    public long? MaxDomDepth { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth when serializing script objects from script execution.
    /// </summary>
    [JsonPropertyName("maxObjectDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? MaxObjectDepth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating which shadow trees to serialize when serializing nodes from script execution.
    /// </summary>
    [JsonPropertyName("includeShadowTree")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IncludeShadowTreeSerializationOption? IncludeShadowTree { get; set; }
}
