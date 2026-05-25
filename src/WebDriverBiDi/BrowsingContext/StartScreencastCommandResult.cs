// <copyright file="StartScreencastCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result from starting a screenshot.
/// </summary>
public record StartScreencastCommandResult : CommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartScreencastCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal StartScreencastCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the started screencast.
    /// </summary>
    [JsonPropertyName("screencast")]
    [JsonInclude]
    [JsonRequired]
    public string ScreencastId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the path of the file containing the screencast video and audio streams.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonInclude]
    [JsonRequired]
    public string Path { get; internal set; } = string.Empty;
}
