// <copyright file="SubscribeCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.subscribe command.
/// </summary>
public class SubscribeCommandParameters : CommandParameters<EmptyResult>
{
    private readonly List<string> eventList = [];

    private readonly List<string> contextList = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeCommandParameters"/> class.
    /// </summary>
    public SubscribeCommandParameters()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeCommandParameters"/> class.
    /// </summary>
    /// <param name="events">The list of events to which to subscribe or unsubscribe.</param>
    /// <param name="contexts">The list of browsing context IDs for which to subscribe to or unsubscribe from the specified events.</param>
    public SubscribeCommandParameters(string[] events, string[] contexts)
    {
        this.eventList.AddRange(events);
        this.contextList.AddRange(contexts);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeCommandParameters"/> class.
    /// </summary>
    /// /// <param name="events">The list of events to which to subscribe or unsubscribe.</param>
    /// <param name="contexts">The list of browsing context IDs for which to subscribe to or unsubscribe from the specified events.</param>
    public SubscribeCommandParameters(List<string> events, List<string> contexts)
    {
        this.eventList = events;
        this.contextList = contexts;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "session.subscribe";

    /// <summary>
    /// Gets the list of events to which to subscribe or unsubscribe.
    /// </summary>
    [JsonPropertyName("events")]
    public List<string> Events => this.eventList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events.
    /// </summary>
    [JsonIgnore]
    public List<string> Contexts => this.contextList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.contextList.Count == 0)
            {
                return null;
            }

            return this.contextList;
        }
    }
}
