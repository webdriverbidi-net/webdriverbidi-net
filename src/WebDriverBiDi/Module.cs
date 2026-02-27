// <copyright file="Module.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Protocol;

/// <summary>
/// Base class representing a module in the WebDriver Bidi protocol.
/// </summary>
public abstract class Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Module"/> class.
    /// </summary>
    /// <param name="driver">The driver used for communication by the module.</param>
    protected Module(BiDiDriver driver)
    {
        this.Driver = driver;
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// Gets the driver used for communication by the module.
    /// </summary>
    protected BiDiDriver Driver { get; }

    /// <summary>
    /// Registers an invoker for a given event.
    /// </summary>
    /// <typeparam name="T">The type of data used in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    protected virtual void RegisterAsyncEventInvoker<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
    {
        this.Driver.RegisterEvent<T>(eventName, eventInvoker);
    }
}
