// <copyright file="EventObserverPriority.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Enumerated value describing the priority of execution of an <see cref="EventObserver{T}"/>.
/// </summary>
internal enum EventObserverPriority
{
    /// <summary>
    /// Observer is a data collection observer, and should run before event handling observers.
    /// </summary>
    DataCollectorObserverPriority,

    /// <summary>
    /// Observer is an event handling observer.
    /// </summary>
    NormalObserverPriority,
}
