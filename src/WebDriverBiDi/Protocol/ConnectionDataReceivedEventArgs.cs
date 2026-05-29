// <copyright file="ConnectionDataReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;

/// <summary>
/// Object containing event data for events raised when data is received from a WebDriver Bidi connection.
/// </summary>
public record ConnectionDataReceivedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionDataReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="owner">
    /// The <see cref="IMemoryOwner{T}"/> whose buffer contains the received data.
    /// Ownership transfers to the consumer of this event, which is responsible for
    /// passing it to <see cref="IncomingMessage"/> for disposal.
    /// </param>
    /// <param name="length">The number of valid bytes in the owner's buffer.</param>
    public ConnectionDataReceivedEventArgs(IMemoryOwner<byte> owner, int length)
    {
        this.BufferOwner = owner;
        this.DataLength = length;
    }

    /// <summary>
    /// Gets the data received from the connection as a read-only slice of the owner's buffer.
    /// </summary>
    public ReadOnlyMemory<byte> Data => this.BufferOwner.Memory.Slice(0, this.DataLength);

    /// <summary>
    /// Gets the <see cref="IMemoryOwner{T}"/> that owns the buffer containing the received data.
    /// </summary>
    public IMemoryOwner<byte> BufferOwner { get; }

    /// <summary>
    /// Gets the number of valid bytes in the owner's buffer.
    /// </summary>
    public int DataLength { get; }
}
