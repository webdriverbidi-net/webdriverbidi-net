// <copyright file="RealmDestroyedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for the event raised when a script realm is destroyed.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RealmDestroyedEventArgs : WebDriverBidiEventArgs
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
