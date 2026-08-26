// <copyright file="CommandCancellationReason.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Enumerated value describing why the local end stopped waiting for the response to a command.
/// </summary>
/// <remarks>
/// The reason is recorded when a pending command is canceled so that, if the remote end later
/// answers anyway, the discarded response can be reported with the cause of the cancellation
/// (see <see cref="CanceledCommandInfo"/>).
/// </remarks>
public enum CommandCancellationReason
{
    /// <summary>
    /// The command was canceled explicitly, either through the <see cref="CancellationToken"/>
    /// passed to the command or by a direct call to <see cref="Transport.CancelCommand(Command, CommandCancellationReason)"/>.
    /// </summary>
    Canceled,

    /// <summary>
    /// The command's timeout elapsed before a response was received.
    /// </summary>
    TimedOut,

    /// <summary>
    /// The command was still pending when the connection to the remote end was closed.
    /// </summary>
    ConnectionClosed,
}
