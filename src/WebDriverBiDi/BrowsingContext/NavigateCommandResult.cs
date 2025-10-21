// <copyright file="NavigateCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Contains the result of a navigation.
/// </summary>
public record NavigateCommandResult : CommandResult
{
    private string? id;
    private string url = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    protected NavigateCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the navigation.
    /// </summary>
    [JsonPropertyName("navigation")]
    [JsonInclude]
    public string? NavigationId { get => this.id; private set => this.id = value; }

    /// <summary>
    /// Gets the URL of the navigation.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get => this.url; private set => this.url = value; }
}
