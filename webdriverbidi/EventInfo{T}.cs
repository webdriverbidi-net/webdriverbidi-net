// <copyright file="EventInfo{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Class containing information used in the invocation of an event invoker.
/// </summary>
/// <typeparam name="T">The type of data describing the event.</typeparam>
public class EventInfo<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventInfo{T}"/> class.
    /// </summary>
    /// <param name="eventData">The data for the event invocation.</param>
    /// <param name="additionalData">Additional data returned for the event.</param>
    public EventInfo(T eventData, Dictionary<string, object?> additionalData)
    {
        this.EventData = eventData;
        this.AdditionalData = additionalData;
    }

    /// <summary>
    /// Gets the data for the event invocation.
    /// </summary>
    public T EventData { get; }

    /// <summary>
    /// Gets additional data returned for the event.
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get; }
}