// <copyright file="RealmInfo.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing information about a script realm.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RealmInfoJsonConverter))]
public class RealmInfo
{
    private string realmId = string.Empty;
    private string origin = string.Empty;
    private RealmType realmType = RealmType.Window;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealmInfo"/> class.
    /// </summary>
    internal RealmInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the realm.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    /// <summary>
    /// Gets the origin of the realm.
    /// </summary>
    [JsonProperty("origin")]
    [JsonRequired]
    public string Origin { get => this.origin; internal set => this.origin = value; }

    /// <summary>
    /// Gets the type of the realm.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public RealmType Type { get => this.realmType; internal set => this.realmType = value; }
}