// <copyright file="SetScreenOrientationOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScreenOrientationOverride command.
/// </summary>
public class SetScreenOrientationOverrideCommandParameters : CommandParameters<SetScreenOrientationOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetScreenOrientationOverrideCommandParameters"/> class.
    /// </summary>
    public SetScreenOrientationOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetScreenOrientationOverrideCommandParameters"/>
    /// with the <see cref="ScreenOrientation"/> property set to <see langword="null"/> to clear any
    /// existing screen orientation override. Returns a new instance on each access to allow for
    /// modification of the properties without affecting other uses. Functionally equivalent to
    /// using the parameterless constructor, but provided as a named property to make the intent of
    /// clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetScreenOrientationOverrideCommandParameters ResetScreenOrientationOverride => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setScreenOrientationOverride";

    /// <summary>
    /// Gets or sets the emulated screen orientation settings for the browser.
    /// When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("screenOrientation")]
    [JsonInclude]
    public ScreenOrientation? ScreenOrientation { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the screen orientation override.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the screen orientation override.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
