// <copyright file="PrintCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the tree of browsing contexts using the browserContext.getTree command.
/// </summary>
public record PrintCommandResult : CommandResult
{
    [JsonConstructor]
    private PrintCommandResult()
    {
    }

    /// <summary>
    /// Gets the screenshot image data as a base64-encoded string.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonRequired]
    [JsonInclude]
    public string Data { get; internal set; } = string.Empty;
}
