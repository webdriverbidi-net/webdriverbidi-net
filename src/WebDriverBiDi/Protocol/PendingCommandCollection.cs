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
/// Canceled commands are remembered within a window of the most recent
/// <see cref="MaxTrackedCanceledCommands"/> cancellations, so at most that many are remembered at once.
/// The window counts cancellations, not the commands still remembered: a command whose late response has
/// been recognized by <see cref="TryRemoveCanceledCommand"/> is no longer remembered, but its cancellation
/// keeps its place in the window. When a new cancellation pushes the oldest place out of the window, the
/// command canceled there is forgotten, even if fewer commands than the limit are still remembered. A
/// response for a forgotten command is treated as an unknown message, exactly as a response for a command
/// that was never sent.
/// </para>
/// </remarks>
public class PendingCommandCollection : IDisposable
{
    /// <summary>
    /// The default number of most recent cancellations within which canceled commands are remembered for
    /// late-response recognition.
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
    /// canceled commands within the most recent <see cref="DefaultMaxTrackedCanceledCommands"/> cancellations.
    /// </summary>
    public PendingCommandCollection()
        : this(DefaultMaxTrackedCanceledCommands)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingCommandCollection"/> class.
    /// </summary>
    /// <param name="maxTrackedCanceledCommands">
    /// The number of most recent cancellations within which canceled commands are remembered for
    /// late-response recognition, which is also the most that can be remembered at once. A value of
    /// zero disables tracking, in which case a response for a canceled command is treated as an
    /// unknown message.
    /// </param>
    public PendingCommandCollection(uint maxTrackedCanceledCommands)
    {
        this.maxTrackedCanceledCommands = maxTrackedCanceledCommands;
    }

    /// <summary>
    /// Gets the unique ID of this collection.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets a value indicating whether this collection is accepting commands.
    /// </summary>
    public bool IsAcceptingCommands => Interlocked.CompareExchange(ref this.isAcceptingCommands, 0, 0) == 1;

    /// <summary>
    /// Gets the number of commands currently in the collection.
    /// </summary>
    public int PendingCommandCount => this.pendingCommands.Count;

    /// <summary>
    /// Gets the number of most recent cancellations within which canceled commands are remembered for
    /// late-response recognition, which is also the most that can be remembered at once.
    /// </summary>
    public uint MaxTrackedCanceledCommands => this.maxTrackedCanceledCommands;

    /// <summary>
    /// Gets the number of canceled commands currently remembered for late-response recognition.
    /// </summary>
    /// <remarks>
    /// A command whose late response has been recognized no longer counts here, but its cancellation still
    /// occupies a place in the window of recent cancellations, so older commands can be forgotten while this
    /// count is below <see cref="MaxTrackedCanceledCommands"/>.
    /// </remarks>
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
    /// <remarks>
    /// Only this command instance is removed and remembered. A different command pending in this collection
    /// under the same ID, which a transport that issues its own IDs could produce, is left untouched.
    /// </remarks>
    public virtual bool CancelPendingCommand(Command command, CommandCancellationReason reason)
    {
        bool canceled = command.Cancel();

        // Remove the entry only if it holds this instance: removal through the collection interface compares
        // the value as well as the key.
        if (((ICollection<KeyValuePair<long, Command>>)this.pendingCommands).Remove(new KeyValuePair<long, Command>(command.CommandId, command)))
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
            throw new InvalidOperationException($"Cannot clear the collection while it can accept new incoming commands; close it with {nameof(this.CloseAsync)} first");
        }

        foreach (Command pendingCommand in this.pendingCommands.Values)
        {
            pendingCommand.Cancel();
            this.TrackCanceledCommand(pendingCommand, CommandCancellationReason.ConnectionClosed);
        }

        this.pendingCommands.Clear();
    }

    /// <summary>
    /// Fails all pending commands in the collection, giving each one its own exception.
    /// The collection must have been closed before calling this method.
    /// </summary>
    /// <param name="exceptionFactory">
    /// Creates the exception for a pending command; invoked once per command. One instance shared
    /// across commands is rethrown on every awaiting caller, and each rethrow appends to that one
    /// object's stack trace, so concurrent callers would corrupt each other's diagnostics.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the collection has not been closed to the addition of new commands.
    /// </exception>
    public virtual void FailAllPendingCommands(Func<Exception> exceptionFactory)
    {
        if (this.IsAcceptingCommands)
        {
            throw new InvalidOperationException($"Cannot fail commands while the collection can accept new incoming commands; close it with {nameof(this.CloseAsync)} first");
        }

        foreach (Command pendingCommand in this.pendingCommands.Values)
        {
            pendingCommand.SetException(exceptionFactory());
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

            // The order queue is the window of recent cancellations. An ID consumed by
            // TryRemoveCanceledCommand deliberately keeps its place in it, as the class remarks
            // document, so the queue alone decides what is forgotten; removing an already
            // consumed ID from the dictionary is a harmless no-op.
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
