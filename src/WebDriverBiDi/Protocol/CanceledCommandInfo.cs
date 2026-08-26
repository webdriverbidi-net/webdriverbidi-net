// <copyright file="CanceledCommandInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics;

/// <summary>
/// Describes a command that the local end stopped waiting for, so that a response arriving
/// afterward can be recognized as belonging to a canceled command rather than being treated
/// as an unknown message or an unexpected error.
/// </summary>
/// <remarks>
/// Only lightweight identifying data is retained; the <see cref="Command"/> itself, including its
/// parameters, is not, so that tracking canceled commands never pins large parameter graphs.
/// </remarks>
public sealed class CanceledCommandInfo
{
    private readonly Stopwatch cancellationStopwatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="CanceledCommandInfo"/> class.
    /// </summary>
    /// <param name="command">The command that was canceled.</param>
    /// <param name="reason">The reason the command was canceled.</param>
    internal CanceledCommandInfo(Command command, CommandCancellationReason reason)
    {
        this.CommandId = command.CommandId;
        this.CommandName = command.CommandName;
        this.ResponseType = command.ResponseType;
        this.Reason = reason;
        this.cancellationStopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Gets the ID of the canceled command.
    /// </summary>
    public long CommandId { get; }

    /// <summary>
    /// Gets the protocol method name of the canceled command.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Gets the type the response to the canceled command would have been deserialized to.
    /// </summary>
    public Type ResponseType { get; }

    /// <summary>
    /// Gets the reason the command was canceled.
    /// </summary>
    public CommandCancellationReason Reason { get; }

    /// <summary>
    /// Gets the time that has elapsed since the command was canceled.
    /// </summary>
    public TimeSpan TimeSinceCancellation => this.cancellationStopwatch.Elapsed;
}
