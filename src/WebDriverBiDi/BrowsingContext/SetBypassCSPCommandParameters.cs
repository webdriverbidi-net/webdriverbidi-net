// <copyright file="SetBypassCSPCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.setBypassCSP command.
/// </summary>
public class SetBypassCSPCommandParameters : CommandParameters<SetBypassCSPCommandResult>
{
    private bool? bypass;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetBypassCSPCommandParameters"/> class.
    /// </summary>
    public SetBypassCSPCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetBypassCSPCommandParameters"/>
    /// with the <see cref="Bypass"/> property set to <see langword="null"/> to
    /// clear any existing bypass CSP override. Returns a new instance on each access
    /// to allow for modification of the properties without affecting other uses. Functionally
    /// equivalent to using the parameterless constructor, but provided as a named property
    /// to make the intent of clearing the override more explicit in code that uses this
    /// property.
    /// </summary>
    public static SetBypassCSPCommandParameters ResetBypassCSP => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.setBypassCSP";

    /// <summary>
    /// Gets or sets a value indicating whether to bypass content security policies (CSP)
    /// for the specified contexts. Note that <see langword="null"/> and <see langword="false"/>
    /// are functionally equivalent.
    /// </summary>
    [JsonIgnore]
    public bool? Bypass { get => this.bypass; set => this.bypass = value; }

    /// <summary>
    /// Gets the browsing contexts for which to bypass content security policies (CSP).
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
    /// Gets the user contexts for which to bypass content security policies (CSP).
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
    /// Gets a value indicating whether to bypass content security policies (CSP)
    /// is enabled or disabled for the specified contexts for serialization purposes.
    /// </summary>
    [JsonPropertyName("bypass")]
    [JsonInclude]
    internal bool? SerializableBypass
    {
        get
        {
            if (this.bypass.HasValue && this.bypass.Value)
            {
                return true;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the browsing contexts for which to bypass content security policies (CSP), for serialization purposes.
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
    /// Gets the user contexts for which to bypass content security policies (CSP), for serialization purposes.
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
