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
    private List<string> headers = new();
    private List<string>? contexts;
    private List<string>? userContexts;

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
    public List<string> Headers => this.headers;

    /// <summary>
    /// Gets or sets the browsing contexts, if any, for which to set the cache behavior.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get => this.contexts;  set => this.contexts = value; }

    /// <summary>
    /// Gets or sets the user contexts, if any, for which to set the cache behavior.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get => this.userContexts;  set => this.userContexts = value; }
}