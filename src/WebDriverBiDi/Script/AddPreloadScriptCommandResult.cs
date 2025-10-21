// <copyright file="AddPreloadScriptCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Result for adding a preload script using the script.addPreloadScript command.
/// </summary>
public record AddPreloadScriptCommandResult : CommandResult
{
    private string preloadScriptId = string.Empty;

    [JsonConstructor]
    private AddPreloadScriptCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the preload script.
    /// </summary>
    [JsonPropertyName("script")]
    [JsonInclude]
    public string PreloadScriptId { get => this.preloadScriptId; private set => this.preloadScriptId = value; }
}
