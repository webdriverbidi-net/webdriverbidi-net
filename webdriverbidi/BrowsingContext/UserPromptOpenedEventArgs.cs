// <copyright file="UserPromptOpenedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for the event raised when a user prompt opens.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class UserPromptOpenedEventArgs : WebDriverBidiEventArgs
{
    private string browsingContextId;
    private UserPromptType promptType;
    private string message;

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
    public UserPromptType PromptType { get => this.promptType; internal set => this.promptType = value; }

    /// <summary>
    /// Gets the message displayed by the user prompt.
    /// </summary>
    [JsonProperty("message")]
    [JsonRequired]
    public string Message { get => this.message; internal set => this.message = value; }

    /// <summary>
    /// Sets the text value of the user prompt type for deserialization purposes.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    internal string SerializablePromptType
    {
        set
        {
            if (!Enum.TryParse<UserPromptType>(value, true, out UserPromptType type))
            {
                throw new WebDriverBidiException($"Invalid value for user prompt type: '{value}'");
            }

            this.promptType = type;
        }
    }
}
