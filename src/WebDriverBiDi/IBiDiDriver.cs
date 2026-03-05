// <copyright file="IBiDiDriver.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;

/// <summary>
/// Interface for a driver that uses the WebDriver BiDi protocol. This interface is implemented by
/// <see cref="BiDiDriver"/> and can be used for testing, or to allow users to implement their own
/// driver classes. It extends <see cref="IAsyncDisposable"/> to allow for proper asynchronous
/// disposal of resources.
/// </summary>
/// <remarks>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </remarks>
public interface IBiDiDriver : IAsyncDisposable
{
    /// <summary>
    /// Gets an observable event that notifies when a protocol event is received from protocol transport.
    /// </summary>
    ObservableEvent<EventReceivedEventArgs> OnEventReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when a protocol error is received from protocol transport.
    /// </summary>
    ObservableEvent<ErrorReceivedEventArgs> OnUnexpectedErrorReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from protocol transport.
    /// </summary>
    ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by this driver.
    /// </summary>
    ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }

    /// <summary>
    /// Gets a value indicating whether the driver has started communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    bool IsStarted { get; }

    /// <summary>
    /// Asynchronously starts the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task StartAsync(string connectionString, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for the
    /// default command timeout. The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, CancellationToken cancellationToken = default)
        where T : CommandResult;

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, TimeSpan commandTimeout, CancellationToken cancellationToken)
        where T : CommandResult;

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for the
    /// default command timeout.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, CancellationToken cancellationToken)
        where T : CommandResult;

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, TimeSpan commandTimeout, CancellationToken cancellationToken)
        where T : CommandResult;

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker);

    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    void RegisterModule(Module module);

    /// <summary>
    /// Gets a module from the set of registered modules for this driver.
    /// </summary>
    /// <typeparam name="T">A module object which is a subclass of <see cref="Module"/>.</typeparam>
    /// <param name="moduleName">The name of the module to return.</param>
    /// <returns>The protocol module object.</returns>
    T GetModule<T>(string moduleName)
        where T : Module;

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
}