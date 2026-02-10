// <copyright file="Source.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing the source for a script.
/// </summary>
public record Source
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Source"/> class.
    /// </summary>
    [JsonConstructor]
    internal Source()
    {
    }

    /// <summary>
    /// Gets the ID of the realm for a script.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the browsing context ID for a script.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Context { get; internal set; }
}
