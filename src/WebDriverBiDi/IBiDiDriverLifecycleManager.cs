// <copyright file="IBiDiDriverLifecycleManager.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Interface for a driver that manages its lifecycle, including starting and stopping the driver.
/// This interface is implemented by <see cref="BiDiDriver"/> and can be used for testing, or to
/// allow users to implement their own driver classes. It extends <see cref="IAsyncDisposable"/>
/// to allow for proper asynchronous disposal of resources.
/// </summary>
/// <remarks>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </remarks>
public interface IBiDiDriverLifecycleManager : IAsyncDisposable
{
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
    Task StartAsync(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}
