// <copyright file="IBiDiDriverEvents.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Log;
using WebDriverBiDi.Protocol;

/// <summary>
/// Interface for a driver that exposes events related to WebDriver BiDi protocol communication. This interface
/// is implemented by <see cref="BiDiDriver"/> and can be used for testing, or to allow users to implement their
/// own driver classes. It provides observable events for protocol events, errors received, unknown messages,
/// and log messages.
/// </summary>
/// <remarks>
/// <para>
/// The log level and the error-handling behaviors that used to sit here are settings of the transport
/// rather than events, and are reached through <see cref="BiDiDriver.TransportConfiguration"/>.
/// </para>
/// <para>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </para>
/// </remarks>
public interface IBiDiDriverEvents
{
    /// <summary>
    /// Gets an observable event that notifies when a protocol event is received from protocol transport.
    /// </summary>
    ObservableEvent<EventReceivedEventArgs> OnEventReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when a protocol error is received from protocol transport.
    /// </summary>
    ObservableEvent<ErrorReceivedEventArgs> OnUnexpectedErrorReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from protocol transport.
    /// </summary>
    ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when an error occurs in an observer of an observable event.
    /// </summary>
    /// <remarks>
    /// This event is for diagnostic and observability purposes, and does not prevent
    /// the propagation of the error back to the Transport class.
    /// </remarks>
    ObservableEvent<EventHandlerErrorOccurredEventArgs> OnEventHandlerErrorOccurred { get; }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by this driver.
    /// </summary>
    ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }

    /// <summary>
    /// Gets a value indicating whether a message at the given level would be raised on
    /// <see cref="OnLogMessage"/>, so that a caller can avoid building a message that would be discarded.
    /// </summary>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> of the message the caller would raise.</param>
    /// <returns><see langword="true"/> if such a message would be raised; otherwise, <see langword="false"/>.</returns>
    bool IsLogLevelEnabled(WebDriverBiDiLogLevel level);
}
