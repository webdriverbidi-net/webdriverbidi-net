// <copyright file="CapabilitiesRequest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the capabilities requested for a session.
/// </summary>
public class CapabilitiesRequest
{
    /// <summary>
    /// Gets or sets the set of capabilities that must be matched to create a new session.
    /// </summary>
    [JsonPropertyName("alwaysMatch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public CapabilityRequest? AlwaysMatch { get; set; }

    /// <summary>
    /// Gets or sets the list of sets of capabilities any of which may be matched to create a new session.
    /// </summary>
    [JsonPropertyName("firstMatch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public List<CapabilityRequest>? FirstMatch { get; set; }
}
