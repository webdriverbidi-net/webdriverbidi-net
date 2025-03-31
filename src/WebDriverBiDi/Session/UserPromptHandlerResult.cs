// <copyright file="UserPromptHandlerResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Describes a set of settings for handling user prompts during test execution.
/// </summary>
public record UserPromptHandlerResult
{
    private UserPromptHandler userPromptHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPromptHandlerResult"/> class.
    /// </summary>
    /// <param name="userPromptHandler">The <see cref="UserPromptHandler"/> to use in the result.</param>
    internal UserPromptHandlerResult(UserPromptHandler userPromptHandler)
    {
        this.userPromptHandler = userPromptHandler;
    }

    /// <summary>
    /// Gets the type of prompt handler for user prompts for which a handler type has not been explicitly set.
    /// </summary>
    public UserPromptHandlerType? Default => this.userPromptHandler.Default;

    /// <summary>
    /// Gets the type of prompt handler for alert user prompts.
    /// </summary>
    public UserPromptHandlerType? Alert => this.userPromptHandler.Alert;

    /// <summary>
    /// Gets the type of prompt handler for confirm user prompts.
    /// </summary>
    public UserPromptHandlerType? Confirm => this.userPromptHandler.Confirm;

    /// <summary>
    /// Gets the type of prompt handler for prompt user prompts.
    /// </summary>
    public UserPromptHandlerType? Prompt => this.userPromptHandler.Prompt;

    /// <summary>
    /// Gets the type of prompt handler for beforeUnload user prompts.
    /// </summary>
    public UserPromptHandlerType? BeforeUnload => this.userPromptHandler.BeforeUnload;

    /// <summary>
    /// Gets the type of prompt handler for file user prompts.
    /// </summary>
    public UserPromptHandlerType? File => this.userPromptHandler.File;
}
