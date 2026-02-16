// <copyright file="ClientHintsMetadata.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the overriding client hints for the browser.
/// </summary>
public class ClientHintsMetadata
{
    /// <summary>
    /// Gets or sets the list of brands for overriding client hints.
    /// </summary>
    [JsonPropertyName("brands")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BrandVersion>? Brands { get; set; }

    /// <summary>
    /// Gets or sets the list of full versions for overriding client hints.
    /// </summary>
    [JsonPropertyName("fullVersionList")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BrandVersion>? FullVersionList { get; set; }

    /// <summary>
    /// Gets or sets the platform for overriding client hints.
    /// </summary>
    [JsonPropertyName("platform")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Platform { get; set; }

    /// <summary>
    /// Gets or sets the platform version for overriding client hints.
    /// </summary>
    [JsonPropertyName("platformVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlatformVersion { get; set; }

    /// <summary>
    /// Gets or sets the architecture for overriding client hints.
    /// </summary>
    [JsonPropertyName("architecture")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Architecture { get; set; }

    /// <summary>
    /// Gets or sets the model for overriding client hints.
    /// </summary>
    [JsonPropertyName("model")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the overridden client hints indicate a mobile user agent.
    /// </summary>
    [JsonPropertyName("mobile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Mobile { get; set; }

    /// <summary>
    /// Gets or sets the bit-ness for overriding client hints.
    /// </summary>
    [JsonPropertyName("bitness")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Bitness { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the overridden client hints indicate a WOW64 user agent.
    /// </summary>
    [JsonPropertyName("wow64")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Wow64 { get; set; }

    /// <summary>
    /// Gets or sets the list of form factors for overriding client hints.
    /// </summary>
    [JsonPropertyName("formFactors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FormFactors { get; set; }
}
