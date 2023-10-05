// <copyright file="EventReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when a protocol event is received from a WebDriver Bidi connection.
/// </summary>
public class EventReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The event message containing information about the event.</param>
    public EventReceivedEventArgs(EventMessage message)
    {
        this.EventName = message.EventName;
        this.EventData = message.EventData;
        this.AdditionalData = message.AdditionalData;
    }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    public object? EventData { get; }

    /// <summary>
    /// Gets additional properties deserialized by this event.
    /// </summary>
    public ReceivedDataDictionary AdditionalData { get; }
}
