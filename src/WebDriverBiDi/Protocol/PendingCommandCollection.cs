// <copyright file="PendingCommandCollection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Object containing a thread-safe collection of pending commands.
/// </summary>
/// <remarks>
/// <para>
/// In addition to the commands that are awaiting a response, the collection remembers a bounded
/// number of commands that were canceled while pending (see
/// <see cref="CancelPendingCommand(Command, CommandCancellationReason)"/> and <see cref="Clear"/>).
/// The remote end does not know that the local end has stopped waiting, so it may still send a
/// response for such a command; <see cref="TryRemoveCanceledCommand"/> lets the transport recognize
/// that response and discard it, rather than treating it as an unknown message or an unexpected error.
/// </para>
/// <para>
/// At most <see cref="MaxTrackedCanceledCommands"/> canceled commands are remembered; when the limit is
/// exceeded, the oldest entries are forgotten first. A response for a forgotten command is treated as
/// an unknown message, exactly as a response for a command that was never sent.
/// </para>
/// </remarks>
public class PendingCommandCollection : IDisposable
{
    /// <summary>
    /// The default maximum number of canceled commands remembered for late-response recognition.
    /// </summary>
    public const uint DefaultMaxTrackedCanceledCommands = 1024;

    private readonly SemaphoreSlim commandAdditionSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<long, Command> pendingCommands = new();
    private readonly object canceledCommandsLock = new();
    private readonly Dictionary<long, CanceledCommandInfo> canceledCommands = [];
    private readonly Queue<long> canceledCommandOrder = new();
    private readonly uint maxTrackedCanceledCommands;
    private int isDisposedFlag = 0;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isAcceptingCommands = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingCommandCollection"/> class that remembers
    /// up to <see cref="DefaultMaxTrackedCanceledCommands"/> canceled commands.
    /// </summary>
    public PendingCommandCollection()
        : this(DefaultMaxTrackedCanceledCommands)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingCommandCollection"/> class.
    /// </summary>
    /// <param name="maxTrackedCanceledCommands">
    /// The maximum number of canceled commands to remember for late-response recognition. A value of
    /// zero disables tracking, in which case a response for a canceled command is treated as an
    /// unknown message.
    /// </param>
    public PendingCommandCollection(uint maxTrackedCanceledCommands)
    {
        this.maxTrackedCanceledCommands = maxTrackedCanceledCommands;
    }

    /// <summary>
    /// Gets a value indicating whether this collection is accepting commands.
    /// </summary>
    public bool IsAcceptingCommands => Interlocked.CompareExchange(ref this.isAcceptingCommands, 0, 0) == 1;

    /// <summary>
    /// Gets the number of commands currently in the collection.
    /// </summary>
    public int PendingCommandCount => this.pendingCommands.Count;

    /// <summary>
    /// Gets the maximum number of canceled commands remembered for late-response recognition.
    /// </summary>
    public uint MaxTrackedCanceledCommands => this.maxTrackedCanceledCommands;

    /// <summary>
    /// Gets the number of canceled commands currently remembered for late-response recognition.
    /// </summary>
    public int TrackedCanceledCommandCount
    {
        get
        {
            lock (this.canceledCommandsLock)
            {
                return this.canceledCommands.Count;
            }
        }
    }

    private bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Asynchronously adds a command to the collection.
    /// </summary>
    /// <param name="command">The command to add to the collection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown if the collection is no longer accepting commands, or the collection already
    /// contains a command with the ID of the command being added.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task AddPendingCommandAsync(Command command, CancellationToken cancellationToken = default)
    {
        await this.commandAdditionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!this.IsAcceptingCommands)
            {
                throw new WebDriverBiDiException("Cannot add command; pending command collection is closed");
            }

