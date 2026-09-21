// <copyright file="SetExtraHeadersCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.setExtraHeaders command.
/// </summary>
public class SetExtraHeadersCommandParameters : CommandParameters<SetExtraHeadersCommandResult>
{
    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetExtraHeadersCommandParameters"/>
    /// with the <see cref="Headers"/> property set to an empty list to clear the extra headers
    /// at whichever scope the instance targets: the session-wide headers when neither
    /// <see cref="Contexts"/> nor <see cref="UserContexts"/> is populated, or the headers of the
    /// named contexts or user contexts otherwise; the remote end keeps each scope separately.
    /// Returns a new instance on each access to allow for modification of the properties without
    /// affecting other uses. Functionally equivalent to using the parameterless constructor, but
    /// provided as a named property to make the intent of clearing the extra headers more explicit
    /// in code that uses this property.
    /// </summary>
    public static SetExtraHeadersCommandParameters ResetExtraHeaders => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.setExtraHeaders";

    /// <summary>
    /// Gets the list of extra HTTP headers to send with every request.
    /// </summary>
    [JsonPropertyName("headers")]
    public List<Header> Headers { get; } = [];

    /// <summary>
    /// Gets the browsing contexts, if any, for which to set the extra headers.
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
    /// Gets the user contexts, if any, for which to set the extra headers.
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
    /// Gets the browsing contexts, if any, for which to set the extra headers, for serialization purposes.
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
    /// Gets the user contexts, if any, for which to set the extra headers, for serialization purposes.
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
