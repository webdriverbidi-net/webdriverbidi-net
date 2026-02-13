// <copyright file="UserPromptClosedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for the event raised when a user prompt is closed.
/// </summary>
public record UserPromptClosedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPromptClosedEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context for which the user prompt was closed.</param>
    /// <param name="isAccepted">A value of true if the user prompt was accepted; false if it was canceled.</param>
    [JsonConstructor]
    public UserPromptClosedEventArgs(string browsingContextId, bool isAccepted)
    {
        this.BrowsingContextId = browsingContextId;
        this.IsAccepted = isAccepted;
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was closed.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the user prompt was accepted (true), or if it was canceled (false).
    /// </summary>
    [JsonPropertyName("accepted")]
    [JsonRequired]
    [JsonInclude]
    public bool IsAccepted { get; internal set; }

    /// <summary>
    /// Gets the text of the user prompt.
    /// </summary>
    [JsonPropertyName("userText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? UserText { get; internal set; }
}
