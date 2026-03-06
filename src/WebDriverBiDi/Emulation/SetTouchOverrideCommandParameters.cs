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
    /// Gets a pre-initialized instance of <see cref="SetTouchOverrideCommandParameters"/>
    /// with the <see cref="MaxTouchPoints"/> property set to <see langword="null"/> to
    /// clear any existing touch override. Returns a new instance on each access to allow
    /// for modification of the properties without affecting other uses. Functionally
    /// equivalent to using the parameterless constructor, but provided as a named property
    /// to make the intent of clearing the override more explicit in code that uses this
    /// property.
    /// </summary>
    public static SetTouchOverrideCommandParameters ResetTouchOverride => new();

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
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the touch override.
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
