// <copyright file="ServiceWorkerRealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Object representing a service worker realm for executing script.
/// </summary>
public record ServiceWorkerRealmInfo : RealmInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceWorkerRealmInfo"/> class.
    /// </summary>
    internal ServiceWorkerRealmInfo()
        : base()
    {
    }
}
