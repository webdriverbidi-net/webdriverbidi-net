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
    /// with the <see cref="Headers"/> property set to an empty list to clear any
    /// existing extra headers. Returns a new instance on each access to allow for
    /// modification of the properties without affecting other uses. Functionally equivalent
    /// to using the parameterless constructor, but provided as a named property to make the
    /// intent of clearing the extra headers more explicit in code that uses this property.
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
    [JsonInclude]
    public List<string> Headers { get; } = [];

    /// <summary>
    /// Gets or sets the browsing contexts, if any, for which to set the extra headers.
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
    /// Gets or sets the user contexts, if any, for which to set the extra headers.
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
