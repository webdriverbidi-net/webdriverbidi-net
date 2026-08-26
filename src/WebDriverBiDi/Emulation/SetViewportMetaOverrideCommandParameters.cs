// <copyright file="SetViewportMetaOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setViewportMetaOverride command.
/// </summary>
public class SetViewportMetaOverrideCommandParameters : CommandParameters<SetViewportMetaOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetViewportMetaOverrideCommandParameters"/> class.
    /// </summary>
    public SetViewportMetaOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetViewportMetaOverrideCommandParameters"/>
    /// with the <see cref="IsViewportMetaOverridden"/> property set to <see langword="null"/> to
    /// clear any existing scripting enabled override. Returns a new instance on each access
    /// to allow for modification of the properties without affecting other uses. Functionally equivalent
    /// to using the parameterless constructor, but provided as a named property to make the intent
    /// of clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetViewportMetaOverrideCommandParameters ResetViewportMetaOverride => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setViewportMetaOverride";

    /// <summary>
    /// Gets or sets a value indicating whether the &lt;meta name=viewport&gt; tag is overridden.
    /// </summary>
    [JsonPropertyName("viewportMeta")]
    [JsonInclude]
    public bool? IsViewportMetaOverridden { get; set; }

    /// <summary>
    /// Gets the browsing contexts for which to set whether the &lt;meta name=viewport&gt; tag is overridden.
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
    /// Gets the user contexts for which to set whether the &lt;meta name=viewport&gt; tag is overridden.
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
    /// Gets the browsing contexts for which to set whether the &lt;meta name=viewport&gt; tag is overridden, for serialization purposes.
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
    /// Gets the user contexts for which to set whether the &lt;meta name=viewport&gt; tag is overridden, for serialization purposes.
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
