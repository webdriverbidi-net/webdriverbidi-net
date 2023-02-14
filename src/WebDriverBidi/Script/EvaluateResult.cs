// <copyright file="EvaluateResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Base class for the result of a script evaluation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public class EvaluateResult : CommandResult
{
    private string realmId = string.Empty;
    private EvaluateResultType resultType = EvaluateResultType.Success;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateResult"/> class.
    /// </summary>
    protected EvaluateResult()
    {
    }

    /// <summary>
    /// Gets the type of the result of the script execution.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public EvaluateResultType ResultType { get => this.resultType; internal set => this.resultType = value; }

    /// <summary>
    /// Gets the ID of the realm in which the script was executed.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}