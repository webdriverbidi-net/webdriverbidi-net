// <copyright file="EventReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when a protocol event is received from a WebDriver Bidi connection.
/// </summary>
public record EventReceivedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventReceivedEventArgs"/> class for an event whose
    /// <c>params</c> object carried no extension properties.
    /// </summary>
    /// <param name="message">The event message received.</param>
    public EventReceivedEventArgs(EventMessage message)
        : this(message, ReceivedDataDictionary.EmptyDictionary)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The event message received.</param>
    /// <param name="additionalData">The extension properties received inside the event's <c>params</c> object.</param>
    public EventReceivedEventArgs(EventMessage message, ReceivedDataDictionary additionalData)
    {
        this.EventName = message.EventName;
        this.EventData = message.EventData;
        this.AdditionalData = additionalData;
        this.AdditionalEventProperties = message.AdditionalData;
    }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    public object? EventData { get; }
}
