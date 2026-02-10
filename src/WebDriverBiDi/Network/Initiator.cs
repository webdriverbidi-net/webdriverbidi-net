// <copyright file="Initiator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The initiator of a network traffic item.
/// </summary>
public record Initiator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Initiator"/> class.
    /// </summary>
    [JsonConstructor]
    private Initiator()
    {
    }

    /// <summary>
    /// Gets the type of entity initiating the request.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonInclude]
    public InitiatorType? Type { get; internal set; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonPropertyName("columnNumber")]
    [JsonInclude]
    public ulong? ColumnNumber { get; internal set; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    [JsonInclude]
    public ulong? LineNumber { get; internal set; }

    /// <summary>
    /// Gets the stack trace of the script initiating the request.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonInclude]
    public StackTrace? StackTrace { get; internal set; }

    /// <summary>
    /// Gets the ID of the request.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonInclude]
    public string? RequestId { get; internal set; }
}
