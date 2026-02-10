// <copyright file="EvaluateResultException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing the evaluation of a script that throws an exception.
/// </summary>
public record EvaluateResultException : EvaluateResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateResultException"/> class.
    /// </summary>
    [JsonConstructor]
    private EvaluateResultException()
        : base()
    {
    }

    /// <summary>
    /// Gets the exception details of the script evaluation.
    /// </summary>
    [JsonPropertyName("exceptionDetails")]
    [JsonInclude]
    public ExceptionDetails ExceptionDetails { get; internal set; } = new();
}
