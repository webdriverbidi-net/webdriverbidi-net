// <copyright file="IEventObserverErrorReporter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Provides a sink for observer failures that happen after an event handler has already returned
/// control to the caller.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the library's other interfaces, this one <em>is</em> meant to be implemented: a custom
/// <see cref="IBiDiCommandExecutor"/> implements it to receive the failures of observers of the events
/// its modules raise. <see cref="BiDiDriver"/> implements it already, so an application using the
/// driver needs nothing here.
/// </para>
/// <para>
/// A failure of this kind cannot be thrown at the code that raised the event, because the handler has
/// already returned: it is the failure of a handler registered with
/// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>, whose task the dispatcher does
/// not await. <see cref="Module"/> installs the reporter of the executor it is constructed with on
/// every event it registers, and where the executor does not implement this interface such a failure is
/// observed — so it never surfaces as a
/// <see cref="System.Threading.Tasks.TaskScheduler.UnobservedTaskException"/> — and then discarded.
/// </para>
/// <para>
/// The reporter is read at the moment a failure is reported rather than captured when an observer is
/// added, so an implementation may return a different callback over time. It should not throw; a
/// failure of the reporter itself has nowhere further to go.
/// </para>
/// </remarks>
public interface IEventObserverErrorReporter
{
    /// <summary>
    /// Gets the callback used to report a late observer execution error.
    /// </summary>
    Func<EventObserverErrorInfo, Task> EventObserverErrorReporter { get; }
}
