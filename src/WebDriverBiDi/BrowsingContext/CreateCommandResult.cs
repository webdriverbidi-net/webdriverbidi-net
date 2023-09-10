// <copyright file="CreateCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result for creating a new browsing context using the browserContext.create command.
/// </summary>
public class CreateCommandResult : CommandResult
{
    private string contextId = string.Empty;

    [JsonConstructor]
    public CreateCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the created browsing context.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get => this.contextId; internal set => this.contextId = value; }
}