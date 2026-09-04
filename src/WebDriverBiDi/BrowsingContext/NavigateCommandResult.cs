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
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal NavigateCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the navigation, or <see langword="null"/> if the remote end reported none.
    /// </summary>
    /// <remarks>
    /// The specification's CDDL types this field as <c>Navigation / null</c>, so a null value is
    /// permitted on the wire and this property is nullable to match. The specification's own
    /// navigation steps allocate a navigation id for every navigation, including one that waits for
    /// <see cref="ReadinessState.None"/> and one that resolves to the same document, so a conforming
    /// remote end is not expected to return null in practice.
    /// </remarks>
    [JsonPropertyName("navigation")]
    [JsonRequired]
    [JsonInclude]
    public string? NavigationId { get; internal set; }

    /// <summary>
    /// Gets the URL of the navigation.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;
}
