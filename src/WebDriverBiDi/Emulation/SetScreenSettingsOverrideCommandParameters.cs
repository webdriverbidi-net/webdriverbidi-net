// <copyright file="SetScreenSettingsOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScreenSettingsOverride command.
/// </summary>
public class SetScreenSettingsOverrideCommandParameters : CommandParameters<SetScreenSettingsOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetScreenSettingsOverrideCommandParameters"/> class.
    /// </summary>
    public SetScreenSettingsOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setScreenSettingsOverride";

    /// <summary>
    /// Gets or sets the area of the screen to emulate.
    /// When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("screenArea")]
    [JsonInclude]
    public ScreenArea? ScreenArea { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the screen settings override.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the screen settings override.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
