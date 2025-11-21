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
    /// <summary>
    /// Gets or sets the type of prompt handler for user prompts for which a handler type has not been explicitly set.
    /// </summary>
    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Default { get; set; }

    /// <summary>
    /// Gets or sets the type of prompt handler for alert user prompts.
    /// </summary>
    [JsonPropertyName("alert")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Alert { get; set; }

    /// <summary>
    /// Gets or sets the type of prompt handler for confirm user prompts.
    /// </summary>
    [JsonPropertyName("confirm")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Confirm { get; set; }

    /// <summary>
    /// Gets or sets the type of prompt handler for prompt user prompts.
    /// </summary>
    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the type of prompt handler for before unload user prompts.
    /// </summary>
    [JsonPropertyName("beforeunload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? BeforeUnload { get; set; }

    /// <summary>
    /// Gets or sets the type of prompt handler for file user prompts.
    /// </summary>
    [JsonPropertyName("file")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandlerType? File { get; set; }
}
