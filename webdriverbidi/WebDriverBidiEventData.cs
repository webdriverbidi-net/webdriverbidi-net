// <copyright file="WebDriverBidiEventData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing data about a WebDriver Bidi event.
/// </summary>
public class WebDriverBidiEventData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiEventData" /> class.
    /// </summary>
    /// <param name="eventArgsType">The type of EventArgs created by the event.</param>
    /// <param name="eventInvoker">The Action to be executed when the event is raised.</param>
    public WebDriverBidiEventData(Type eventArgsType, Action<object> eventInvoker)
    {
        this.EventArgsType = eventArgsType;
        this.EventInvoker = eventInvoker;
    }

    /// <summary>
    /// Gets the type of EventArgs created by the event.
    /// </summary>
    public Type EventArgsType { get; }

    /// <summary>
    /// Gets the Action to be executed when the event is raised.
    /// </summary>
    public Action<object> EventInvoker { get; }
}