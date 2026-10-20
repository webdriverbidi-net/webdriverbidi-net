// <copyright file="IEventObserverErrorReporter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Provides a sink for the failures of observable event observers.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the library's other interfaces, this one <em>is</em> meant to be implemented: a custom
/// <see cref="IBiDiModuleHost"/> implements it to receive the failures of observers of the events
/// its modules raise. <see cref="BiDiDriver"/> implements it already, so an application using the
/// driver needs nothing here.
/// </para>
/// <para>
/// Every observer failure is reported here rather than thrown at the code that raised the event: a
/// synchronously-run handler's as it happens, and the failure of a handler registered with
/// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>, whose task the dispatcher does
/// not await, after the handler has returned. <see cref="Module"/> installs the reporter of the executor
/// it is constructed with on every event it registers. Where the executor does not implement this
/// interface, a synchronously-run handler's failure propagates to the code that raised the event, and an
/// asynchronously-run handler's failure is observed — so it never surfaces as a
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
