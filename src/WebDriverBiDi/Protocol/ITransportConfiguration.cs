// <copyright file="ITransportConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Interface for the settings of a <see cref="Transport"/> that a consumer may tune. It is implemented
/// by <see cref="Transport"/> and reached from <see cref="BiDiDriver.TransportConfiguration"/>, so that a
/// driver created with the parameterless constructor can be tuned without constructing a transport by hand.
/// </summary>
/// <remarks>
/// <para>
/// This interface is deliberately narrower than <see cref="Transport"/> itself. The transport's
/// lifecycle and messaging operations — connecting, disconnecting, sending a command, registering an
/// event message — are driven by the <see cref="BiDiDriver"/> that owns the transport, which enforces the
/// registration-timing rules around them. Handing those out alongside the settings would let a caller
/// step around the driver, so only the settings appear here.
/// </para>
/// <para>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly to
/// allow for testing and to allow users to implement their own transport classes if they choose.
/// </para>
/// </remarks>
public interface ITransportConfiguration
{
    /// <summary>
    /// Gets or sets the minimum <see cref="WebDriverBiDiLogLevel"/> at which log messages are raised on
    /// <see cref="BiDiDriver.OnLogMessage"/>. Defaults to <see cref="WebDriverBiDiLogLevel.Info"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is one setting for the whole pipeline: the driver, its <see cref="Transport"/> and the
    /// transport's <see cref="Connection"/> all read and write the same value, so a message from any of
    /// the three is subject to it.
    /// </para>
    /// <para>
    /// The default excludes <see cref="WebDriverBiDiLogLevel.Debug"/> and
    /// <see cref="WebDriverBiDiLogLevel.Trace"/>. Set it to <see cref="WebDriverBiDiLogLevel.Debug"/> for
    /// a message per command sent and answered, or to <see cref="WebDriverBiDiLogLevel.Trace"/> to also
    /// receive every message exchanged with the remote end, which is how protocol traffic is inspected.
    /// Trace is not the default because composing those messages decodes each payload into a string.
    /// <see cref="WebDriverBiDiLogLevel.Off"/> suppresses every message.
    /// </para>
    /// <para>
    /// This governs only <see cref="BiDiDriver.OnLogMessage"/>. The <see cref="WebDriverBiDiEventSource"/>
    /// diagnostic events are independent, and are filtered by whatever enables the event source.
    /// </para>
    /// </remarks>
    WebDriverBiDiLogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions thrown by event handlers.
    /// Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning that such an exception is neither
    /// collected nor thrown from a later call, and does not stop the driver processing messages from the
    /// transport; it is still raised on <see cref="BiDiDriver.OnEventHandlerErrorOccurred"/> and as the
    /// <c>EventHandlerError</c> event of <see cref="WebDriverBiDiEventSource"/>.
    /// </summary>
    /// <remarks>
    /// Exceptions from handlers registered with
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> participate in this behavior
    /// when they are not already owned by task capture. If the caller captures handler tasks using
    /// <see cref="EventObserver{T}.WaitForCapturedTasksAsync"/>,
    /// <see cref="EventObserver{T}.WaitForCapturedTasksCompleteAsync"/>, or
    /// <see cref="EventObserver{T}.GetCapturedTasks"/>, those task exceptions remain owned by the caller
    /// rather than being surfaced again through the transport error pipeline. What decides that ownership
    /// is whether a capture session was active when the handler ran, not when its task faulted: a task
    /// that is <em>already</em> faulted by the time the handler returns it — one from
    /// <see cref="System.Threading.Tasks.Task.FromException(System.Exception)"/>, say, or from an
    /// <c>async</c> handler that throws before its first <c>await</c> — is captured like any other, and
    /// its failure belongs to the caller in the same way. A handler that throws <em>before returning a
    /// task at all</em> leaves nothing to capture; that exception reaches the code raising the event
    /// directly, and is governed by this property.
    /// </remarks>
    TransportErrorBehavior EventHandlerExceptionBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling a protocol error: a message recognized as
    /// an error response or as a registered event whose payload cannot be deserialized, or an unexpected
    /// failure while processing an incoming message. An error response whose ID matches a pending command
    /// fails that command instead, and a message that cannot be parsed as JSON is an unknown message.
    /// Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning that the error is neither collected
    /// nor thrown from a later call, and does not stop the driver processing messages from the transport;
    /// it is still written to <see cref="BiDiDriver.OnLogMessage"/> at <see cref="WebDriverBiDiLogLevel.Error"/>
    /// and, for a payload that cannot be deserialized, raised as the <c>ProtocolError</c> event of
    /// <see cref="WebDriverBiDiEventSource"/>. No observable event is raised for it.
    /// </summary>
    TransportErrorBehavior ProtocolErrorBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling an unknown message: a message that cannot be
    /// parsed as JSON, or one that is not a command response, an error response, or a registered event, such
    /// as a response for a command ID that was never issued or an event whose name is not registered.
    /// Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning that the error is neither collected
    /// nor thrown from a later call, and does not stop the driver processing messages from the transport; it
    /// is still raised on <see cref="BiDiDriver.OnUnknownMessageReceived"/> and as the
    /// <c>UnknownMessageReceived</c> event of <see cref="WebDriverBiDiEventSource"/>, and a message that
    /// cannot be parsed is also written to <see cref="BiDiDriver.OnLogMessage"/> at <see cref="WebDriverBiDiLogLevel.Error"/>.
    /// A response that arrives for a command after that command has timed out or been canceled is not
    /// an unknown message; it is logged and discarded without affecting this behavior.
    /// </summary>
    TransportErrorBehavior UnknownMessageBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when an unexpected error is
    /// encountered, such as an error response received with no corresponding command. Defaults to
    /// <see cref="TransportErrorBehavior.Ignore"/>, meaning that the error is neither collected nor thrown
    /// from a later call, and does not stop the driver processing messages from the transport; it is still
    /// raised on <see cref="BiDiDriver.OnUnexpectedErrorReceived"/>.
    /// An error response that arrives for a command after that command has timed out or been canceled is
    /// not an unexpected error; it is logged and discarded without affecting this behavior.
    /// </summary>
    TransportErrorBehavior UnexpectedErrorBehavior { get; set; }

