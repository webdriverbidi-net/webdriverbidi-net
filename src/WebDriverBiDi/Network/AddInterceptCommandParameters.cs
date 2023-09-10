// <copyright file="AddInterceptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.addIntercept command.
/// </summary>
public class AddInterceptCommandParameters : CommandParameters<AddInterceptCommandResult>
{
    private readonly List<InterceptPhase> interceptPhases = new();
    private List<UrlPattern>? urlPatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddInterceptCommandParameters" /> class.
    /// </summary>
    public AddInterceptCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.addIntercept";

    /// <summary>
    /// Gets the list of phases for which network traffic will be intercepted.
    /// </summary>
    [JsonPropertyName("phases")]
    public List<InterceptPhase> Phases { get => this.interceptPhases; }

    /// <summary>
    /// Gets or sets list of URL patterns for which to intercept network traffic.
    /// </summary>
    [JsonPropertyName("urlPatterns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public List<UrlPattern>? UrlPatterns { get => this.urlPatterns; set => this.urlPatterns = value; }
}