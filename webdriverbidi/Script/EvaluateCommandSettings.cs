// <copyright file="EvaluateCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.evaluate command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class EvaluateCommandSettings : CommandSettings
{
    private string expression;
    private ScriptTarget scriptTarget;
    private bool awaitPromise;
    private OwnershipModel? ownershipModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateCommandSettings"/> class.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="scriptTarget">The target of the script to evaluate against.</param>
    /// <param name="awaitPromise"><see langword="true" /> to await the script evaluation as a Promise; otherwise, <see langword="false" />.</param>
    public EvaluateCommandSettings(string expression, ScriptTarget scriptTarget, bool awaitPromise)
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
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(ScriptEvaluateResult);

    /// <summary>
    /// Gets or sets the expression to evaluate.
    /// </summary>
    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    /// <summary>
    /// Gets or sets the target against which to evaluate the script.
    /// </summary>
    [JsonProperty("target")]
    public ScriptTarget ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to await the completion of the evaluation of the script.
    /// </summary>
    [JsonProperty("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    /// <summary>
    /// Gets or sets the value of the model of ownership of the handles of the values in the script.
    /// </summary>
    public OwnershipModel? OwnershipModel { get => this.ownershipModel; set => this.ownershipModel = value; }

    /// <summary>
    /// Gets the value of the ownership model for serialization purposes.
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
}