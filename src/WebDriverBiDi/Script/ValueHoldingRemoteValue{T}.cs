// <copyright file="ValueHoldingRemoteValue{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Interface for a remote value for a specific type, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for script
/// execution on the remote end.
/// </summary>
/// <typeparam name="T">The native .NET type of the remote value.</typeparam>
public abstract record ValueHoldingRemoteValue<T> : ValueHoldingRemoteValue, ITypeSafeRemoteValue<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueHoldingRemoteValue{T}"/> class.
    /// </summary>
    /// <param name="type">The type of the remote value.</param>
    protected ValueHoldingRemoteValue(RemoteValueType type)
        : base(type)
    {
    }

    /// <summary>
    /// Gets the value of this remote value as an object.
    /// </summary>
    /// <remarks>
    /// This value is guaranteed to be non-null, even if the type T allows null
    /// values.
    /// </remarks>
    public override object ValueObject { get => this.Value; }

    /// <summary>
    /// Gets or sets the value of this remote value converted to the native .NET type.
    /// </summary>
    /// <remarks>
    /// This value is guaranteed to be non-null, even if the type T allows null
    /// values.
    /// </remarks>
    [NotNull]
    public abstract T Value { get; protected set; }
}
