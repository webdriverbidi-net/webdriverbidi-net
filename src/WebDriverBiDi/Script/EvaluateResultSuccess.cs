// <copyright file="EvaluateResultSuccess.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing the successful evaluation of a script.
/// </summary>
public record EvaluateResultSuccess : EvaluateResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateResultSuccess"/> class.
    /// </summary>
    [JsonConstructor]
    private EvaluateResultSuccess()
        : base()
    {
    }

    /// <summary>
    /// Gets the result of the script evaluation.
    /// </summary>
    [JsonPropertyName("result")]
    [JsonInclude]
    public RemoteValue Result { get; internal set; } = new("null");
}
