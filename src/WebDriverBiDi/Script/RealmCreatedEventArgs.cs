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
    /// Gets the ID of the browsing context of the realm being created.
    /// </summary>
    public string? BrowsingContext
    {
        get
        {
            if (this.info is not WindowRealmInfo windowRealm)
            {
                return null;
            }

            return windowRealm.BrowsingContext;
        }
    }

    /// <summary>
    /// Gets the type of the realm being created.
    /// </summary>
    public RealmType Type { get => this.info.Type; }
}
