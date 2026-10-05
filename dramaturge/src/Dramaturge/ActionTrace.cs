// <copyright file="ActionTrace.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Script;

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
    /// <param name="page">The page the action is taken on.</param>
    public ActionTrace(TraceRecording recording, string callId, Page page)
    {
        this.recording = recording;
        this.CallId = callId;
        this.Page = page;
    }

    /// <summary>
    /// Gets the action's ID in the trace.
    /// </summary>
    public string CallId { get; }

    /// <summary>
    /// Gets the page the action is taken on.
    /// </summary>
    public Page Page { get; }

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

    /// <summary>
    /// Records the element the action acts on, as it acts: a snapshot with the element marked, if the recording
    /// takes snapshots, and the point it acts at, offset from the element's center.
    /// </summary>
    /// <param name="frame">The element's frame.</param>
    /// <param name="target">The element.</param>
    /// <param name="offset">The point's offset from the element's center, or <see langword="null"/> for an action without one.</param>
    /// <returns>A task that completes when the element is recorded.</returns>
    public Task TargetAsync(Frame frame, NodeRemoteValue target, PointerOffset? offset)
    {
        return this.recording.RecordTargetAsync(this, frame, target, offset);
    }
}
