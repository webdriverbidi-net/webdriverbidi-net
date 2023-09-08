// <copyright file="Initiator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;
using WebDriverBiDi.Script;

/// <summary>
/// The initiator of a network traffic item.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
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
    internal Initiator()
    {
    }

    /// <summary>
    /// Gets the type of entity initiating the request.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public InitiatorType Type { get => this.type; internal set => this.type = value; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonProperty("columnNumber")]
    public ulong? ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    /// <summary>
    /// Gets the column number of the script initiating the request.
    /// </summary>
    [JsonProperty("lineNumber")]
    public ulong? LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    /// <summary>
    /// Gets the stack trace of the script initiating the request.
    /// </summary>
    [JsonProperty("stackTrace")]
    public StackTrace? StackTrace { get => this.stackTrace; internal set => this.stackTrace = value; }

    /// <summary>
    /// Gets the ID of the request.
    /// </summary>
    [JsonProperty("request")]
    public string? RequestId { get => this.requestId; internal set => this.requestId = value; }
}