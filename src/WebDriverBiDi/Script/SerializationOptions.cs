// <copyright file="SerializationOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Options for serialization of script objects.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SerializationOptions
{
    private long? maxDomDepth;
    private long? maxObjectDepth;
    private IncludeShadowTreeSerializationOption? includeShadowTree;

    /// <summary>
    /// Gets or sets the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    [JsonProperty("maxDomDepth", NullValueHandling = NullValueHandling.Ignore)]
    public long? MaxDomDepth { get => this.maxDomDepth; set => this.maxDomDepth = value; }

    /// <summary>
    /// Gets or sets the maximum depth when serializing script objects from script execution.
    /// </summary>
    [JsonProperty("maxObjectDepth", NullValueHandling = NullValueHandling.Ignore)]
    public long? MaxObjectDepth { get => this.maxObjectDepth; set => this.maxObjectDepth = value; }

    /// <summary>
    /// Gets or sets a value indicating which shadow trees to serializes when serializing nodes from script execution.
    /// </summary>
    [JsonProperty("includeShadowTree", NullValueHandling = NullValueHandling.Ignore)]
    public IncludeShadowTreeSerializationOption? IncludeShadowTree { get => this.includeShadowTree; set => this.includeShadowTree = value; }
}