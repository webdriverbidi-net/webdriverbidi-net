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
    private readonly List<ArgumentValue> arguments = new();
    private string functionDeclaration;
    private Target scriptTarget;
    private bool awaitPromise;
    private ArgumentValue? thisObject;
    private ResultOwnership? resultOwnership;
    private SerializationOptions? serializationOptions;
    private bool? userActivation;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallFunctionCommandParameters"/> class.
    /// </summary>
    /// <param name="functionDeclaration">The function declaration.</param>
    /// <param name="scriptTarget">The script target in which to call the function.</param>
    /// <param name="awaitPromise"><see langword="true" /> to await the script evaluation as a Promise; otherwise, <see langword="false" />.</param>
    public CallFunctionCommandParameters(string functionDeclaration, Target scriptTarget, bool awaitPromise)
    {
        this.functionDeclaration = functionDeclaration;
        this.scriptTarget = scriptTarget;
        this.awaitPromise = awaitPromise;
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
    public string FunctionDeclaration { get => this.functionDeclaration; set => this.functionDeclaration = value; }

    /// <summary>
    /// Gets or sets the script target against which to call the function.
    /// </summary>
    [JsonPropertyName("target")]
    public Target ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to wait for the function execution to complete.
    /// </summary>
    [JsonPropertyName("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    /// <summary>
    /// Gets or sets the item to use as the 'this' object when the function is called.
    /// </summary>
    [JsonPropertyName("this")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ArgumentValue? ThisObject { get => this.thisObject; set => this.thisObject = value; }

    /// <summary>
    /// Gets the list of arguments to pass to the function.
    /// </summary>
    [JsonIgnore]
    public List<ArgumentValue> Arguments => this.arguments;

    /// <summary>
    /// Gets or sets the ownership model to use for objects in the function call.
    /// </summary>
    [JsonPropertyName("resultOwnership")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResultOwnership? ResultOwnership { get => this.resultOwnership; set => this.resultOwnership = value; }

    /// <summary>
    /// Gets or sets the serialization options for serializing results.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SerializationOptions? SerializationOptions { get => this.serializationOptions; set => this.serializationOptions = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to activate the browsing context when calling the function. When omitted, is treated as if false.
    /// </summary>
    [JsonPropertyName("userActivation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? UserActivation { get => this.userActivation; set => this.userActivation = value; }

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
            if (this.arguments.Count == 0)
            {
                return null;
            }

            return this.arguments.AsReadOnly();
        }
    }
}