            if (!this.pendingCommands.TryAdd(command.CommandId, command))
            {
                throw new WebDriverBiDiException($"Could not add command with id {command.CommandId}, as id already exists");
            }
        }
        finally
        {
            this.commandAdditionSemaphore.Release();
        }
    }

    /// <summary>
    /// Removes a command from the collection.
    /// </summary>
    /// <param name="commandId">The ID of the command to remove.</param>
    /// <param name="removedCommand">The command object removed from the collection.</param>
    /// <returns><see langword="true"/> if a command with the specified ID exists in the collection to be removed; otherwise, <see langword="false"/>.</returns>
    public virtual bool RemovePendingCommand(long commandId, [NotNullWhen(true)] out Command? removedCommand)
    {
        return this.pendingCommands.TryRemove(commandId, out removedCommand);
    }

    /// <summary>
    /// Cancels a command and, if it was still pending, removes it from the collection and remembers
    /// it so that a response arriving later can be recognized by <see cref="TryRemoveCanceledCommand"/>.
    /// </summary>
    /// <param name="command">The command to cancel.</param>
    /// <param name="reason">The reason the command is being canceled.</param>
    /// <returns>
    /// <see langword="true"/> if the cancellation took effect, meaning the command had not yet
    /// completed; <see langword="false"/> if the command had already completed with a result or
    /// fault (for example, because its response arrived just before this call), in which case that
    /// outcome stands. Whether the command was remembered for late-response recognition depends only
    /// on whether it was still pending, and can be observed via <see cref="TrackedCanceledCommandCount"/>.
    /// </returns>
    public virtual bool CancelPendingCommand(Command command, CommandCancellationReason reason)
    {
        bool canceled = command.Cancel();
        if (this.pendingCommands.TryRemove(command.CommandId, out _))
        {
            this.TrackCanceledCommand(command, reason);
        }

        return canceled;
    }

    /// <summary>
    /// Determines whether a response with the specified command ID belongs to a command that was
    /// canceled while pending, and if so forgets that command so that subsequent responses with the
    /// same ID are treated as unknown.
    /// </summary>
    /// <param name="commandId">The command ID carried by the response.</param>
    /// <param name="canceledCommand">When this method returns <see langword="true"/>, the information recorded when the command was canceled.</param>
    /// <returns><see langword="true"/> if the ID belongs to a remembered canceled command; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryRemoveCanceledCommand(long commandId, [NotNullWhen(true)] out CanceledCommandInfo? canceledCommand)
    {
        lock (this.canceledCommandsLock)
        {
            if (this.canceledCommands.TryGetValue(commandId, out canceledCommand))
            {
                this.canceledCommands.Remove(commandId);
                return true;
            }
        }

        canceledCommand = null;
        return false;
    }

    /// <summary>
    /// Clears the collection, canceling all pending tasks of commands in the collection. Each
    /// cleared command is remembered with <see cref="CommandCancellationReason.ConnectionClosed"/>
    /// so that a response still being processed while the connection shuts down is discarded
    /// rather than reported as an unknown message.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the collection has not been closed to the addition of new commands.
    /// </exception>
    public virtual void Clear()
    {
        if (this.IsAcceptingCommands)
        {
            throw new InvalidOperationException("Cannot clear the collection while it can accept new incoming commands; close it with the Close method first");
        }

        foreach (Command pendingCommand in this.pendingCommands.Values)
        {
            pendingCommand.Cancel();
            this.TrackCanceledCommand(pendingCommand, CommandCancellationReason.ConnectionClosed);
        }

        this.pendingCommands.Clear();
    }

    /// <summary>
    /// Fails all pending commands in the collection with the specified exception.
    /// The collection must have been closed before calling this method.
    /// </summary>
    /// <param name="exception">The exception to set on each pending command.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the collection has not been closed to the addition of new commands.
    /// </exception>
    public virtual void FailAllPendingCommands(Exception exception)
    {
        if (this.IsAcceptingCommands)
        {
            throw new InvalidOperationException("Cannot fail commands while the collection can accept new incoming commands; close it with the Close method first");
        }

        foreach (Command pendingCommand in this.pendingCommands.Values)
        {
            pendingCommand.SetException(exception);
        }

        this.pendingCommands.Clear();
    }

    /// <summary>
    /// Asynchronously closes the collection, disallowing addition of any further commands to it.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task CloseAsync()
    {
        await this.commandAdditionSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            Interlocked.Exchange(ref this.isAcceptingCommands, 0);
        }
        finally
        {
            this.commandAdditionSemaphore.Release();
        }
    }

    /// <summary>
    /// Releases all resources used by this <see cref="PendingCommandCollection"/>.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this <see cref="PendingCommandCollection"/>
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.IsDisposed)
        {
            if (disposing)
            {
                this.commandAdditionSemaphore.Dispose();
            }

            this.SetDisposed();
        }
    }

    private void TrackCanceledCommand(Command command, CommandCancellationReason reason)
    {
        if (this.maxTrackedCanceledCommands == 0)
        {
            return;
        }

        lock (this.canceledCommandsLock)
        {
            this.canceledCommands[command.CommandId] = new CanceledCommandInfo(command, reason);
            this.canceledCommandOrder.Enqueue(command.CommandId);

            // Forget the oldest entries once the limit is exceeded. The order queue may
            // still hold IDs that TryRemoveCanceledCommand has already consumed; removing
            // those from the dictionary again is a harmless no-op.
            while (this.canceledCommandOrder.Count > this.maxTrackedCanceledCommands)
            {
                long evictedCommandId = this.canceledCommandOrder.Dequeue();
                this.canceledCommands.Remove(evictedCommandId);
            }
        }
    }

    private void SetDisposed()
    {
        Interlocked.Exchange(ref this.isDisposedFlag, 1);
    }
}
