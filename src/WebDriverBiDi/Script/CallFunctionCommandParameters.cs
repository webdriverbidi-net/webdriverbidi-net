// <copyright file="CallFunctionCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the script.callFunction command.
/// </summary>
public class CallFunctionCommandParameters : CommandParameters<EvaluateResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallFunctionCommandParameters"/> class.
    /// </summary>
    /// <param name="functionDeclaration">The function declaration.</param>
    /// <param name="scriptTarget">The script target in which to call the function.</param>
    /// <param name="awaitPromise"><see langword="true" /> to await the script evaluation as a Promise; otherwise, <see langword="false" />.</param>
    public CallFunctionCommandParameters(string functionDeclaration, Target scriptTarget, bool awaitPromise)
    {
        this.FunctionDeclaration = functionDeclaration;
        this.ScriptTarget = scriptTarget;
        this.AwaitPromise = awaitPromise;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "script.callFunction";

    /// <summary>
    /// Gets or sets the function declaration.
    /// </summary>
    [JsonPropertyName("functionDeclaration")]
    public string FunctionDeclaration { get; set; }

    /// <summary>
    /// Gets or sets the script target against which to call the function.
    /// </summary>
    [JsonPropertyName("target")]
    public Target ScriptTarget { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to wait for the function execution to complete.
    /// </summary>
    [JsonPropertyName("awaitPromise")]
    public bool AwaitPromise { get; set; }

    /// <summary>
    /// Gets or sets the item to use as the 'this' object when the function is called.
    /// </summary>
    [JsonPropertyName("this")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ArgumentValue? ThisObject { get; set; }

    /// <summary>
    /// Gets the list of arguments to pass to the function.
    /// </summary>
    [JsonIgnore]
    public List<ArgumentValue> Arguments { get; } = [];

    /// <summary>
    /// Gets or sets the ownership model to use for objects in the function call.
    /// </summary>
    [JsonPropertyName("resultOwnership")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResultOwnership? ResultOwnership { get; set; }

    /// <summary>
    /// Gets or sets the serialization options for serializing results.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SerializationOptions? SerializationOptions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to activate the browsing context when calling the function. When omitted, is treated as if false.
    /// </summary>
    [JsonPropertyName("userActivation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? UserActivation { get; set; }

    /// <summary>
    /// Gets the list of arguments for serialization purposes.
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal IList<ArgumentValue>? SerializableArguments
    {
        get
        {
            if (this.Arguments.Count == 0)
            {
                return null;
            }

            return this.Arguments.AsReadOnly();
        }
    }
}
