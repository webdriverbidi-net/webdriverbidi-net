// <copyright file="Module.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Base class representing a module in the WebDriver Bidi protocol.
/// </summary>
public abstract class Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Module"/> class.
    /// </summary>
    /// <param name="driver">The driver used for communication by the module.</param>
    protected Module(IBiDiCommandExecutor driver)
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
    protected IBiDiCommandExecutor Driver { get; }

    /// <summary>
    /// Registers an event so that when it is received, the deserialized data is forwarded
    /// directly to the given <see cref="ObservableEvent{T}"/>. Use this overload when the
    /// deserialized type <typeparamref name="T"/> is the same as the event args type.
    /// </summary>
    /// <typeparam name="T">The type of event args, which must also be the deserialized event data type.</typeparam>
    /// <param name="observableEvent">The <see cref="ObservableEvent{T}"/> to notify when the event is received.</param>
    protected void RegisterObservableEvent<T>(ObservableEvent<T> observableEvent)
        where T : WebDriverBiDiEventArgs
    {
        this.ConfigureObserverErrorReporting(observableEvent);

        async Task EventInvoker(EventInfo<T> eventData)
        {
            T eventArgs = eventData.ToEventArgs<T>();
            await observableEvent.NotifyObserversAsync(eventArgs).ConfigureAwait(false);
        }

        this.Driver.RegisterEvent<T>(observableEvent.EventName, EventInvoker);
    }

    /// <summary>
    /// Registers an event so that when it is received, the deserialized data is converted
    /// using the provided factory and forwarded to the given <see cref="ObservableEvent{TEventArgs}"/>.
    /// Use this overload when the deserialized type <typeparamref name="T"/> differs from the
    /// event args type <typeparamref name="TEventArgs"/>.
    /// </summary>
    /// <typeparam name="T">The deserialized event data type.</typeparam>
    /// <typeparam name="TEventArgs">The event args type to produce and forward.</typeparam>
    /// <param name="observableEvent">The <see cref="ObservableEvent{T}"/> to notify when the event is received.</param>
    /// <param name="eventArgsConverter">A function that creates a <typeparamref name="TEventArgs"/> from the deserialized data.</param>
    protected void RegisterObservableEvent<T, TEventArgs>(ObservableEvent<TEventArgs> observableEvent, Func<T, TEventArgs> eventArgsConverter)
        where TEventArgs : WebDriverBiDiEventArgs
    {
        this.ConfigureObserverErrorReporting(observableEvent);

        async Task EventInvoker(EventInfo<T> eventData)
        {
            TEventArgs eventArgs = eventData.ToEventArgs(eventArgsConverter);
            await observableEvent.NotifyObserversAsync(eventArgs).ConfigureAwait(false);
        }

        this.Driver.RegisterEvent<T>(observableEvent.EventName, EventInvoker);
    }

    private void ConfigureObserverErrorReporting<T>(ObservableEvent<T> observableEvent)
        where T : WebDriverBiDiEventArgs
    {
        if (this.Driver is IEventObserverErrorReporter observerErrorReporter)
        {
            observableEvent.SetObserverErrorReporter(observerErrorReporter.EventObserverErrorReporter);
        }
    }
}
