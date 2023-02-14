// <copyright file="SubscribeCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the session.subscribe command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SubscribeCommandParameters : CommandParameters<EmptyResult>
{
    private readonly List<string> eventList = new();

    private readonly List<string> contextList = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeCommandParameters"/> class.
    /// </summary>
    public SubscribeCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "session.subscribe";

    /// <summary>
    /// Gets the list of events to which to subscribe or unsubscribe.
    /// </summary>
    [JsonProperty("events")]
    public List<string> Events => this.eventList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events.
    /// </summary>
    public List<string> Contexts => this.contextList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events for serialization purposes.
    /// </summary>
    [JsonProperty("contexts", NullValueHandling = NullValueHandling.Ignore)]
    internal List<string>? SeralizableContexts
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