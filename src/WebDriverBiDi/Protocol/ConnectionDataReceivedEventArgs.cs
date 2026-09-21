// <copyright file="ConnectionDataReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Text;

/// <summary>
/// Object containing event data for events raised when data is received from a WebDriver BiDi connection.
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

    /// <summary>
    /// Prints the members of these event arguments for <see cref="object.ToString"/>: those of the base record, and
    /// <see cref="DataLength"/>.
    /// </summary>
    /// <param name="builder">The builder to print the members to.</param>
    /// <returns><see langword="true"/>, because members were printed.</returns>
    /// <remarks>
    /// <see cref="Data"/> and <see cref="BufferOwner"/> are deliberately not printed. The buffer is pooled memory that
    /// the consumer of the event disposes once it has processed the message, after which reading <see cref="Data"/>
    /// throws <see cref="ObjectDisposedException"/>, so printing it would make <see cref="object.ToString"/> throw.
    /// </remarks>
    protected override bool PrintMembers(StringBuilder builder)
    {
        // The base record always prints its members, so these follow a separator.
        base.PrintMembers(builder);
        builder.Append(", DataLength = ").Append(this.DataLength);
        return true;
    }
}
