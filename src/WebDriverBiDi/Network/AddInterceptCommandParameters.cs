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
    /// <param name="additionalPhases">The additional <see cref="InterceptPhase"/>s for which to add the intercept.</param>
    public AddInterceptCommandParameters(InterceptPhase phase, params InterceptPhase[] additionalPhases)
    {
        this.Phases.Add(phase);

        foreach (InterceptPhase additionalPhase in additionalPhases)
        {
            if (!this.Phases.Contains(additionalPhase))
            {
                this.Phases.Add(additionalPhase);
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
    /// Gets the list of top-level browsing context IDs for which traffic will be intercepted.
    /// If present, it must contain at least one browsing context ID, and all IDs must represent top-level
    /// browsing contexts, or an error will be thrown by the remote end.
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
    /// Gets the list of URL patterns for which to intercept network traffic.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array. An empty list therefore means "not specified": the property is omitted from the
    /// JSON payload entirely. Add entries to the list to populate it.
    /// </remarks>
    [JsonIgnore]
    public List<UrlPattern> UrlPatterns { get; } = [];

    /// <summary>
    /// Gets the list of top-level browsing context IDs for which traffic will be intercepted, for serialization purposes.
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
    /// Gets the list of URL patterns for which to intercept network traffic, for serialization purposes.
    /// </summary>
    [JsonPropertyName("urlPatterns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<UrlPattern>? SerializableUrlPatterns
    {
        get
        {
            if (this.UrlPatterns.Count == 0)
            {
                return null;
            }

            return this.UrlPatterns;
        }
    }
}
