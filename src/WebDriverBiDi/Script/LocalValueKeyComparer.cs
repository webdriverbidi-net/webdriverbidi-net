// <copyright file="LocalValueKeyComparer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Runtime.CompilerServices;

/// <summary>
/// Compares the keys of a dictionary of <see cref="LocalValue"/> objects converted from a
/// <see cref="RemoteValueDictionary"/>. String keys compare by value; <see cref="LocalValue"/>
/// keys compare by reference, preserving the reference-keyed semantics of the source
/// dictionary: each object key received from the remote end denotes a distinct JavaScript
/// object even when two of them convert to structurally identical values (for example, two
/// distinct Date keys holding the same instant). <see cref="LocalValue"/> is a record with
/// value equality, so the default comparer would collapse such keys into a single entry.
/// </summary>
internal sealed class LocalValueKeyComparer : IEqualityComparer<object>
{
    /// <summary>
    /// Gets the shared instance of the comparer.
    /// </summary>
    public static LocalValueKeyComparer Instance { get; } = new();

    /// <inheritdoc/>
    public new bool Equals(object? x, object? y)
    {
        return x is LocalValue ? ReferenceEquals(x, y) : object.Equals(x, y);
    }

    /// <inheritdoc/>
    public int GetHashCode(object obj)
    {
        return obj is LocalValue ? RuntimeHelpers.GetHashCode(obj) : obj.GetHashCode();
    }
}
