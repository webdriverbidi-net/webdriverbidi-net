// <copyright file="UserPromptOpenedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.Session;

/// <summary>
/// Object containing event data for the event raised when a user prompt opens.
/// </summary>
public record UserPromptOpenedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPromptOpenedEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context for which the user prompt was opened.</param>
    /// <param name="promptType">The type of the user prompt.</param>
    /// <param name="message">The message displayed in the user prompt.</param>
    [JsonConstructor]
    public UserPromptOpenedEventArgs(string browsingContextId, UserPromptType promptType, string message)
    {
        this.BrowsingContextId = browsingContextId;
        this.PromptType = promptType;
        this.Message = message;
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was opened.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; }

    /// <summary>
    /// Gets the prompt handler type for this event.
    /// </summary>
    [JsonPropertyName("handler")]
    [JsonRequired]
    [JsonInclude]
    public UserPromptHandlerType Handler { get; internal set; }

    /// <summary>
    /// Gets the type of user prompt opened.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public UserPromptType PromptType { get; internal set; }

    /// <summary>
    /// Gets the message displayed by the user prompt.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonRequired]
    [JsonInclude]
    public string Message { get; internal set; }

    /// <summary>
    /// Gets the default value of the user prompt, if any.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    [JsonInclude]
    public string? DefaultValue { get; internal set; }
}
