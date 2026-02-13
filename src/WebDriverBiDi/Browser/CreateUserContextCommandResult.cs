// <copyright file="CreateUserContextCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Result for creating a new user context for the browser.createUserContext command.
/// </summary>
public record CreateUserContextCommandResult : CommandResult
{
    /// <summary>
    /// Gets the ID of the user context.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonRequired]
    [JsonInclude]
    public string UserContextId { get; internal set; } = string.Empty;
}
