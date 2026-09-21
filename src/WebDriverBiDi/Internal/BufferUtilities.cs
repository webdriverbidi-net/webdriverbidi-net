// <copyright file="BufferUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

#if !NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Text;

/// <summary>
/// A utility class for reading a buffer of bytes without copying it where the target framework allows.
/// </summary>
/// <remarks>
/// <para>
/// The netstandard2.0 build has no <see cref="Encoding.GetString(byte[])"/> overload that takes a span, so
/// decoding a <see cref="ReadOnlyMemory{T}"/> there means reaching the array behind it. Every buffer this
/// library decodes comes from a pool backed by <see cref="System.Buffers.ArrayPool{T}"/>, so the array is
/// always there to be found; the copy remains for a buffer a caller supplied over memory of its own, which
/// no array backs.
/// </para>
/// <para>
/// This class is intentionally marked as internal, as it only contains utility methods used within this
/// library. They should have little use for users outside of this library.
/// </para>
/// </remarks>
internal static class BufferUtilities
{
    /// <summary>
    /// Decodes a buffer of UTF-8 bytes to a string.
    /// </summary>
    /// <param name="buffer">The bytes to decode.</param>
    /// <returns>The string the bytes encode.</returns>
    public static string GetUtf8String(ReadOnlyMemory<byte> buffer)
    {
#if NET5_0_OR_GREATER
        return Encoding.UTF8.GetString(buffer.Span);
#else
        if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
        {
            return Encoding.UTF8.GetString(segment.Array!, segment.Offset, segment.Count);
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
#endif
    }
}
