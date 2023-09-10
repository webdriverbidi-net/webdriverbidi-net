// <copyright file="EvaluateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides parameters for the script.evaluate command.
/// </summary>
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
    [JsonIgnore]
    public override string MethodName => "script.evaluate";

    /// <summary>
    /// Gets or sets the expression to evaluate.
    /// </summary>
    [JsonPropertyName("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    /// <summary>
    /// Gets or sets the target against which to evaluate the script.
    /// </summary>
    [JsonPropertyName("target")]
    public Target ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to await the completion of the evaluation of the script.
    /// </summary>
    [JsonPropertyName("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    /// <summary>
    /// Gets or sets the value of the model of ownership of the handles of the values in the script.
    /// </summary>
    [JsonPropertyName("resultOwnership")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public ResultOwnership? ResultOwnership { get => this.resultOwnership; set => this.resultOwnership = value; }

    /// <summary>
    /// Gets or sets the serialization options for serializing results.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public SerializationOptions? SerializationOptions { get => this.serializationOptions; set => this.serializationOptions = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to activate the browsing context when evaluating the script. When omitted, is treated as if false.
    /// </summary>
    [JsonPropertyName("userActivation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public bool? UserActivation { get => this.userActivation; set => this.userActivation = value; }
}