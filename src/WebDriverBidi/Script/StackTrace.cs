// <copyright file="StackTrace.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing a stack trace from a script.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class StackTrace
{
    private List<StackFrame> callFrames = new();

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
    public IList<StackFrame> CallFrames => this.callFrames.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of stack frames for serialization purposes.
    /// </summary>
    [JsonProperty("callFrames")]
    [JsonRequired]
    internal List<StackFrame> SerializableCallFrames { get => this.callFrames; set => this.callFrames = value; }
}