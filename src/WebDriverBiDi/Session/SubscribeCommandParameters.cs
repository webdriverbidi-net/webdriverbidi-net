// <copyright file="SubscribeCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.subscribe command.
/// </summary>
public class SubscribeCommandParameters : CommandParameters<SubscribeCommandResult>
{
    private readonly List<string> eventList = [];

    private readonly List<string> contextList = [];

    private readonly List<string> userContextList = [];

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
    /// <param name="contexts">The list of browsing context IDs for which to subscribe to the specified events.</param>
    /// <param name="userContexts">The list of user context IDs for which to subscribe to the specified events.</param>
    public SubscribeCommandParameters(IList<string> events, IList<string>? contexts = null, IList<string>? userContexts = null)
    {
        this.eventList.AddRange(events);
        if (contexts is null && userContexts is null)
        {
            throw new ArgumentNullException("contexts and userContexts parameters cannot both be null");
        }

        if (contexts is not null)
        {
            this.contextList.AddRange(contexts);
        }

        if (userContexts is not null)
        {
            this.userContextList.AddRange(userContexts);
        }
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
    /// Gets the list of browsing context IDs for which to subscribe to the specified events.
    /// </summary>
    [JsonIgnore]
    public List<string> Contexts => this.contextList;

    /// <summary>
    /// Gets the list of user context IDs for which to subscribe to the specified events.
    /// </summary>
    [JsonIgnore]
    public List<string> UserContexts => this.userContextList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to the specified events for serialization purposes.
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

    /// <summary>
    /// Gets the list of user context IDs for which to subscribe to the specified events for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUserContexts
    {
        get
        {
            if (this.userContextList.Count == 0)
            {
                return null;
            }

            return this.userContextList;
        }
    }
}
