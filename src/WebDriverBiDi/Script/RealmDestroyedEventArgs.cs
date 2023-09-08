// <copyright file="RealmDestroyedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for the event raised when a script realm is destroyed.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RealmDestroyedEventArgs : WebDriverBiDiEventArgs
{
    private string realmId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealmDestroyedEventArgs"/> class.
    /// </summary>
    /// <param name="realmId">The ID of the realm being destroyed.</param>
    [JsonConstructor]
    public RealmDestroyedEventArgs(string realmId)
    {
        this.realmId = realmId;
    }

    /// <summary>
    /// Gets the ID of the realm being destroyed.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}
