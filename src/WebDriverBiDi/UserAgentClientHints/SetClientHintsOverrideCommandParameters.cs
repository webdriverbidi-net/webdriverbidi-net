// <copyright file="SetClientHintsOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the userAgentClientHints.setClientHintsOverride command.
/// </summary>
public class SetClientHintsOverrideCommandParameters : CommandParameters<SetClientHintsOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetClientHintsOverrideCommandParameters"/> class.
    /// </summary>
    public SetClientHintsOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "userAgentClientHints.setClientHintsOverride";

    /// <summary>
    /// Gets or sets the client hints to override. When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("clientHints")]
    [JsonInclude]
    public ClientHintsMetadata? ClientHints { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the client hints override.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the client hints override.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
