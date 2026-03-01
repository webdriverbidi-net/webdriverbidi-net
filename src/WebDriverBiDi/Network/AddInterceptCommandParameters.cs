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
    /// <summary>
    /// Initializes a new instance of the <see cref="AddInterceptCommandParameters" /> class.
    /// </summary>
    /// <param name="phase">The <see cref="InterceptPhase"/> for which to add the intercept.</param>
    public AddInterceptCommandParameters(InterceptPhase phase)
    {
        this.Phases.Add(phase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddInterceptCommandParameters" /> class.
    /// </summary>
    /// <param name="phases">The <see cref="InterceptPhase"/>s for which to add the intercept.</param>
    public AddInterceptCommandParameters(params InterceptPhase[] phases)
    {
        if (phases.Length == 0)
        {
            throw new ArgumentException("You must supply at least one phase for the intercept", nameof(phases));
        }

        foreach (InterceptPhase phase in phases)
        {
            if (!this.Phases.Contains(phase))
            {
                this.Phases.Add(phase);
            }
        }
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
    public List<InterceptPhase> Phases { get; } = [];

    /// <summary>
    /// Gets or sets the list of top-level browsing context IDs for which traffic will be intercepted.
    /// If present, it must contain at least one browsing context ID, and all IDs must represent top-level
    /// browsing contexts, or an error will be thrown by the remote end.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BrowsingContextIds { get; set; }

    /// <summary>
    /// Gets or sets the list of URL patterns for which to intercept network traffic.
    /// </summary>
    [JsonPropertyName("urlPatterns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public List<UrlPattern>? UrlPatterns { get; set; }
}
