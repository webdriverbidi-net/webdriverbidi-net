// <copyright file="WebDriverBidiEventData{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing data about a WebDriver Bidi event where the data type is specifically known.
/// </summary>
/// <typeparam name="T">The type of the data for the event.</typeparam>
public class WebDriverBidiEventData<T> : WebDriverBidiEventData
{
    private readonly Action<EventInvocationData<T>> invoker;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiEventData{T}"/> class.
    /// </summary>
    /// <param name="invoker">The delegate to use when invoking the event.</param>
    public WebDriverBidiEventData(Action<EventInvocationData<T>> invoker)
    {
        this.invoker = invoker;
    }

    /// <summary>
    /// Gets the type of the event data.
    /// </summary>
    public override Type EventArgsType => typeof(T);

    /// <summary>
    /// Invokes the event.
    /// </summary>
    /// <param name="eventData">The data to use when invoking the event.</param>
    /// <param name="additionalData">Additional data passed to the event for invocation.</param>
    /// <exception cref="WebDriverBidiException">
    /// Thrown when the type of the event data is not the type associated with this event data class.
    /// </exception>
    public override void InvokeEvent(object eventData, Dictionary<string, object?> additionalData)
    {
        if (eventData is not T typedEventData)
        {
            throw new WebDriverBidiException($"Unable to cast received event data to {typeof(T)}");
        }

        EventInvocationData<T> invocationData = new(typedEventData, additionalData);
        this.invoker(invocationData);
    }
}