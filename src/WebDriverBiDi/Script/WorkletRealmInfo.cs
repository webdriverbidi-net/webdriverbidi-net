// <copyright file="WorkletRealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a paint worklet realm for executing script.
/// </summary>
public record WorkletRealmInfo : RealmInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkletRealmInfo"/> class.
    /// </summary>
    [JsonConstructor]
    internal WorkletRealmInfo()
        : base()
    {
    }
}
