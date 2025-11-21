// <copyright file="GetUserContextsCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the user context info for a browser.
/// </summary>
public record GetUserContextsCommandResult : CommandResult
{
    /// <summary>
    /// Gets a read-only list of all of the user contexts open for the current browser.
    /// </summary>
    [JsonIgnore]
    public IList<UserContextInfo> UserContexts => this.SerializableUserContexts.AsReadOnly();

    /// <summary>
    /// Gets or sets the ID of the user context for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonRequired]
    [JsonInclude]
    internal List<UserContextInfo> SerializableUserContexts { get; set; } = [];
}
