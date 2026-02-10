// <copyright file="NewCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Result for creating a new session using the session.new command.
/// </summary>
public record NewCommandResult : CommandResult
{
    [JsonConstructor]
    private NewCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the session.
    /// </summary>
    [JsonPropertyName("sessionId")]
    [JsonRequired]
    [JsonInclude]
    public string SessionId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the actual capabilities used in this session.
    /// </summary>
    [JsonPropertyName("capabilities")]
    [JsonRequired]
    [JsonInclude]
    public CapabilitiesResult Capabilities { get; internal set; } = new();
}
