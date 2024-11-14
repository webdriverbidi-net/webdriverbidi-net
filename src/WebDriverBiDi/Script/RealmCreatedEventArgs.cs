// <copyright file="RealmCreatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Object containing event data for the event raised when a script realm is destroyed.
/// </summary>
public class RealmCreatedEventArgs : WebDriverBiDiEventArgs
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
    /// Gets this RealmCreatedEventArgs instance as a RealmInfo containing type-specific realm info.
    /// </summary>
    /// <typeparam name="T">The specific type of RealmInfo to return.</typeparam>
    /// <returns>This RealmCreatedEventArgs instance cast to the specified correct type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RealmInfo is not the specified type.</exception>
    public T As<T>()
        where T : RealmInfo
    {
        return this.info.As<T>();
    }
}
