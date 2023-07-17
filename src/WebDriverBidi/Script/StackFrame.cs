// <copyright file="StackFrame.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Represents a frame within a stack trace for a script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class StackFrame
{
    private int lineNumber = -1;
    private int columnNumber = -1;
    private string functionName = string.Empty;
    private string url = string.Empty;

    [JsonConstructor]
    private StackFrame()
    {
    }

    /// <summary>
    /// Gets the name of the function for this stack frame.
    /// </summary>
    [JsonProperty("functionName")]
    [JsonRequired]
    public string FunctionName { get => this.functionName; internal set => this.functionName = value; }

    /// <summary>
    /// Gets the line number for this stack frame.
    /// </summary>
    [JsonProperty("lineNumber")]
    [JsonRequired]
    public int LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    /// <summary>
    /// Gets the column number for this stack frame.
    /// </summary>
    [JsonProperty("columnNumber")]
    [JsonRequired]
    public int ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    /// <summary>
    /// Gets the URL for this stack frame.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }
}