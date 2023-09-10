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
public class Initiator
{
    private InitiatorType type = InitiatorType.Other;
    private ulong? columnNumber;
    private ulong? lineNumber;
    private StackTrace? stackTrace;
    private string? requestId;

    /// <summary>
    /// Initializes a new instance of the <see cref="Initiator"/> class.
    /// </summary>
    public Initiator()
    {
    }

    /// <summary>
    /// Gets the type of entity initiating the request.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public InitiatorType Type { get => this.type; internal set => this.type = value; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonPropertyName("columnNumber")]
    [JsonInclude]
    public ulong? ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    [JsonInclude]
    public ulong? LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    /// <summary>
    /// Gets the stack trace of the script initiating the request.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonInclude]
    public StackTrace? StackTrace { get => this.stackTrace; internal set => this.stackTrace = value; }

    /// <summary>
    /// Gets the ID of the request.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonInclude]
    public string? RequestId { get => this.requestId; internal set => this.requestId = value; }
}