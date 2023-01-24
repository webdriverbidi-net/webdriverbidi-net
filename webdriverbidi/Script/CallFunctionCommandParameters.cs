// <copyright file="CallFunctionCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.callFunction command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CallFunctionCommandParameters : CommandParameters<ScriptEvaluateResult>
{
    private readonly List<ArgumentValue> arguments = new();
    private string functionDeclaration;
    private ScriptTarget scriptTarget;
    private bool awaitPromise;
    private ArgumentValue? thisObject;
    private OwnershipModel? ownershipModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallFunctionCommandParameters"/> class.
    /// </summary>
    /// <param name="functionDeclaration">The function declaration.</param>
    /// <param name="scriptTarget">The script target in which to call the function.</param>
    /// <param name="awaitPromise"><see langword="true" /> to await the script evaluation as a Promise; otherwise, <see langword="false" />.</param>
    public CallFunctionCommandParameters(string functionDeclaration, ScriptTarget scriptTarget, bool awaitPromise)
    {
        this.functionDeclaration = functionDeclaration;
        this.scriptTarget = scriptTarget;
        this.awaitPromise = awaitPromise;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.callFunction";

    /// <summary>
    /// Gets or sets the function declaration.
    /// </summary>
    [JsonProperty("functionDeclaration")]
    public string FunctionDeclaration { get => this.functionDeclaration; set => this.functionDeclaration = value; }

    /// <summary>
    /// Gets or sets the script target against which to call the function.
    /// </summary>
    [JsonProperty("target")]
    public ScriptTarget ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to wait for the function execution to complete.
    /// </summary>
    [JsonProperty("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    /// <summary>
    /// Gets or sets the item to use as the 'this' object when the function is called.
    /// </summary>
    [JsonProperty("this", NullValueHandling = NullValueHandling.Ignore)]
    public ArgumentValue? ThisObject { get => this.thisObject; set => this.thisObject = value; }

    /// <summary>
    /// Gets the list of arguments to pass to the function.
    /// </summary>
    public List<ArgumentValue> Arguments => this.arguments;

    /// <summary>
    /// Gets or sets the ownership model to use for objects in the function call.
    /// </summary>
    public OwnershipModel? OwnershipModel { get => this.ownershipModel; set => this.ownershipModel = value; }

    /// <summary>
    /// Gets the ownership model for serialization purposes.
    /// </summary>
    [JsonProperty("resultOwnership", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableOwnershipModel
    {
        get
        {
            if (this.ownershipModel is null)
            {
                return null;
            }

            return this.ownershipModel.Value.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the list of arguments for serialization purposes.
    /// </summary>
    [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
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