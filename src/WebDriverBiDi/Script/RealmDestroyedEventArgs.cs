// <copyright file="RealmDestroyedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the event raised when a script realm is destroyed.
/// </summary>
public record RealmDestroyedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RealmDestroyedEventArgs"/> class.
    /// </summary>
    /// <param name="realmId">The ID of the realm being destroyed.</param>
    [JsonConstructor]
    public RealmDestroyedEventArgs(string realmId)
    {
        this.RealmId = realmId;
    }

    /// <summary>
    /// Gets the ID of the realm being destroyed.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get; internal set; }
}
