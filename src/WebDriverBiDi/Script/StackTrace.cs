// <copyright file="StackTrace.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a stack trace from a script.
/// </summary>
public record StackTrace
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StackTrace"/> class.
    /// </summary>
    [JsonConstructor]
    internal StackTrace()
    {
    }

    /// <summary>
    /// Gets the read-only list of stack frames for this stack trace.
    /// </summary>
    public IList<StackFrame> CallFrames => this.SerializableCallFrames.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of stack frames for serialization purposes.
    /// </summary>
    [JsonPropertyName("callFrames")]
    [JsonRequired]
    [JsonInclude]
    internal List<StackFrame> SerializableCallFrames { get; set; } = [];
}
