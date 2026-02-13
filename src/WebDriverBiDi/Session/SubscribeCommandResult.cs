// <copyright file="SubscribeCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the status of a remote end using the session.status command.
/// </summary>
public record SubscribeCommandResult : CommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal SubscribeCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the subscription.
    /// </summary>
    [JsonPropertyName("subscription")]
    [JsonRequired]
    [JsonInclude]
    public string SubscriptionId { get; internal set; } = string.Empty;
}
