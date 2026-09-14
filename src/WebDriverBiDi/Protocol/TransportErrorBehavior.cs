// <copyright file="TransportErrorBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// The enumerated value specifying what the <see cref="Transport"/> should do upon encountering an error
/// that no command call can report: a message it cannot process, an error response not caused by a command,
/// or an unhandled exception in an event handler.
/// </summary>
public enum TransportErrorBehavior
{
    /// <summary>
    /// Neither collect nor throw errors. An ignored error is still reported through the transport's diagnostic
    /// channels, as described for each error behavior property of <see cref="Transport"/>: the corresponding
    /// observable event where there is one, <see cref="Transport.OnLogMessage"/>, or
    /// <see cref="WebDriverBiDiEventSource"/>.
    /// </summary>
    Ignore,

    /// <summary>
    /// Collect errors as they occur and throw an exception upon termination of the <see cref="Transport"/>.
    /// </summary>
    Collect,

    /// <summary>
    /// Terminates the <see cref="Transport"/> connection immediately. This will have the effect of throwing
    /// an exception upon execution of the next command.
    /// </summary>
    Terminate,
}
