// <copyright file="ITypeSafeRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Interface for a remote value for a specific type, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for script
/// execution on the remote end.
/// </summary>
/// <typeparam name="T">The native .NET type of the remote value.</typeparam>
public interface ITypeSafeRemoteValue<T>
{
    /// <summary>
    /// Gets the value of this remote value converted to the native .NET type.
    /// </summary>
    /// <remarks>
    /// Implementations whose type parameter <typeparamref name="T"/> is non-nullable return a non-null
    /// value; implementations that use a nullable <typeparamref name="T"/> (for example the collection
    /// and node remote values) may return <see langword="null"/>.
    /// </remarks>
    T Value { get; }
}
