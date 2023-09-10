// <copyright file="GetRealmsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the script.getRealms command.
/// </summary>
public class GetRealmsCommandParameters : CommandParameters<GetRealmsCommandResult>
{
    private string? browsingContextId;
    private RealmType? realmType;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRealmsCommandParameters"/> class.
    /// </summary>
    public GetRealmsCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "script.getRealms";

    /// <summary>
    /// Gets or sets the ID of the browsing context of the realms to get.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the type of realms to get.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public RealmType? RealmType { get => this.realmType; set => this.realmType = value; }
}