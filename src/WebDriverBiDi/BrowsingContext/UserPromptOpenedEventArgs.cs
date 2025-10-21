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
    private string browsingContextId;
    private UserPromptHandlerType handler;
    private UserPromptType promptType;
    private string message;
    private string? defaultValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPromptOpenedEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context for which the user prompt was opened.</param>
    /// <param name="promptType">The type of the user prompt.</param>
    /// <param name="message">The message displayed in the user prompt.</param>
    [JsonConstructor]
    public UserPromptOpenedEventArgs(string browsingContextId, UserPromptType promptType, string message)
    {
        this.browsingContextId = browsingContextId;
        this.promptType = promptType;
        this.message = message;
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was opened.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the prompt handler type for this event.
    /// </summary>
    [JsonPropertyName("handler")]
    [JsonRequired]
    [JsonInclude]
    public UserPromptHandlerType Handler { get => this.handler;  private set => this.handler = value; }

    /// <summary>
    /// Gets the type of user prompt opened.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public UserPromptType PromptType { get => this.promptType; private set => this.promptType = value; }

    /// <summary>
    /// Gets the message displayed by the user prompt.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonRequired]
    [JsonInclude]
    public string Message { get => this.message; private set => this.message = value; }

    /// <summary>
    /// Gets the default value of the user prompt, if any.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    [JsonInclude]
    public string? DefaultValue { get => this.defaultValue; private set => this.defaultValue = value; }
}
