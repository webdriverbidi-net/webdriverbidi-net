// <copyright file="RealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing information about a script realm.
/// </summary>
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
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    /// <summary>
    /// Gets the origin of the realm.
    /// </summary>
    [JsonPropertyName("origin")]
    [JsonRequired]
    [JsonInclude]
    public string Origin { get => this.origin; internal set => this.origin = value; }

    /// <summary>
    /// Gets the type of the realm.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public RealmType Type { get => this.realmType; internal set => this.realmType = value; }
}
