// <copyright file="PendingCommandCollection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Collections.Concurrent;

/// <summary>
/// Object containing a thread-safe collection of pending commands.
/// </summary>
public class PendingCommandCollection : IDisposable
{
    private readonly SemaphoreSlim commandAdditionSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<long, Command> pendingCommands = new();
    private int isAcceptingCommands = 1;

    /// <summary>
    /// Gets a value indicating whether this collection is accepting commands.
    /// </summary>
    public bool IsAcceptingCommands => Interlocked.CompareExchange(ref this.isAcceptingCommands, 0, 0) == 1;

    /// <summary>
    /// Gets the number of commands currently in the collection.
    /// </summary>
    public int PendingCommandCount => this.pendingCommands.Count;

    /// <summary>
    /// Asynchronously adds a command to the collection.
    /// </summary>
    /// <param name="command">The command to add to the collection.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown if the collection is no longer accepting commands, or the collection already
    /// contains a command with the ID of the command being added.
    /// </exception>
    public virtual async Task AddPendingCommandAsync(Command command)
    {
        await this.commandAdditionSemaphore.WaitAsync().ConfigureAwait(false);
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
    public virtual bool RemovePendingCommand(long commandId, out Command removedCommand)
    {
        // Note: We can use the null-forgiving operator here, as all callers will
        // check the boolean return value, and therefore never operate on a null
        // Command value.
        return this.pendingCommands.TryRemove(commandId, out removedCommand!);
    }

    /// <summary>
    /// Clears the collection, canceling all pending tasks of commands in the collection.
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
        if (disposing)
        {
            this.commandAdditionSemaphore.Dispose();
        }
    }
}
