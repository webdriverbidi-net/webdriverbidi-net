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
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPreloadScriptCommandParameters"/> class.
    /// </summary>
    /// <param name="functionDeclaration">The function declaration defining the preload script.</param>
    public AddPreloadScriptCommandParameters(string functionDeclaration)
    {
        this.FunctionDeclaration = functionDeclaration;
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
    public string FunctionDeclaration { get; set; }

    /// <summary>
    /// Gets the arguments for the function declaration.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array. An empty list therefore means "not specified": the property is omitted from the
    /// JSON payload entirely. Add entries to the list to populate it.
    /// </remarks>
    [JsonIgnore]
    public List<ChannelValue> Arguments { get; } = [];

    /// <summary>
    /// Gets the browsing contexts for which to add the preload script.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> Contexts { get; } = [];

    /// <summary>
    /// Gets or sets the sandbox name of the preload script.
    /// </summary>
    [JsonPropertyName("sandbox")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sandbox { get; set; }

    /// <summary>
    /// Gets the user contexts for which to add the preload script.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> UserContexts { get; } = [];

    /// <summary>
    /// Gets the browsing contexts for which to add the preload script, for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.Contexts.Count == 0)
            {
                return null;
            }

            return this.Contexts;
        }
    }

    /// <summary>
    /// Gets the user contexts for which to add the preload script, for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUserContexts
    {
        get
        {
            if (this.UserContexts.Count == 0)
            {
                return null;
            }

            return this.UserContexts;
        }
    }

    /// <summary>
    /// Gets the arguments for the function declaration, for serialization purposes.
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<ChannelValue>? SerializableArguments
    {
        get
        {
            if (this.Arguments.Count == 0)
            {
                return null;
            }

            return this.Arguments;
        }
    }
}
