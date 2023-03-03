// <copyright file="WebSocketFrameData.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Represents data about a WebSocket frame.
/// </summary>
public class WebSocketFrameData
{
    private readonly WebSocketOpcodeType opcode;
    private readonly byte[] data;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketFrameData"/> class.
    /// </summary>
    /// <param name="opcode">The opcode of the frame.</param>
    /// <param name="data">The byte array containing the data of the frame.</param>
    public WebSocketFrameData(WebSocketOpcodeType opcode, byte[] data)
    {
        this.opcode = opcode;
        this.data = data;
    }

    /// <summary>
    /// Gets the opcode for this frame.
    /// </summary>
    public WebSocketOpcodeType Opcode => this.opcode;

    /// <summary>
    /// Gets the byte array containing the data for this frame.
    /// </summary>
    public byte[] Data => this.data;
}
