// <copyright file="SetCacheBehaviorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.setCacheBehavior command.
/// </summary>
public class SetCacheBehaviorCommandParameters : CommandParameters<EmptyResult>
{
    private CacheBehavior cacheBehavior = CacheBehavior.Default;
    private List<string>? contexts;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.setCacheBehavior";

    /// <summary>
    /// Gets or sets the behavior of the cache.
    /// </summary>
    [JsonPropertyName("cacheBehavior")]
    public CacheBehavior CacheBehavior { get => this.cacheBehavior; set => this.cacheBehavior = value; }

    /// <summary>
    /// Gets or sets the contexts, if any, for which to set the cache behavior.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get => this.contexts;  set => this.contexts = value; }
}