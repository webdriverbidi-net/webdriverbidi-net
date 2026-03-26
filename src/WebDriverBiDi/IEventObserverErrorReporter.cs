// <copyright file="IEventObserverErrorReporter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Provides an internal sink for observer failures that happen after an event
/// handler has already returned control to the caller.
/// </summary>
internal interface IEventObserverErrorReporter
{
    /// <summary>
    /// Gets the callback used to report a late observer execution error.
    /// </summary>
    Func<EventObserverErrorInfo, Task> EventObserverErrorReporter { get; }
}
