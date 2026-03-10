// <copyright file="ObservableEventNameAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Marks an <see cref="ObservableEvent{T}"/> property with its event name string so that
/// Roslyn analyzers can read the name from compiled metadata as well as from source.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ObservableEventNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEventNameAttribute"/> class.
    /// </summary>
    /// <param name="eventName">The event name string passed to the <see cref="ObservableEvent{T}"/> constructor.</param>
    public ObservableEventNameAttribute(string eventName)
    {
        this.EventName = eventName;
    }

    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName { get; }
}