    /// <summary>
    /// Gets or sets the timeout to wait for the incoming message queue to be emptied and its messages
    /// processed when disconnecting. The default is 10 seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This timeout applies to waiting for the incoming message queue to empty, to waiting for the
    /// messages in the queue to be processed, and, during disposal, to waiting for an in-flight connect
    /// attempt to complete before the transport's resources are released. It is distinct from
    /// <see cref="Connection.ShutdownTimeout"/>, which bounds the underlying connection's close handshake.
    /// </para>
    /// <para>
    /// Abandoning the wait does not stop the reader. Messages already delivered to the queue go on being
    /// processed in the background, and their handlers go on running; what this timeout bounds is how
    /// long the shutdown waits for them, not whether they run.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds
    /// the maximum timer duration supported by the runtime.
    /// </exception>
    TimeSpan ShutdownTimeout { get; set; }

    /// <summary>
    /// Gets or sets the timeout to wait for exclusive access to the transport's connection while another
    /// operation holds it. The default is 60 seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Connecting, disconnecting, sending a command and registering a type info resolver each take
    /// exclusive access for the duration of their work, so one of them waits while another is in
    /// progress. Bounding that wait keeps an operation issued from code the transport itself invoked
    /// while holding the access — a synchronous observer of <see cref="BiDiDriver.OnLogMessage"/> that
    /// sends a command, for example — from waiting on an operation that is itself waiting on the observer
    /// to return. Such an operation fails with <see cref="WebDriverBiDiTimeoutException"/> instead of
    /// never completing. An observer that needs to drive the driver should be registered with
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> so that it does not hold up
    /// the operation it was dispatched from.
    /// </para>
    /// <para>
    /// The default is deliberately longer than the longest legitimate hold, so that lowering it is a
    /// deliberate choice rather than a trap. A value shorter than the longest hold a session can
    /// legitimately take will fail operations that would otherwise have succeeded.
    /// <see cref="TimeSpan.Zero"/> never waits, and <see cref="Timeout.InfiniteTimeSpan"/> restores an
    /// unbounded wait.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds
    /// the maximum timer duration supported by the runtime.
    /// </exception>
    TimeSpan ConnectionLockTimeout { get; set; }
}
