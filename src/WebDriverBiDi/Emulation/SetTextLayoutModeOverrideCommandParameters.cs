// <copyright file="SetTextLayoutModeOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setTextLayoutModeOverride command.
/// </summary>
public class SetTextLayoutModeOverrideCommandParameters : CommandParameters<SetTextLayoutModeOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetTextLayoutModeOverrideCommandParameters"/> class.
    /// </summary>
    public SetTextLayoutModeOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetTextLayoutModeOverrideCommandParameters"/>
    /// with the <see cref="TextLayoutMode"/> property set to <see langword="null"/> to clear
    /// any existing text layout mode override. Returns a new instance on each access to allow for
    /// modification of the properties without affecting other uses. Functionally equivalent to
    /// using the parameterless constructor, but provided as a named property to make the intent
    /// of clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetTextLayoutModeOverrideCommandParameters ResetTextLayoutModeOverride => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setTextLayoutModeOverride";

    /// <summary>
    /// Gets or sets the text layout mode to emulate. When <see langword="null"/>, clears the emulated text layout mode.
    /// </summary>
    [JsonPropertyName("textLayoutMode")]
    public TextLayoutMode? TextLayoutMode { get; set; }

    /// <summary>
    /// Gets the browsing contexts for which to set the emulated text layout mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </para>
    /// <para>
    /// The two scopes are mutually exclusive: a command that names both browsing contexts and user contexts is
    /// rejected by the remote end with an <c>invalid argument</c> error. Scope the command one way or the other.
    /// </para>
    /// </remarks>
    [JsonIgnore]
    public List<string> Contexts { get; } = [];

    /// <summary>
    /// Gets the user contexts for which to set the emulated text layout mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </para>
    /// <para>
    /// The two scopes are mutually exclusive: a command that names both browsing contexts and user contexts is
    /// rejected by the remote end with an <c>invalid argument</c> error. Scope the command one way or the other.
    /// </para>
    /// </remarks>
    [JsonIgnore]
    public List<string> UserContexts { get; } = [];

    /// <summary>
    /// Gets the browsing contexts for which to set the emulated text layout mode, for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.Contexts.Count == 0)
            {
                return null;
            }

            return this.Contexts;
        }
    }

    /// <summary>
    /// Gets the user contexts for which to set the emulated text layout mode, for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUserContexts
    {
        get
        {
            if (this.UserContexts.Count == 0)
            {
                return null;
            }

            return this.UserContexts;
        }
    }
}
