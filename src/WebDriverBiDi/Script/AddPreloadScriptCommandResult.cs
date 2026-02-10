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
    [JsonConstructor]
    internal AddPreloadScriptCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the preload script.
    /// </summary>
    [JsonPropertyName("script")]
    [JsonInclude]
    public string PreloadScriptId { get; internal set; } = string.Empty;
}
