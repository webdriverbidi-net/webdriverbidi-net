// <copyright file="ITransportDiagnostics.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Interface for the observable state of a <see cref="Transport"/>. It is implemented by
/// <see cref="Transport"/> and reached from <see cref="BiDiDriver.TransportDiagnostics"/>, so that a
/// driver created with the parameterless constructor can be observed without constructing a transport by
/// hand.
/// </summary>
/// <remarks>
/// <para>
/// Every member is read-only, and every value is a snapshot that may be stale by the time the caller
/// observes it. None of them throws, whatever point of the transport's lifecycle they are read at, so
/// they are safe to poll from a timer or a health check.
/// </para>
/// <para>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly to
/// allow for testing and to allow users to implement their own transport classes if they choose.
/// </para>
/// </remarks>
public interface ITransportDiagnostics
{
    /// <summary>
    /// Gets a value indicating the lifecycle state of the transport with respect to its connection to a
    /// remote end.
    /// </summary>
    /// <remarks>
    /// <see cref="BiDiDriver.IsStarted"/> derives from this value, and so do the registration-timing
    /// rules for modules, custom events and type info resolvers. This property distinguishes the states
    /// that <see cref="BiDiDriver.IsStarted"/> collapses into <see langword="false"/>, such as a
    /// transport that is still connecting from one that has already disconnected.
    /// </remarks>
    TransportState State { get; }

    /// <summary>
    /// Gets the number of messages currently buffered in the incoming message queue and waiting to be
    /// processed by the reader task.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A persistently growing value indicates that event handlers are not keeping up with the incoming
    /// message rate, and should prompt investigation of handler duration or the use of
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>.
    /// </para>
    /// <para>
    /// The value reflects the queue for the <em>current</em> connection. Each connect installs a fresh
    /// queue whose depth begins at zero, and every message is counted against the queue it was written to
    /// for as long as that queue is being drained. A reconnect that gives up waiting for the previous
    /// connection's reader therefore reports only the current connection's backlog, even while the
    /// previous reader is still draining what remains of its own queue.
    /// </para>
    /// </remarks>
    int IncomingQueueDepth { get; }

    /// <summary>
    /// Gets the number of commands that have been sent to the remote end and are awaiting a response.
    /// </summary>
    /// <remarks>
    /// A persistently high value suggests that the remote end is not responding promptly, or that a burst
    /// of commands is in flight without corresponding responses yet. The pending-command collection is
    /// cleared while disconnecting, so reads after a disconnect typically return zero.
    /// </remarks>
    int PendingCommandCount { get; }
}
