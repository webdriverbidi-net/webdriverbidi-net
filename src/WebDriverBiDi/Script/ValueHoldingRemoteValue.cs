// <copyright file="ValueHoldingRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Interface for a remote value providing access to the underlying .NET value of the
/// remote value.
/// </summary>
public abstract record ValueHoldingRemoteValue : RemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueHoldingRemoteValue"/> class.
    /// </summary>
    /// <param name="type">The type of the remote value.</param>
    protected ValueHoldingRemoteValue(RemoteValueType type)
        : base()
    {
        this.Type = type;
    }

    /// <summary>
    /// Gets the value of this remote value as an object.
    /// </summary>
    /// <remarks>
    /// This value is guaranteed to be non-null, even if the type T allows null
    /// values.
    /// </remarks>
    public abstract object ValueObject { get; }
}
