// <copyright file="UserPromptHandler.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Describes a set of settings for handling user prompts during test execution.
/// </summary>
public class UserPromptHandler
{
    private UserPromptHandlerType? defaultHandlerType;
    private UserPromptHandlerType? alertHandlerType;
    private UserPromptHandlerType? confirmHandlerType;
    private UserPromptHandlerType? promptHandlerType;
    private UserPromptHandlerType? beforeUnloadHandlerType;
    private UserPromptHandlerType? fileHandlerType;

    /// <summary>
    /// Gets or sets the type of prompt handler for user prompts for which a handler type has not been explicitly set.
    /// </summary>
    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Default { get => this.defaultHandlerType; set => this.defaultHandlerType = value; }

    /// <summary>
    /// Gets or sets the type of prompt handler for alert user prompts.
    /// </summary>
    [JsonPropertyName("alert")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Alert { get => this.alertHandlerType; set => this.alertHandlerType = value; }

    /// <summary>
    /// Gets or sets the type of prompt handler for confirm user prompts.
    /// </summary>
    [JsonPropertyName("confirm")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Confirm { get => this.confirmHandlerType; set => this.confirmHandlerType = value; }

    /// <summary>
    /// Gets or sets the type of prompt handler for prompt user prompts.
    /// </summary>
    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Prompt { get => this.promptHandlerType; set => this.promptHandlerType = value; }

    /// <summary>
    /// Gets or sets the type of prompt handler for before unload user prompts.
    /// </summary>
    [JsonPropertyName("beforeunload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? BeforeUnload { get => this.beforeUnloadHandlerType; set => this.beforeUnloadHandlerType = value; }

    /// <summary>
    /// Gets or sets the type of prompt handler for file user prompts.
    /// </summary>
    [JsonPropertyName("file")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? File { get => this.fileHandlerType; set => this.fileHandlerType = value; }
}
