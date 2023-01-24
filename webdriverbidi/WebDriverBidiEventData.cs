// <copyright file="WebDriverBidiEventData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing data about a WebDriver Bidi event.
/// </summary>
public abstract class WebDriverBidiEventData
{
    /// <summary>
    /// Gets the type of EventArgs created by the event.
    /// </summary>
    public abstract Type EventArgsType { get; }

    /// <summary>
    /// Invokes the event.
    /// </summary>
    /// <param name="eventData">The data used to pass to the event for invocation.</param>
    public abstract void InvokeEvent(object eventData, Dictionary<string, object?> additionalData);
}