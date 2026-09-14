// <copyright file="EvaluateResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Base class for the result of a script evaluation.
/// </summary>
[JsonConverter(typeof(DiscriminatedUnionJsonConverter<EvaluateResult>))]
[DiscriminatedTypeProperty("type")]
[DiscriminatedDerivedType(typeof(EvaluateResultSuccess), "success")]
[DiscriminatedDerivedType(typeof(EvaluateResultException), "exception")]
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

    /// <summary>
    /// Casts this <see cref="EvaluateResult"/> to a type-specific evaluation result, throwing if it is not of that
    /// type. Use <see cref="TryAs{T}"/> to test without throwing.
    /// </summary>
    /// <typeparam name="T">The specific type of EvaluateResult to return.</typeparam>
    /// <returns>This instance cast to the specified correct type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this EvaluateResult is not the specified type.</exception>
    public T As<T>()
        where T : EvaluateResult
    {
        if (this is T converted)
        {
            return converted;
        }

        throw new WebDriverBiDiException($"EvaluateResult of type '{this.ResultType}' cannot be cast to type '{typeof(T).Name}'");
    }

    /// <summary>
    /// Attempts to cast this <see cref="EvaluateResult"/> to a type-specific evaluation result, returning
    /// <see langword="false"/> rather than throwing when it is not of that type.
    /// </summary>
    /// <typeparam name="T">The specific type of EvaluateResult to return.</typeparam>
    /// <param name="result">When this method returns, contains the cast value or null if the cast failed.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryAs<T>([NotNullWhen(true)] out T? result)
        where T : EvaluateResult
    {
        if (this is T converted)
        {
            result = converted;
            return true;
        }

        result = null;
        return false;
    }
}
