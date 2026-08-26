// <copyright file="MessageBuffer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;

/// <summary>
/// Accumulates the pieces of a message that arrives in more than one read (WebSocket frames,
/// or pipe reads that do not end on a message terminator) in memory rented from
/// <see cref="MemoryPool{T}.Shared"/>, so that the completed message can be handed to an
/// <see cref="IncomingMessage"/> as-is rather than being copied into a second buffer.
/// </summary>
/// <remarks>
/// <para>
/// Ownership of the rented memory transfers to the caller of <see cref="TakeOwnership"/>; after that
/// call the buffer is empty and may be reused for the next message. <see cref="Discard"/> returns
/// partially accumulated data to the pool (for example when a fragmented message is abandoned) and
/// likewise leaves the accumulator ready for reuse. Disposing the buffer discards any data it
/// still holds.
/// </para>
/// <para>
/// This class is not thread-safe; it is intended to be owned by a single receive loop. It is public
/// so that custom <see cref="Connection"/> implementations can reuse it, and so that it can be tested
/// directly; most users will never need it.
/// </para>
/// </remarks>
public sealed class MessageBuffer : IDisposable
{
    private IMemoryOwner<byte>? owner;
    private int length;

    /// <summary>
    /// Gets the number of bytes accumulated so far.
    /// </summary>
    public int Length => this.length;

    /// <summary>
    /// Gets a value indicating whether any bytes have been accumulated.
    /// </summary>
    public bool HasData => this.owner is not null;

    /// <summary>
    /// Appends a piece of the message to the accumulated data, growing the pooled buffer if needed.
    /// Empty pieces are ignored and do not start an accumulation.
    /// </summary>
    /// <param name="fragment">The bytes to append.</param>
    public void Append(ReadOnlySpan<byte> fragment)
    {
        if (fragment.IsEmpty)
        {
            return;
        }

        int required = this.length + fragment.Length;
        if (this.owner is null)
        {
            this.owner = MemoryPool<byte>.Shared.Rent(required);
        }
        else if (required > this.owner.Memory.Length)
        {
            // Grow geometrically to amortize the number of copies, but with pooled
            // memory on both sides of the copy. The pool may hand back a block larger
            // than requested, which is fine: consumers always slice to the explicit length.
            IMemoryOwner<byte> grown = MemoryPool<byte>.Shared.Rent(Math.Max(required, this.owner.Memory.Length * 2));
            this.owner.Memory.Slice(0, this.length).CopyTo(grown.Memory);
            this.owner.Dispose();
            this.owner = grown;
        }

        fragment.CopyTo(this.owner.Memory.Span.Slice(this.length));
        this.length = required;
    }

    /// <summary>
    /// Transfers ownership of the accumulated message to the caller and resets the accumulator
    /// so it can be reused for the next message.
    /// </summary>
    /// <param name="messageLength">When this method returns, contains the length of the accumulated message in bytes.</param>
    /// <returns>
    /// The <see cref="IMemoryOwner{T}"/> holding the message. Its <see cref="IMemoryOwner{T}.Memory"/>
    /// may be longer than <paramref name="messageLength"/>; the caller must dispose it to return the
    /// memory to the pool.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when no data has been accumulated.</exception>
    public IMemoryOwner<byte> TakeOwnership(out int messageLength)
    {
        if (this.owner is null)
        {
            throw new InvalidOperationException("No message data has been accumulated.");
        }

        IMemoryOwner<byte> result = this.owner;
        messageLength = this.length;
        this.owner = null;
        this.length = 0;
        return result;
    }

    /// <summary>
    /// Returns any partially accumulated data to the pool and resets the accumulator so it can be
    /// reused. Calling this method when nothing has been accumulated is a no-op.
    /// </summary>
    public void Discard()
    {
        this.owner?.Dispose();
        this.owner = null;
        this.length = 0;
    }

    /// <summary>
    /// Discards any data still held by this accumulator.
    /// </summary>
    public void Dispose()
    {
        this.Discard();
    }
}
