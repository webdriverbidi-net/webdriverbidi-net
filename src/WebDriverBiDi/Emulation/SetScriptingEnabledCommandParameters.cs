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
    public SetScriptingEnabledCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetScriptingEnabledCommandParameters"/>
    /// with the <see cref="IsScriptingEnabled"/> property set to <see langword="null"/> to
    /// clear any existing scripting enabled override. Returns a new instance on each access
    /// to allow for modification of the properties without affecting other uses. Functionally equivalent
    /// to using the parameterless constructor, but provided as a named property to make the intent
    /// of clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetScriptingEnabledCommandParameters ResetScriptingEnabled => new();

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
    public bool? IsScriptingEnabled { get; set; }

    /// <summary>
    /// Gets the browsing contexts for which to set whether scripting is enabled.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> Contexts { get; } = [];

    /// <summary>
    /// Gets the user contexts for which to set whether scripting is enabled.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> UserContexts { get; } = [];

    /// <summary>
    /// Gets the browsing contexts for which to set whether scripting is enabled, for serialization purposes.
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
    /// Gets the user contexts for which to set whether scripting is enabled, for serialization purposes.
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
