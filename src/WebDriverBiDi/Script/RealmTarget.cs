// <copyright file="RealmTarget.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// A script target for a realm.
/// </summary>
public record RealmTarget : Target
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RealmTarget"/> class.
    /// </summary>
    /// <param name="realmId">The ID of the realm.</param>
    [JsonConstructor]
    public RealmTarget(string realmId)
    {
        this.RealmId = realmId;
    }

    /// <summary>
    /// Gets the ID of the realm.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get; internal set; }
}
