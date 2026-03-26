// <copyright file="EventObserverErrorInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Describes a failure raised by an observable event observer.
/// </summary>
public sealed record EventObserverErrorInfo
{
    /// <summary>
    /// Gets the name of the observable event whose observer faulted.
    /// </summary>
    public required string ObservableEventName { get; init; }

    /// <summary>
    /// Gets the identifier of the observer that faulted.
    /// </summary>
    public required string ObserverId { get; init; }

    /// <summary>
    /// Gets the description of the observer that faulted.
    /// </summary>
    public required string ObserverDescription { get; init; }

    /// <summary>
    /// Gets the exception thrown by the observer.
    /// </summary>
    public required Exception Exception { get; init; }

    /// <summary>
    /// Gets a value indicating whether the observer was configured to run asynchronously.
    /// </summary>
    public required bool IsAsynchronousHandler { get; init; }

    /// <summary>
    /// Gets a value indicating whether the fault occurred after control returned to the caller.
    /// </summary>
    public required bool FaultOccurredAfterHandlerReturned { get; init; }
}
