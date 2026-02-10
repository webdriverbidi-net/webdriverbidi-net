// <copyright file="UserContextInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the user context info for a browser.
/// </summary>
public record UserContextInfo
{
    [JsonConstructor]
    private UserContextInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the user context.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonRequired]
    [JsonInclude]
    public string UserContextId { get; internal set; } = string.Empty;
}
