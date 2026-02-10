// <copyright file="GetRealmsCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting realms using the script.getRealms command.
/// </summary>
public record GetRealmsCommandResult : CommandResult
{
    [JsonConstructor]
    internal GetRealmsCommandResult()
    {
    }

    /// <summary>
    /// Gets a read-only list of information about the realms.
    /// </summary>
    [JsonIgnore]
    public IList<RealmInfo> Realms => this.SerializableRealms.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of information about the realms for serialization purposes.
    /// </summary>
    [JsonPropertyName("realms")]
    [JsonInclude]
    internal List<RealmInfo> SerializableRealms { get; set; } = [];
}
