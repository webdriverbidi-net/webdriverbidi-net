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
    /// Gets or sets the browsing contexts for which to bypass content security policies (CSP).
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
    /// Gets or sets the user contexts for which to bypass content security policies (CSP).
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }

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
}
