// <copyright file="EvaluateResultSuccess.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing the successful evaluation of a script.
/// </summary>
public class EvaluateResultSuccess : EvaluateResult
{
    private RemoteValue result = new("null");

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateResultSuccess"/> class.
    /// </summary>
    [JsonConstructor]
    internal EvaluateResultSuccess()
        : base()
    {
    }

    /// <summary>
    /// Gets the result of the script evaluation.
    /// </summary>
    [JsonProperty("result")]
    public RemoteValue Result { get => this.result; internal set => this.result = value; }
}
