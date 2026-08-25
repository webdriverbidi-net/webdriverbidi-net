// <copyright file="ObservableEventHandlerOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Enumerated value describing options for the execution of a handler for an ObservableEvent.
/// </summary>
public enum ObservableEventHandlerOptions
{
    /// <summary>
    /// No options, meaning handlers attempt to run synchronously, awaiting the completion of execution. This is the default.
    /// </summary>
    RunHandlerSynchronously = 0,

    /// <summary>
    /// The handler's completion is not awaited by the event dispatcher. Order of multiple executions of the handler is not guaranteed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This option changes what the dispatcher does with the <see cref="System.Threading.Tasks.Task"/> a handler
    /// returns, not where the handler starts executing. Every handler is invoked on the thread that is
    /// dispatching the event (the transport's message-processing thread for driver events). For a
    /// <c>Func&lt;T, Task&gt;</c> handler, the code that runs before the handler returns its
    /// <see cref="System.Threading.Tasks.Task"/> — in an <c>async</c> lambda, everything up to the first
    /// <c>await</c> that does not complete synchronously — still runs on the dispatching thread; only the
    /// remainder is detached. Blocking calls placed before that first <c>await</c>, or a non-<c>async</c>
    /// <c>Task</c>-returning handler that does its work synchronously and returns a completed task, block
    /// the dispatcher regardless of this option.
    /// </para>
    /// <para>
    /// Handlers added with the <c>Action&lt;T&gt;</c> overload of <c>AddObserver</c> are the exception: with
    /// this option the whole action is queued to the thread pool, so none of it runs on the dispatching thread.
    /// </para>
    /// <para>
    /// To offload blocking work from a <c>Task</c>-returning handler, make it <c>async</c> and place an
    /// <c>await</c> (for example <c>await Task.Yield()</c>) before the blocking work, or wrap the work in
    /// <c>Task.Run</c>. The BIDI007 and BIDI023 analyzers report handlers where this option cannot help.
    /// </para>
    /// </remarks>
    RunHandlerAsynchronously = 1,
}
