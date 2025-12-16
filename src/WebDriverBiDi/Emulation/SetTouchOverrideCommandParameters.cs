// <copyright file="SetTouchOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setTouchOverride command.
/// </summary>
public class SetTouchOverrideCommandParameters : CommandParameters<SetTouchOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetTouchOverrideCommandParameters"/> class.
    /// </summary>
    public SetTouchOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setTouchOverride";

    /// <summary>
    /// Gets or sets the emulated touch points settings for the browser.
    /// When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("maxTouchPoints")]
    [JsonInclude]
    public ulong? MaxTouchPoints { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the touch override.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the touch override.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
