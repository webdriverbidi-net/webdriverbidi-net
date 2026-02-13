// <copyright file="RegularExpressionValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a regular expression.
/// </summary>
public record RegularExpressionValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegularExpressionValue"/> class with a given pattern.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    [JsonConstructor]
    public RegularExpressionValue(string pattern)
        : this(pattern, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegularExpressionValue"/> class with a given pattern and flags.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags used in the regular expression.</param>
    public RegularExpressionValue(string pattern, string? flags)
    {
        this.Pattern = pattern;
        this.Flags = flags;
    }

    /// <summary>
    /// Gets the pattern used in the regular expression.
    /// </summary>
    [JsonPropertyName("pattern")]
    [JsonRequired]
    [JsonInclude]
    public string Pattern { get; internal set; }

    /// <summary>
    /// Gets the flags used in the regular expression.
    /// </summary>
    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Flags { get; internal set; }
}
