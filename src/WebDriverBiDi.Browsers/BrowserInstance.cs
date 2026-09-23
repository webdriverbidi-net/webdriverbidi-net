// <copyright file="BrowserInstance.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Represents a running browser instance that has been launched and is ready to accept connections.
/// Implements <see cref="IAsyncDisposable"/> to ensure proper cleanup of browser processes.
/// </summary>
/// <remarks>
/// An instance is a handle to one launch. Once its launcher has launched another browser, closing,
/// killing, or disposing the instance has no effect, and <see cref="IsRunning"/> is <see langword="false"/>.
/// </remarks>
public class BrowserInstance : IAsyncDisposable
{
    private readonly BrowserLauncher launcher;
    private readonly int launchId;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserInstance"/> class.
    /// </summary>
    /// <param name="launcher">The launcher that created this instance.</param>
    /// <param name="connectionString">The connection string for connecting to the browser.</param>
    /// <param name="processId">The process ID of the browser, or 0 if not applicable.</param>
    /// <param name="launchId">The identity of the launch this instance represents.</param>
    internal BrowserInstance(BrowserLauncher launcher, string connectionString, int processId, int launchId)
    {
        this.launchId = launchId;
        this.launcher = launcher;
        this.ConnectionString = connectionString;
        this.ProcessId = processId;
    }

    /// <summary>
    /// Gets the connection string for connecting to the browser via WebDriver BiDi.
    /// Use this property with <see cref="BiDiDriver.StartAsync"/> to establish a connection.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Gets the process ID of the browser process, or 0 if the browser is remote or the process ID is not available.
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Gets a value indicating whether the browser process is still running.
    /// For remote browsers, this always returns true until the instance is disposed.
    /// </summary>
    public bool IsRunning => this.IsActive && this.launcher.IsRunning;

    private bool IsActive => !this.disposed && this.launcher.CurrentLaunchId == this.launchId;

    /// <summary>
    /// Asynchronously closes the browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <param name="cancellationToken">
    /// A token that cancels waiting for the browser to exit; the browser is then killed, after which an
    /// <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <exception cref="CannotQuitBrowserException">Thrown when the browser cannot be closed; use <see cref="KillAsync"/> to force termination.</exception>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsActive)
        {
            return;
        }

        await this.launcher.QuitBrowserAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Forcefully terminates the browser. This should only be used when <see cref="CloseAsync"/> fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// A browser on a remote grid cannot be terminated from this machine, so for such a browser this method has no effect.
    /// </remarks>
    /// <param name="cancellationToken">
    /// A token that cancels waiting for the killed browser to exit, after which an
    /// <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    public async Task KillAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsActive)
        {
            return;
        }

        await this.launcher.KillBrowserAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously disposes the browser instance. This will attempt to close the browser,
    /// and if that fails, will forcefully terminate it.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (!this.IsActive)
        {
            this.disposed = true;
            return;
        }

        this.disposed = true;

        // The launcher is called directly, because CloseAsync and KillAsync return early once disposed is set.
        try
        {
            await this.launcher.QuitBrowserAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            try
            {
                await this.launcher.KillBrowserAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Disposal must not throw; this matches BrowserLauncher.DisposeAsyncCore.
            }
        }

        GC.SuppressFinalize(this);
    }
}
