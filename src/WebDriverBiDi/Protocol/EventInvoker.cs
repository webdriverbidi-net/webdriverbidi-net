// <copyright file="EventInvoker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing data about a WebDriver Bidi event.
/// </summary>
public abstract class EventInvoker
{
    /// <summary>
    /// Asynchronously invokes the event.
    /// </summary>
    /// <param name="eventData">The data to use when invoking the event.</param>
    /// <param name="additionalData">Additional data passed to the event for invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the type of the event data is not the type associated with this event data class.
    /// </exception>
    public abstract Task InvokeEventAsync(object eventData, ReceivedDataDictionary additionalData);
}
