// <copyright file="EvaluateCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Provides parameters for the script.evaluate command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class EvaluateCommandParameters : CommandParameters<EvaluateResult>
{
    private string expression;
    private Target scriptTarget;
    private bool awaitPromise;
    private ResultOwnership? resultOwnership;
    private SerializationOptions? serializationOptions;
    private bool? userActivation;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateCommandParameters"/> class.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="scriptTarget">The target of the script to evaluate against.</param>
    /// <param name="awaitPromise"><see langword="true" /> to await the script evaluation as a Promise; otherwise, <see langword="false" />.</param>
    public EvaluateCommandParameters(string expression, Target scriptTarget, bool awaitPromise)
    {
        this.expression = expression;
        this.scriptTarget = scriptTarget;
        this.awaitPromise = awaitPromise;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.evaluate";

    /// <summary>
    /// Gets or sets the expression to evaluate.
    /// </summary>
    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    /// <summary>
    /// Gets or sets the target against which to evaluate the script.
    /// </summary>
    [JsonProperty("target")]
    public Target ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to await the completion of the evaluation of the script.
    /// </summary>
    [JsonProperty("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    /// <summary>
    /// Gets or sets the value of the model of ownership of the handles of the values in the script.
    /// </summary>
    [JsonProperty("resultOwnership", NullValueHandling = NullValueHandling.Ignore)]
    public ResultOwnership? ResultOwnership { get => this.resultOwnership; set => this.resultOwnership = value; }

    /// <summary>
    /// Gets or sets the serialization options for serializing results.
    /// </summary>
    [JsonProperty("serializationOptions", NullValueHandling = NullValueHandling.Ignore)]
    public SerializationOptions? SerializationOptions { get => this.serializationOptions; set => this.serializationOptions = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to activate the browsing context when evaluating the script. When omitted, is treated as if false.
    /// </summary>
    [JsonProperty("userActivation", NullValueHandling = NullValueHandling.Ignore)]
    public bool? UserActivation { get => this.userActivation; set => this.userActivation = value; }
}