// <copyright file="ProtocolModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Base class representing a module in the WebDriver Bidi protocol.
/// </summary>
public abstract class ProtocolModule
{
    private readonly Driver driver;
    private readonly Dictionary<string, WebDriverBidiEventData> eventInvokers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The driver used for communication by the module.</param>
    protected ProtocolModule(Driver driver)
    {
        this.driver = driver;
        this.driver.EventReceived += this.OnDriverEventReceived;
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// Gets the driver used for communication by the module.
    /// </summary>
    protected Driver Driver => this.driver;

    /// <summary>
    /// Registers an invoker for an event sent by the protocol.
    /// </summary>
    /// <param name="eventName">The name of the event for which to register the invoker.</param>
    /// <param name="eventArgsType">The type of EventArgs used when the event is raised.</param>
    /// <param name="eventInvoker">The Action used to raise the event.</param>
    protected void RegisterEventInvoker(string eventName, Type eventArgsType, Action<object> eventInvoker)
    {
        this.eventInvokers[eventName] = new WebDriverBidiEventData(eventArgsType, eventInvoker);
        this.driver.RegisterEvent(eventName, eventArgsType);
    }

    private void OnDriverEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        if (this.eventInvokers.ContainsKey(e.EventName))
        {
            var eventData = this.eventInvokers[e.EventName];
            var eventArgs = e.EventData;
            if (eventArgs is null || !eventArgs.GetType().IsAssignableTo(eventData.EventArgsType))
            {
                throw new WebDriverBidiException($"Unable to cast received event data to {eventData.EventArgsType.Name}");
            }

            eventData.EventInvoker(eventArgs);
        }
    }
}