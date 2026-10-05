// <copyright file="ActionTrace.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// The entry of one action in a recording trace, through which the action reports what it waits for.
/// </summary>
internal sealed class ActionTrace
{
    private readonly TraceRecording recording;
    private string? lastLog;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionTrace"/> class.
    /// </summary>
    /// <param name="recording">The recording the action is in.</param>
    /// <param name="callId">The action's ID in the trace.</param>
    public ActionTrace(TraceRecording recording, string callId)
    {
        this.recording = recording;
        this.CallId = callId;
    }

    /// <summary>
    /// Gets the action's ID in the trace.
    /// </summary>
    public string CallId { get; }

    /// <summary>
    /// Adds a line to the action's log, unless it repeats the last.
    /// </summary>
    /// <param name="message">The line.</param>
    public void Log(string message)
    {
        if (message == this.lastLog)
        {
            return;
        }

        this.lastLog = message;
        this.recording.WriteLog(this.CallId, message);
    }
}
