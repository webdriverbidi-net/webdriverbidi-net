// <copyright file="ProtocolEventReceivedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing event data for events raised when a protocol event is received from a WebDriver Bidi connection.
/// </summary>
public class ProtocolEventReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolEventReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="methodName">The name of the method for which the event is being received.</param>
    /// <param name="eventData">The data about the event being received.</param>
    public ProtocolEventReceivedEventArgs(string methodName, object? eventData)
    {
        this.EventName = methodName;
        this.EventData = eventData;
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