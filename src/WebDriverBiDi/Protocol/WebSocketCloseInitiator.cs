// <copyright file="WebSocketCloseInitiator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Enumerated value describing which end has claimed the close of a <see cref="WebSocketConnection"/> session,
/// and with it the sending of this end's one Close frame.
/// </summary>
internal enum WebSocketCloseInitiator
{
    /// <summary>
    /// Neither end has claimed the close.
    /// </summary>
    None,

    /// <summary>
    /// This end claimed the close, by stopping the connection, and sends the Close frame that begins the handshake.
    /// </summary>
    Local,

    /// <summary>
    /// The remote end began the close, and the receive loop claimed it in order to acknowledge the remote end's Close frame.
    /// </summary>
    Remote,
}
