// <copyright file="StopScreencastCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result from stopping a screencast.
/// </summary>
public record StopScreencastCommandResult : CommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopScreencastCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal StopScreencastCommandResult()
    {
    }

    /// <summary>
    /// Gets the path to the file containing the screencast audio and video streams.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonRequired]
    [JsonInclude]
    public string Path { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a message describing a failure to write the screencast file while recording, which stopped the
    /// recording early; <see langword="null"/> when the file was written without error.
    /// </summary>
    [JsonPropertyName("error")]
    [JsonInclude]
    public string? Error { get; internal set; }
}
