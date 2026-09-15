// <copyright file="CapabilityRequest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing capabilities requested when starting a new session.
/// </summary>
public class CapabilityRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether the browser should accept insecure (self-signed) SSL certificates.
    /// </summary>
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AcceptInsecureCerts { get; set; }

    /// <summary>
    /// Gets or sets the name of the browser.
    /// </summary>
    [JsonPropertyName("browserName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowserName { get; set; }

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    [JsonPropertyName("browserVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// Gets or sets the platform name.
    /// </summary>
    [JsonPropertyName("platformName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlatformName { get; set; }

    /// <summary>
    /// Gets or sets the proxy to use with this session.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProxyConfiguration? Proxy { get; set; }

    /// <summary>
    /// Gets or sets the behavior of the session for handling user prompts.
    /// </summary>
    [JsonPropertyName("unhandledPromptBehavior")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandler? UnhandledPromptBehavior { get; set; }

    /// <summary>
    /// Gets the dictionary containing additional capabilities to use with this session.
    /// </summary>
    /// <remarks>
    /// An entry may not reuse the name of a property this type already serializes; doing so would write that name
    /// twice in the same JSON object, and a JSON object with a duplicate name has no defined meaning. Sending a
    /// command that contains such an entry throws <see cref="WebDriverBiDiSerializationException"/> rather than
    /// emitting the ambiguous payload. Set the typed property instead.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalCapabilities { get; } = [];
}
