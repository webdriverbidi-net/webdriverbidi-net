// <copyright file="UnhandledErrorKind.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// The enumerated types of unhandled errors by the protocol transport.
/// </summary>
public enum UnhandledErrorKind
{
    /// <summary>
    /// An error resulting from a message recognized as an error response or as a registered event whose
    /// payload cannot be deserialized, for example because it lacks required values, or from an unexpected
    /// failure while processing an incoming message. A message that cannot be parsed as JSON is an
    /// <see cref="UnknownMessage"/> instead.
    /// </summary>
    ProtocolError,

    /// <summary>
    /// An error resulting from a message that cannot be parsed as JSON, or that is not a command response
    /// for a pending command, an error response, or a registered event.
    /// </summary>
    UnknownMessage,

    /// <summary>
    /// An error resulting from a well-formed error response that does not correspond to a command that the
    /// user has sent.
    /// </summary>
    UnexpectedError,

    /// <summary>
    /// An error resulting from an unhandled exception in a user-defined event
    /// handler for a protocol event.
    /// </summary>
    EventHandlerException,
}
