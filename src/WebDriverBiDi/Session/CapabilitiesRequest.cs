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
    public CapabilityRequest? AlwaysMatch { get; set; }

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session.
    /// </summary>
    /// <remarks>
    /// The WebDriver BiDi specification states that the <c>firstMatch</c> property is optional, and, when present,
    /// can contain zero or more members. However, the remote end algorithm for processing the new session command
    /// delegates capability processing to the WebDriver Classic specification's capabilities handling, which
    /// returns an error if <c>firstMatch</c> is an empty array. This property will not be sent with the payload
    /// if it contains no <see cref="CapabilityRequest"/> entries.
    /// </remarks>
    [JsonIgnore]
    public List<CapabilityRequest> FirstMatch { get; } = [];

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session for serialization purposes.
    /// </summary>
    [JsonPropertyName("firstMatch")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal List<CapabilityRequest>? SerializableFirstMatch => this.FirstMatch.Count == 0 ? null : this.FirstMatch;
}
