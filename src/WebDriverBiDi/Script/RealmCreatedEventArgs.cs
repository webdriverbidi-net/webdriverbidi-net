// <copyright file="RealmCreatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Object containing event data for the event raised when a script realm is created.
/// </summary>
public record RealmCreatedEventArgs : WebDriverBiDiEventArgs
{
    private readonly RealmInfo info;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealmCreatedEventArgs"/> class.
    /// </summary>
    /// <param name="info">The RealmInfo object containing information about the realm being created.</param>
    public RealmCreatedEventArgs(RealmInfo info)
    {
        this.info = info;
    }

    /// <summary>
    /// Gets the ID of the realm being created.
    /// </summary>
    public string RealmId { get => this.info.RealmId; }

    /// <summary>
    /// Gets the origin of the realm being created.
    /// </summary>
    public string Origin { get => this.info.Origin; }

    /// <summary>
    /// Gets the type of the realm being created.
    /// </summary>
    public RealmType Type { get => this.info.Type; }

    /// <summary>
    /// Casts the underlying realm info to a type-specific realm info, throwing if it is not of that
    /// type. Use <see cref="TryAs{T}"/> to test without throwing.
    /// </summary>
    /// <typeparam name="T">The specific type of RealmInfo to return.</typeparam>
    /// <returns>The underlying RealmInfo cast to the specified type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RealmInfo is not the specified type.</exception>
    public T As<T>()
        where T : RealmInfo
    {
        return this.info.As<T>();
    }

    /// <summary>
    /// Attempts to cast the underlying realm info to a type-specific realm info, returning
    /// <see langword="false"/> rather than throwing when it is not of that type.
    /// </summary>
    /// <typeparam name="T">The specific type of RealmInfo to return.</typeparam>
    /// <param name="result">When this method returns, contains the cast value or null if the cast failed.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryAs<T>([NotNullWhen(true)] out T? result)
        where T : RealmInfo
    {
        return this.info.TryAs(out result);
    }
}
