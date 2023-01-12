// <copyright file="ExceptionDetails.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object containing the details of an exception thrown by a script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ExceptionDetails
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
    [JsonProperty("text")]
    [JsonRequired]
    public string Text { get => this.text; internal set => this.text = value; }

    /// <summary>
    /// Gets the column number of the statement that caused the exception.
    /// </summary>
    [JsonProperty("columnNumber")]
    [JsonRequired]
    public int ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    /// <summary>
    /// Gets the line number of the statement that caused the exception.
    /// </summary>
    [JsonProperty("lineNumber")]
    [JsonRequired]
    public int LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    /// <summary>
    /// Gets the stack trace of the exception.
    /// </summary>
    [JsonProperty("stackTrace")]
    [JsonRequired]
    public StackTrace StackTrace { get => this.stackTrace; internal set => this.stackTrace = value; }

    /// <summary>
    /// Gets the RemoteValue representing the value of the exception.
    /// </summary>
    [JsonProperty("exception")]
    [JsonRequired]
    public RemoteValue Exception { get => this.exception; internal set => this.exception = value; }
}