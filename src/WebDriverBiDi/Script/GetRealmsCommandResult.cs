// <copyright file="GetRealmsCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Result for getting realms using the script.getRealms command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GetRealmsCommandResult : CommandResult
{
    private List<RealmInfo> realms = new();

    /// <summary>
    /// Gets a read-only list of information about the realms.
    /// </summary>
    public IList<RealmInfo> Realms => this.realms.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of information about the realms for serialization purposes.
    /// </summary>
    [JsonProperty("realms")]
    internal List<RealmInfo> SerializableRealms { get => this.realms; set => this.realms = value; }
}