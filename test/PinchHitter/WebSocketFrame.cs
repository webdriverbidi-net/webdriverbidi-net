// <copyright file="WebSocketFrame.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Buffers.Binary;
using System.Text;

/// <summary>
/// Represents a data frame for the WebSocket protocol.
/// </summary>
public class WebSocketFrame
{
    // The parity bit used for identifying the WebSocket opcode in the
    // WebSocket protocol.
    private static readonly byte ParityBit = 0x80;

    // The threshold below which the length of a WebSocket message can be
    // expressed in a single byte.
    private static readonly byte MessageLengthIndicatorSingleByte = 125;

    // Indicates that the length of a WebSocket message is between 126 and 65535
    // bytes, inclusive, and can therefore be expressed in a 16-bit integer.
    private static readonly byte MessageLengthIndicatorTwoBytes = 126;

    // Indicates that the length of a WebSocket message is greater than 65535
    // bytes, and therefore must be expressed as a 64-bit integer.
    private static readonly byte MessageLengthIndicatorEightBytes = 127;

    private readonly WebSocketOpcodeType opcode;
    private readonly byte[] data;

    private WebSocketFrame(WebSocketOpcodeType opcode, byte[] frameData)
    {
        this.opcode = opcode;
        this.data = frameData;
    }

    /// <summary>
    /// Gets the opcode for this frame.
    /// </summary>
    public WebSocketOpcodeType Opcode => this.opcode;

    /// <summary>
    /// Gets the byte array containing the data for this frame.
    /// </summary>
    public byte[] Data => this.data;

    /// <summary>
    /// Decodes a byte array to a WebSocket frame.
    /// </summary>
    /// <param name="buffer">The byte array to decode.</param>
    /// <returns>The WebSocket frame represented by the byte array.</returns>
    public static WebSocketFrame Decode(byte[] buffer)
    {
        const byte opcodeMask = 0x0F;
        const byte messageLengthMask = 0x7F;
        WebSocketOpcodeType opcode = (WebSocketOpcodeType)(buffer[0] & opcodeMask);

        byte messageLengthIndicator = Convert.ToByte(buffer[1] & messageLengthMask);

        int keyOffset;
        long messageLength;
        if (messageLengthIndicator == MessageLengthIndicatorTwoBytes)
        {
            // Message length is between 126 and 65535 bytes, inclusive
            ReadOnlySpan<byte> messageLengthSpan = new(buffer, 2, sizeof(short));
            messageLength = BinaryPrimitives.ReadInt16BigEndian(messageLengthSpan);
            keyOffset = 4;
        }
        else if (messageLengthIndicator == MessageLengthIndicatorEightBytes)
        {
            // Message length is over 65535 bytes
            ReadOnlySpan<byte> messageLengthSpan = new(buffer, 2, sizeof(long));
            messageLength = BinaryPrimitives.ReadInt64BigEndian(messageLengthSpan);
            keyOffset = 10;
        }
        else
        {
            // Message length is less than 126 bytes, and can be expressed in a
            // single byte, so the message length indicator byte in the frame
            // contains the actual length of the message.
            messageLength = messageLengthIndicator;
            keyOffset = 2;
        }

        // Incoming messages across a WebSocket are masked. The masking algorithm
        // has a four-byte mask, which each byte of the message is XOR'd with the
        // corresponding byte of the mask.
        byte[] decoded = new byte[messageLength];
        ArraySegment<byte> key = new(buffer, keyOffset, 4);
        long offset = Convert.ToInt64(keyOffset + key.Count);
        for (long index = 0; index < messageLength; index++)
        {
            decoded[index] = Convert.ToByte(buffer[offset + index] ^ key[Convert.ToInt32(index % 4)]);
        }

        return new WebSocketFrame(opcode, decoded);
    }

    /// <summary>
    /// Encodes data to a WebSocket frame.
    /// </summary>
    /// <param name="data">The string to encode.</param>
    /// <param name="opcode">The opcode of the frame to encode.</param>
    /// <returns>The WebSocket frame containing the encoded data.</returns>
    public static WebSocketFrame Encode(string data, WebSocketOpcodeType opcode = WebSocketOpcodeType.Text)
    {
        byte opcodeByte = Convert.ToByte(Convert.ToByte(opcode) | ParityBit);
        if (opcode == WebSocketOpcodeType.ClosedConnection)
        {
            // NOTE: Hard code the close frame data.
            return new WebSocketFrame(opcode, new byte[] { opcodeByte, 0x00 });
        }

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        long messageLength = dataBytes.LongLength;

        byte[] frameHeader = new byte[10];
        frameHeader[0] = opcodeByte;

        long dataOffset;
        if (messageLength <= MessageLengthIndicatorSingleByte)
        {
            // Message length is less than 126 bytes
            frameHeader[1] = Convert.ToByte(messageLength);
            dataOffset = 2;
        }
        else if (messageLength <= 65535)
        {
            // Message length is between 126 and 65535 bytes, inclusive
            frameHeader[1] = MessageLengthIndicatorTwoBytes;
            Span<byte> messageLengthSpan = new(frameHeader, 2, sizeof(short));
            BinaryPrimitives.WriteInt16BigEndian(messageLengthSpan, Convert.ToInt16(messageLength));
            dataOffset = 4;
        }
        else
        {
            // Message length is over 65535 bytes
            frameHeader[1] = MessageLengthIndicatorEightBytes;
            Span<byte> messageLengthSpan = new(frameHeader, 2, sizeof(long));
            BinaryPrimitives.WriteInt64BigEndian(messageLengthSpan, messageLength);
            dataOffset = 10;
        }

        byte[] buffer = new byte[dataOffset + messageLength];
        frameHeader.CopyTo(buffer, 0);
        dataBytes.CopyTo(buffer, dataOffset);
        return new WebSocketFrame(opcode, buffer);
    }
}