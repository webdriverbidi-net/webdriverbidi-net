// <copyright file="SharedWorkerRealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Object representing a shared worker realm for executing script.
/// </summary>
public record SharedWorkerRealmInfo : RealmInfo
{
    private List<string> owners = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedWorkerRealmInfo"/> class.
    /// </summary>
    internal SharedWorkerRealmInfo()
        : base()
    {
    }
}
