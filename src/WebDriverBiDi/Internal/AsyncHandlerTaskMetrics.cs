// <copyright file="AsyncHandlerTaskMetrics.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

/// <summary>
/// Maintains a process-global count of in-flight asynchronous event handler tasks and
/// publishes changes to <see cref="WebDriverBiDiEventSource.AsyncHandlerTaskCount(int)"/>.
/// </summary>
/// <remarks>
/// A static non-generic counter is used rather than a field on <see cref="EventObserver{T}"/>,
/// because a static field on a generic type has one instance per generic instantiation and
/// would not yield a process-wide total. Interlocked operations provide the necessary memory
/// barriers; the volatile keyword is not required.
/// </remarks>
internal static class AsyncHandlerTaskMetrics
{
    private static int inFlightCount;

    /// <summary>
    /// Increments the in-flight counter by one and publishes the new value to
    /// <see cref="WebDriverBiDiEventSource.AsyncHandlerTaskCount(int)"/>.
    /// </summary>
    public static void IncrementInFlight()
    {
        int newCount = Interlocked.Increment(ref inFlightCount);
        WebDriverBiDiEventSource.RaiseEvent.AsyncHandlerTaskCount(newCount);
    }

    /// <summary>
    /// Decrements the in-flight counter by one and publishes the new value to
    /// <see cref="WebDriverBiDiEventSource.AsyncHandlerTaskCount(int)"/>.
    /// </summary>
    public static void DecrementInFlight()
    {
        int newCount = Interlocked.Decrement(ref inFlightCount);
        WebDriverBiDiEventSource.RaiseEvent.AsyncHandlerTaskCount(newCount);
    }
}
