// <copyright file="PendingCommandCollection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Collections.Concurrent;

/// <summary>
/// Object containing a thread-safe collection of pending commands.
/// </summary>
public class PendingCommandCollection
{
    private readonly SemaphoreSlim commandAdditionSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<long, Command> pendingCommands = new();

    /// <summary>
    /// Gets a value indicating whether this collection is accepting commands.
    /// </summary>
    public bool IsAcceptingCommands { get; internal set; } = true;

    /// <summary>
    /// Gets the number of commands currently in the collection.
    /// </summary>
    public int PendingCommandCount => this.pendingCommands.Count;

    /// <summary>
    /// Adds a command to the collection.
    /// </summary>
    /// <param name="command">The command to add to the collection.</param>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown if the collection is no longer accepting commands, or the collection already
    /// contains a command with the ID of the command being added.
    /// </exception>
    public virtual void AddPendingCommand(Command command)
    {
        this.commandAdditionSemaphore.Wait();
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
    /// <exception cref="WebDriverBiDiException">
    /// Thrown if the collection has not been closed to the addition of new commands.
    /// </exception>
    public virtual void Clear()
    {
        if (this.IsAcceptingCommands)
        {
            throw new WebDriverBiDiException("Cannot clear the collection while it can accept new incoming commands; close it with the Close method first");
        }

        foreach (Command pendingCommand in this.pendingCommands.Values)
        {
            pendingCommand.Cancel();
        }

        this.pendingCommands.Clear();
    }

    /// <summary>
    /// Closes the collection, disallowing addition of any further commands to it.
    /// </summary>
    public virtual void Close()
    {
        this.commandAdditionSemaphore.Wait();
        this.IsAcceptingCommands = false;
        this.commandAdditionSemaphore.Release();
    }
}
