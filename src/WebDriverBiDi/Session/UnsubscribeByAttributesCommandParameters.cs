// <copyright file="UnsubscribeByAttributesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
public class UnsubscribeByAttributesCommandParameters : UnsubscribeCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByAttributesCommandParameters"/> class for a single event.
    /// </summary>
    /// <param name="eventName">The event from which to unsubscribe.</param>
    public UnsubscribeByAttributesCommandParameters(string eventName)
        : this([eventName])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByAttributesCommandParameters"/> class.
    /// </summary>
    /// <remarks>
    /// The specification requires an unsubscription to name at least one event. Unlike values
    /// whose specification constraints the remote end enforces, an empty events list is
    /// rejected here: an unsubscription from no events cannot be meaningful under any revision
    /// of the specification, and accepting it would only defer a certain failure to the
    /// remote end.
    /// </remarks>
    /// <param name="events">The list of events from which to unsubscribe.</param>
    /// <exception cref="ArgumentException">Thrown when no events are specified in the events list.</exception>
    public UnsubscribeByAttributesCommandParameters(IList<string> events)
        : base()
    {
        if (events.Count == 0)
        {
            throw new ArgumentException("At least one event must be specified.", nameof(events));
        }

        this.Events.AddRange(events);
    }

    /// <summary>
    /// Gets the list of events to which to unsubscribe.
    /// </summary>
    [JsonPropertyName("events")]
    public List<string> Events { get; } = [];
}
