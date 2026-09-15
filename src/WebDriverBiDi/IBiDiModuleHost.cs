// <copyright file="IBiDiModuleHost.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Interface for a driver that executes commands and registers events for use with the WebDriver
/// BiDi protocol. This interface is implemented by <see cref="BiDiDriver"/> and can be used for
/// testing, or to allow users to implement their own driver classes.
/// </summary>
/// <remarks>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </remarks>
public interface IBiDiModuleHost
{
    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandParameters"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="commandTimeout"/> is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds the maximum supported timer duration.</exception>
    /// <exception cref="WebDriverBiDiCommandException">Thrown when the remote end returns an error response for the command.</exception>
    /// <exception cref="WebDriverBiDiSerializationException">Thrown when the command parameters cannot be serialized, or the response cannot be deserialized.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the command does not complete within the timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the driver is not connected to a remote end, or the connection is lost before the command completes.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown when the command is canceled, or its response carries no result or a result of the wrong type.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the driver has been disposed.</exception>
    Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
        where T : CommandResult;

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandParameters"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="commandTimeout"/> is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds the maximum supported timer duration.</exception>
    /// <exception cref="WebDriverBiDiCommandException">Thrown when the remote end returns an error response for the command.</exception>
    /// <exception cref="WebDriverBiDiSerializationException">Thrown when the command parameters cannot be serialized, or the response cannot be deserialized.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the command does not complete within the timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the driver is not connected to a remote end, or the connection is lost before the command completes.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown when the command is canceled, or its response carries no result or a result of the wrong type.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the driver has been disposed.</exception>
    Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
        where T : CommandResult;

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker);
}
