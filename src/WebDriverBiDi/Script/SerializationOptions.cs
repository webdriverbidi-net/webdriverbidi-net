// <copyright file="SerializationOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Options for serialization of script objects.
/// </summary>
public class SerializationOptions
{
    private long? maxDomDepth;
    private long? maxObjectDepth;
    private IncludeShadowTreeSerializationOption? includeShadowTree;

    /// <summary>
    /// Gets or sets the maximum depth when serializing DOM nodes from script execution.
    /// </summary>
    [JsonPropertyName("maxDomDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxDomDepth { get => this.maxDomDepth; set => this.maxDomDepth = value; }

    /// <summary>
    /// Gets or sets the maximum depth when serializing script objects from script execution.
    /// </summary>
    [JsonPropertyName("maxObjectDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxObjectDepth { get => this.maxObjectDepth; set => this.maxObjectDepth = value; }

    /// <summary>
    /// Gets or sets a value indicating which shadow trees to serializes when serializing nodes from script execution.
    /// </summary>
    [JsonPropertyName("includeShadowTree")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IncludeShadowTreeSerializationOption? IncludeShadowTree { get => this.includeShadowTree; set => this.includeShadowTree = value; }
}
