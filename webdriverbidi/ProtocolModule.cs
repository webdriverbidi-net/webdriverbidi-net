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
    /// Registers an invoker for a given event.
    /// </summary>
    /// <typeparam name="T">The type of data used in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    protected virtual void RegisterEventInvoker<T>(string eventName, Action<EventInvocationData<T>> eventInvoker)
    {
        this.eventInvokers[eventName] = new WebDriverBidiEventData<T>(eventInvoker);
        this.driver.RegisterEvent<T>(eventName);
    }

    private void OnDriverEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        if (this.eventInvokers.ContainsKey(e.EventName))
        {
            WebDriverBidiEventData eventData = this.eventInvokers[e.EventName];
            eventData.InvokeEvent(e.EventData!, e.AdditionalData);
        }
    }
}