// <copyright file="RemoteValueKeyComparer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Runtime.CompilerServices;

/// <summary>
/// Compares the keys of a <see cref="RemoteValueDictionary"/>. String keys compare by value;
/// <see cref="RemoteValue"/> keys compare by reference, because each object key received from the
/// remote end denotes a distinct JavaScript object even when two of them serialize identically
/// (for example, two function keys with no handle and no internal ID). <see cref="RemoteValue"/>
/// is a record with value equality, so the default comparer would collapse such keys.
/// </summary>
internal sealed class RemoteValueKeyComparer : IEqualityComparer<object>
{
    /// <summary>
    /// Gets the shared instance of the comparer.
    /// </summary>
    public static RemoteValueKeyComparer Instance { get; } = new();

    /// <inheritdoc/>
    public new bool Equals(object? x, object? y)
    {
        return x is RemoteValue ? ReferenceEquals(x, y) : object.Equals(x, y);
    }

    /// <inheritdoc/>
    public int GetHashCode(object obj)
    {
        return obj is RemoteValue ? RuntimeHelpers.GetHashCode(obj) : obj.GetHashCode();
    }
}
