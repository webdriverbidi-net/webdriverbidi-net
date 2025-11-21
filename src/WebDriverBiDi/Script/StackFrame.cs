// <copyright file="StackFrame.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a frame within a stack trace for a script.
/// </summary>
public record StackFrame
{
    [JsonConstructor]
    private StackFrame()
    {
    }

    /// <summary>
    /// Gets the name of the function for this stack frame.
    /// </summary>
    [JsonPropertyName("functionName")]
    [JsonRequired]
    [JsonInclude]
    public string FunctionName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the line number for this stack frame.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    [JsonRequired]
    [JsonInclude]
    public int LineNumber { get; private set; } = -1;

    /// <summary>
    /// Gets the column number for this stack frame.
    /// </summary>
    [JsonPropertyName("columnNumber")]
    [JsonRequired]
    [JsonInclude]
    public int ColumnNumber { get; private set; } = -1;

    /// <summary>
    /// Gets the URL for this stack frame.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; private set; } = string.Empty;
}
