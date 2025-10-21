// <copyright file="ExceptionDetails.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing the details of an exception thrown by a script.
/// </summary>
public record ExceptionDetails
{
    private int columnNumber = -1;
    private int lineNumber = -1;
    private string text = string.Empty;
    private StackTrace stackTrace = new();
    private RemoteValue exception = new("null");

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDetails"/> class.
    /// </summary>
    [JsonConstructor]
    internal ExceptionDetails()
    {
    }

    /// <summary>
    /// Gets the text of the exception.
    /// </summary>
    [JsonPropertyName("text")]
    [JsonRequired]
    [JsonInclude]
    public string Text { get => this.text; private set => this.text = value; }

    /// <summary>
    /// Gets the column number of the statement that caused the exception.
    /// </summary>
    [JsonPropertyName("columnNumber")]
    [JsonRequired]
    [JsonInclude]
    public int ColumnNumber { get => this.columnNumber; private set => this.columnNumber = value; }

    /// <summary>
    /// Gets the line number of the statement that caused the exception.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    [JsonRequired]
    [JsonInclude]
    public int LineNumber { get => this.lineNumber; private set => this.lineNumber = value; }

    /// <summary>
    /// Gets the stack trace of the exception.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonRequired]
    [JsonInclude]
    public StackTrace StackTrace { get => this.stackTrace; private set => this.stackTrace = value; }

    /// <summary>
    /// Gets the RemoteValue representing the value of the exception.
    /// </summary>
    [JsonPropertyName("exception")]
    [JsonRequired]
    [JsonInclude]
    public RemoteValue Exception { get => this.exception; private set => this.exception = value; }
}
