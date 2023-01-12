// <copyright file="ScriptEvaluateResultSuccess.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing the successful evaluation of a script.
/// </summary>
public class ScriptEvaluateResultSuccess : ScriptEvaluateResult
{
    private RemoteValue result = new("null");

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptEvaluateResultSuccess"/> class.
    /// </summary>
    [JsonConstructor]
    internal ScriptEvaluateResultSuccess()
        : base()
    {
    }

    /// <summary>
    /// Gets the result of the script evaluation.
    /// </summary>
    [JsonProperty("result")]
    public RemoteValue Result { get => this.result; internal set => this.result = value; }
}