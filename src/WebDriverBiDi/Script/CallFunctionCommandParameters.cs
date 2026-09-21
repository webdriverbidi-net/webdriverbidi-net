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
    /// Gets or sets a value indicating whether a Promise returned by the function is awaited.
    /// The function is always called to completion; when this is <see langword="true"/> and the
    /// result is a Promise, its settled value is returned in place of the Promise itself.
    /// </summary>
    [JsonPropertyName("awaitPromise")]
    public bool AwaitPromise { get; set; }

    /// <summary>
    /// Gets or sets the item to use as the 'this' object when the function is called.
    /// </summary>
    [JsonPropertyName("this")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LocalValue? ThisObject { get; set; }

    /// <summary>
    /// Gets the list of arguments to pass to the function.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array. An empty list therefore means "not specified": the property is omitted from the
    /// JSON payload entirely. Add entries to the list to pass arguments to the function.
    /// </remarks>
    [JsonIgnore]
    public List<LocalValue> Arguments { get; } = [];

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
    /// Gets or sets a value indicating whether the call is treated as user-activated, as though the user had
    /// interacted with the page, which APIs gated on a user gesture require. When omitted, is treated as if false.
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
    internal List<LocalValue>? SerializableArguments
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
