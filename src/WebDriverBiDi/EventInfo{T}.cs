// <copyright file="EventInfo{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics.CodeAnalysis;

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
    public EventInfo(T eventData, ReceivedDataDictionary additionalData)
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
    public ReceivedDataDictionary AdditionalData { get; }

    /// <summary>
    /// Creates an object derived from WebDriverBiDiEventArgs using a factory function.
    /// This overload avoids reflection and is safe for use in AOT and trimmed applications.
    /// </summary>
    /// <typeparam name="TEventArgs">A type derived from WebDriverBiDiEventArgs.</typeparam>
    /// <param name="factory">A function that creates a TEventArgs instance from the event data.</param>
    /// <returns>The object containing the information about the event.</returns>
    public TEventArgs ToEventArgs<TEventArgs>(Func<T, TEventArgs> factory)
        where TEventArgs : WebDriverBiDiEventArgs
    {
        TEventArgs result = factory(this.EventData);
        result.AdditionalData = this.AdditionalData;
        return result;
    }

    /// <summary>
    /// Creates an object derived from WebDriverBiDiEventArgs which contains information about an event.
    /// When T and TEventArgs are different types, this method uses reflection to locate a public
    /// constructor on TEventArgs that accepts an argument of type T. Prefer the overload accepting
    /// a factory function for AOT and trimming compatibility.
    /// </summary>
    /// <typeparam name="TEventArgs">
    /// A type derived from WebDriverBiDiEventArgs. The type must be the same as type T of this class,
    /// or must have a public constructor that takes an argument of type T.
    /// </typeparam>
    /// <returns>The object containing the information about the event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the type of TEventArgs is not the same type as T.
    /// </exception>
    public TEventArgs ToEventArgs<TEventArgs>()
        where TEventArgs : WebDriverBiDiEventArgs
    {
        TEventArgs? result = null;
        if (typeof(T) == typeof(TEventArgs))
        {
            result = this.EventData as TEventArgs;
        }

        if (result is null)
        {
            throw new WebDriverBiDiException($"Could not produce an EventArgs of type {typeof(TEventArgs)} from event info having type {typeof(T)}");
        }

        result.AdditionalData = this.AdditionalData;
        return result;
    }
}
