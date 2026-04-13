// <copyright file="DemoSiteServer.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DemoWebSite;

using PinchHitter;

/// <summary>
/// Creates and launches the demo web site, and populates it with content.
/// </summary>
public class DemoWebSiteServer : IAsyncDisposable
{
    private static readonly string contentDirectoryName = "content";
    private readonly Server webServer = new();
    private bool disposed = false;

    /// <summary>
    /// Gets the port on which the server is listening.
    /// </summary>
    public int Port => this.webServer.Port;

    /// <summary>
    /// Launches the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the server has been disposed.</exception>
    public async Task LaunchAsync()
    {
        this.ThrowIfDisposed();
        StoredContentRegistrar.RegisterDirectory(this.webServer, string.Empty, Path.Combine(AppContext.BaseDirectory, contentDirectoryName));
        this.webServer.RegisterHandler("/", new RedirectRequestHandler("/index.html"));
        this.webServer.RegisterHandler("/processForm", HttpRequestMethod.Post, new FormSubmitRequestHandler());
        this.webServer.RegisterHandler("/cookiePage.html", HttpRequestMethod.Get, new CookiePageRequestHandler());
        await this.webServer.StartAsync();
        Console.WriteLine($"Demo web server listening on port {this.Port}");
    }

    /// <summary>
    /// Shuts down the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ShutdownAsync()
    {
        await this.webServer.StopAsync();
    }

    /// <summary>
    /// Disposes the server asynchronously by shutting it down.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
    }

    /// <summary>
    /// Asynchronously releases the resources used by this server.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        try
        {
            await this.ShutdownAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from ShutdownAsync during disposal
        }

        try
        {
            await this.webServer.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from webServer disposal
        }
    }
}
