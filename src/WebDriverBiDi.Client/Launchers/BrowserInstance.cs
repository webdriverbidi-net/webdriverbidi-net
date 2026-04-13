// <copyright file="BrowserInstance.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Represents a running browser instance that has been launched and is ready to accept connections.
/// Implements <see cref="IAsyncDisposable"/> to ensure proper cleanup of browser processes.
/// </summary>
public class BrowserInstance : IAsyncDisposable
{
    private readonly BrowserLauncher launcher;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserInstance"/> class.
    /// </summary>
    /// <param name="launcher">The launcher that created this instance.</param>
    /// <param name="webSocketUrl">The WebSocket URL for connecting to the browser.</param>
    /// <param name="processId">The process ID of the browser, or 0 if not applicable.</param>
    internal BrowserInstance(BrowserLauncher launcher, string webSocketUrl, int processId)
    {
        this.launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        this.WebSocketUrl = webSocketUrl ?? throw new ArgumentNullException(nameof(webSocketUrl));
        this.ProcessId = processId;
    }

    /// <summary>
    /// Gets the WebSocket URL for connecting to the browser via WebDriver BiDi.
    /// Use this URL with <see cref="BiDiDriver.StartAsync"/> to establish a connection.
    /// </summary>
    public string WebSocketUrl { get; }

    /// <summary>
    /// Gets the process ID of the browser process, or 0 if the browser is remote or the process ID is not available.
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Gets a value indicating whether the browser process is still running.
    /// For remote browsers, this always returns true until the instance is disposed.
    /// </summary>
    public bool IsRunning => !this.disposed && this.launcher.IsRunning;

    /// <summary>
    /// Asynchronously closes the browser gracefully. If the browser cannot be closed gracefully
    /// within a reasonable timeout, it will be forcefully terminated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser cannot be closed.</exception>
    public async Task CloseAsync()
    {
        if (this.disposed)
        {
            return;
        }

        await this.launcher.QuitBrowserAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Forcefully terminates the browser process immediately.
    /// This should only be used when graceful shutdown via <see cref="CloseAsync"/> fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task KillAsync()
    {
        if (this.disposed)
        {
            return;
        }

        await this.launcher.StopAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously disposes the browser instance. This will attempt to close the browser gracefully,
    /// and if that fails, will forcefully terminate it.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        try
        {
            await this.CloseAsync().ConfigureAwait(false);
        }
        catch
        {
            // If graceful close fails, force kill
            await this.KillAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }
}
