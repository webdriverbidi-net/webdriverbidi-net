// <copyright file="HandleUserPromptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.handleUserPrompt command.
/// </summary>
public class HandleUserPromptCommandParameters : CommandParameters<HandleUserPromptCommandResult>
{
    private string browsingContextId;
    private bool? accept;
    private string? userText;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandleUserPromptCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID for which to handle the user prompt.</param>
    public HandleUserPromptCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.handleUserPrompt";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to handle the user prompt.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the user prompt should be accepted (if true) or canceled (if false).
    /// </summary>
    [JsonPropertyName("accept")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Accept { get => this.accept; set => this.accept = value; }

    /// <summary>
    /// Gets or sets the text sent to the user prompt.
    /// </summary>
    [JsonPropertyName("userText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserText { get => this.userText; set => this.userText = value; }
}
