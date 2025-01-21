// <copyright file="AddPreloadScriptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the script.addPreloadScript command.
/// </summary>
public class AddPreloadScriptCommandParameters : CommandParameters<AddPreloadScriptCommandResult>
{
    private string functionDeclaration;
    private List<ChannelValue>? arguments;
    private List<string>? contexts;
    private List<string>? userContexts;
    private string? sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPreloadScriptCommandParameters"/> class.
    /// </summary>
    /// <param name="functionDeclaration">The function declaration defining the preload script.</param>
    public AddPreloadScriptCommandParameters(string functionDeclaration)
    {
        this.functionDeclaration = functionDeclaration;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "script.addPreloadScript";

    /// <summary>
    /// Gets or sets the function declaration defining the preload script.
    /// </summary>
    [JsonPropertyName("functionDeclaration")]
    public string FunctionDeclaration { get => this.functionDeclaration; set => this.functionDeclaration = value; }

    /// <summary>
    /// Gets or sets the arguments for the function declaration.
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ChannelValue>? Arguments { get => this.arguments; set => this.arguments = value; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to add the preload script.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get => this.contexts; set => this.contexts = value; }

    /// <summary>
    /// Gets or sets the sandbox name of the preload script.
    /// </summary>
    [JsonPropertyName("sandbox")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to add the preload script.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get => this.userContexts; set => this.userContexts = value; }
}
