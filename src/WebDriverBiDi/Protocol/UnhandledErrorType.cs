// <copyright file="UnhandledErrorType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// The enumerated types of unhandled errors by the protocol transport.
/// </summary>
public enum UnhandledErrorType
{
    /// <summary>
    /// An error resulting from an unparsable protocol message, for example,
    /// invalid JSON, or JSON that does not contain required values.
    /// </summary>
    ProtocolError,

    /// <summary>
    /// An error resulting from a valid JSON protocol message, but which does
    /// not match the schema of any known command result, error, or event.
    /// </summary>
    UnknownMessage,

    /// <summary>
    /// An error resulting from a valid JSON error response, but one that does
    /// not correspond to a command that the user has sent.
    /// </summary>
    UnexpectedError,

    /// <summary>
    /// An error resulting from an unhandled exception in a user-defined event
    /// handler for a protocol event.
    /// </summary>
    EventHandlerException,
}
