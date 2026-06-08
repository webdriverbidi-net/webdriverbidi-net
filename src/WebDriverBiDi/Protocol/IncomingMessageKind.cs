// <copyright file="IncomingMessageKind.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Enumerated value indicating the kind of incoming message dictated by the shape of the received data.
/// </summary>
public enum IncomingMessageKind
{
    /// <summary>
    /// The incoming message has not been parsed, so its kind cannot be determined.
    /// </summary>
    Uninitialized,

    /// <summary>
    /// The incoming message is a message containing a successful command response.
    /// </summary>
    CommandResponse,

    /// <summary>
    /// The incoming message is message containing an error response, possibly for a command.
    /// </summary>
    ErrorResponse,

    /// <summary>
    /// The incoming message is a message containing event data.
    /// </summary>
    Event,

    /// <summary>
    /// The incoming message was intentionally filtered by the document transformer and should be silently discarded.
    /// </summary>
    Filtered,

    /// <summary>
    /// The incoming message is none of the known incoming message kinds.
    /// </summary>
    Unknown,
}
