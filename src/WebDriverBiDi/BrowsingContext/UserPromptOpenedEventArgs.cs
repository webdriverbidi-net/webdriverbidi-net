// <copyright file="UserPromptOpenedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for the event raised when a user prompt opens.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class UserPromptOpenedEventArgs : WebDriverBiDiEventArgs
{
    private string browsingContextId;
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
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the type of user prompt opened.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public UserPromptType PromptType { get => this.promptType; internal set => this.promptType = value; }

    /// <summary>
    /// Gets the message displayed by the user prompt.
    /// </summary>
    [JsonProperty("message")]
    [JsonRequired]
    public string Message { get => this.message; internal set => this.message = value; }

    /// <summary>
    /// Gets the default value of the user prompt, if any.
    /// </summary>
    [JsonProperty("defaultValue")]
    public string? DefaultValue { get => this.defaultValue; internal set => this.defaultValue = value; }
}
