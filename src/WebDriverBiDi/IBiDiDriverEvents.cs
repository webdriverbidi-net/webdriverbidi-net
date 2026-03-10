// <copyright file="IBiDiDriverEvents.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Protocol;

/// <summary>
/// Interface for a driver that exposes events related to WebDriver BiDi protocol communication. This interface
/// is implemented by <see cref="BiDiDriver"/> and can be used for testing, or to allow users to implement their
/// own driver classes. It provides observable events for protocol events, errors received, unknown messages,
/// and log messages.
/// </summary>
/// <remarks>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </remarks>
public interface IBiDiDriverEvents
{
    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions thrown by event handlers
    /// invoked by this driver. Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning that
    /// exceptions from event handlers will be caught and logged but will not cause the driver to stop
    /// processing messages from the transport.
    /// </summary>
    TransportErrorBehavior EventHandlerExceptionBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when a protocol error is
    /// received from the remote end. Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning
    /// that exceptions from protocol errors will be caught and logged but will not cause the driver to
    /// stop processing messages from the transport.
    /// </summary>
    TransportErrorBehavior ProtocolErrorBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when an unknown message is
    /// encountered, such as valid JSON that does not match any protocol data structure. Defaults to
    /// <see cref="TransportErrorBehavior.Ignore"/>, meaning that exceptions from unknown messages will
    /// be caught and logged, but will not cause the driver to stop processing messages from the transport.
    /// </summary>
    TransportErrorBehavior UnknownMessageBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when an unexpected error is
    /// encountered, such as an error response received with no corresponding command. Defaults to
    /// <see cref="TransportErrorBehavior.Ignore"/>, meaning that exceptions from unexpected errors will
    /// be caught and logged but will not cause the driver to stop processing messages from the transport.
    /// </summary>
    TransportErrorBehavior UnexpectedErrorBehavior { get; set; }

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
    /// Gets an observable event that notifies when a log message is emitted by this driver.
    /// </summary>
    ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }
}
