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
    /// Creates an object derived from WebDriverBidiEventArgs which contains information about an event.
    /// </summary>
    /// <typeparam name="TEventArgs">
    /// A type derived from WebDriverBidiEventArgs. The type must be the same as type T of this class,
    /// or must have a public constructor that takes an argument of type T.
    /// </typeparam>
    /// <returns>The object containing the information about the event.</returns>
    /// <exception cref="WebDriverBidiException">
    /// Thrown when either:
    /// <list type="bulleted">
    ///   <item>
    ///     <description>
    ///       The type of TEventArgs is not the same type as T
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The type TEventArgs does not have a public constructor that takes an argument of type T
    ///     </description>
    ///   </item>
    /// </list>
    /// </exception>
    public TEventArgs ToEventArgs<TEventArgs>()
        where TEventArgs : WebDriverBidiEventArgs
    {
        TEventArgs? result = null;
        if (typeof(T) == typeof(TEventArgs))
        {
            result = this.EventData as TEventArgs;
        }
        else
        {
            try
            {
                // We are trying to create a new instance of TEventArgs here using a
                // constructor that takes an argument of type T
                result = Activator.CreateInstance(typeof(TEventArgs), this.EventData) as TEventArgs;
            }
            catch (MissingMethodException)
            {
            }
        }

        if (result is null)
        {
            throw new WebDriverBidiException($"Could not produce an EventArgs of type {typeof(TEventArgs)} from event info having type {typeof(T)}");
        }

        result.AdditionalData = this.AdditionalData;
        return result!;
    }
}