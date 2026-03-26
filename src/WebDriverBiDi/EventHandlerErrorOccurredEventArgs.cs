// <copyright file="EventHandlerErrorOccurredEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Provides data for the event raised when an error occurs in an observer of an observable event.
/// This event is for diagnostic and observability purposes, and does not prevent the propagation
/// of the error back to the Transport class.
/// </summary>
public record EventHandlerErrorOccurredEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventHandlerErrorOccurredEventArgs"/> class.
    /// </summary>
    /// <param name="errorInfo">The error information describing the error that occurred in the event handler.</param>
    public EventHandlerErrorOccurredEventArgs(EventObserverErrorInfo errorInfo)
    {
        this.ErrorInfo = errorInfo;
    }

    /// <summary>
    /// Gets the error information describing the error that occurred in the event handler.
    /// </summary>
    public EventObserverErrorInfo ErrorInfo { get; private set; }
}
