// <copyright file="TransportErrorBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// The enumerated value specifying what the <see cref="Transport"/> should do upon encountering an error,
/// such as a malformed JSON payload, an unexpected error response not caused by a command, or an unhandled
/// exception in an event handler.
/// </summary>
public enum TransportErrorBehavior
{
    /// <summary>
    /// Ignore errors.
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