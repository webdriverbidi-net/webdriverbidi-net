// <copyright file="SetScriptingEnabledCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScriptingEnabled command.
/// </summary>
public class SetScriptingEnabledCommandParameters : CommandParameters<SetScriptingEnabledCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetScriptingEnabledCommandParameters"/> class.
    /// </summary>
    /// <param name="isScriptingEnabled">Indicates whether scripting is enabled or disabled for the specified contexts.</param>
    public SetScriptingEnabledCommandParameters(bool isScriptingEnabled = false)
    {
        this.IsScriptingEnabled = isScriptingEnabled;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setScriptingEnabled";

    /// <summary>
    /// Gets or sets a value indicating whether scripting is enabled or disabled for the specified contexts.
    /// Note carefully that only emulation of disabled JavaScript is supported.
    /// </summary>
    [JsonPropertyName("enabled")]
    [JsonInclude]
    public bool IsScriptingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set whether scripting is enabled.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set whether scripting is enabled.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
