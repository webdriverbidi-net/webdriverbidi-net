// <copyright file="EvaluateResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Base class for the result of a script evaluation.
/// </summary>
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public record EvaluateResult : CommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateResult"/> class.
    /// </summary>
    protected EvaluateResult()
    {
    }

    /// <summary>
    /// Gets the type of the result of the script execution.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public EvaluateResultType ResultType { get; internal set; } = EvaluateResultType.Success;

    /// <summary>
    /// Gets the ID of the realm in which the script was executed.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get; internal set; } = string.Empty;
}
