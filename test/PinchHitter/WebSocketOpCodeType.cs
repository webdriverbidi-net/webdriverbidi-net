// <copyright file="WebSocketOpCodeType.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Enum for WebSocket opcode types.
/// </summary>
public enum WebSocketOpcodeType
{
    /// <summary>
    /// Denotes a continuation code.
    /// </summary>
    Fragment = 0,

    /// <summary>
    /// Denotes a text code.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Denotes a binary code.
    /// </summary>
    Binary = 2,

    /// <summary>
    /// Denotes a closed connection.
    /// </summary>
    ClosedConnection = 8,

    /// <summary>
    /// Denotes a ping.
    /// </summary>
    Ping = 9,

    /// <summary>
    /// Denotes a pong.
    /// </summary>
    Pong = 10,
}
