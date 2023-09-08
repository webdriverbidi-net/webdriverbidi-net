// <copyright file="Source.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing the source for a script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Source
{
    private string realmId = string.Empty;
    private string? browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="Source"/> class.
    /// </summary>
    [JsonConstructor]
    internal Source()
    {
    }

    /// <summary>
    /// Gets the ID of the realm for a script.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    /// <summary>
    /// Gets the browsing context ID for a script.
    /// </summary>
    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public string? Context { get => this.browsingContextId; internal set => this.browsingContextId = value; }
}