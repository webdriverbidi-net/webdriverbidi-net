// <copyright file="ExistingBrowserLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Protocol;

/// <summary>
/// A launcher for a browser that is already listening on a WebSocket URL, which it neither starts
/// nor stops: launching connects to the browser, and closing only detaches from it.
/// </summary>
internal sealed class ExistingBrowserLauncher : BrowserLauncher
{
    private readonly Uri webSocketUrl;
    private bool isAttached;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExistingBrowserLauncher"/> class.
    /// </summary>
    /// <param name="settings">The locator settings naming the browser.</param>
    /// <param name="webSocketUrl">The WebSocket URL on which the browser is listening.</param>
    public ExistingBrowserLauncher(RemoteBrowserLocatorSettings settings, Uri webSocketUrl)
        : base(settings, 0)
    {
        this.webSocketUrl = webSocketUrl;
    }

    /// <summary>
    /// Gets a value indicating whether a browser instance is attached.
    /// </summary>
    public override bool IsRunning => this.isAttached;

    /// <summary>
    /// Gets a value indicating whether the browser can be closed using WebDriver BiDi's browser.close
    /// command, which it cannot, because the browser belongs to whoever started it.
    /// </summary>
    public override bool IsBrowserCloseAllowed => false;

    /// <summary>
    /// Gets a value indicating whether the URL is a Chromium DevTools endpoint (e.g.,
    /// "ws://127.0.0.1:9222/devtools/browser/…"), which is reached through the WebDriver BiDi
    /// mapper, rather than a WebDriver BiDi endpoint.
    /// </summary>
    internal bool IsDevToolsEndpoint => this.webSocketUrl.AbsolutePath.StartsWith("/devtools/", StringComparison.Ordinal);

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("existingBrowserLauncher.logMessage");

    /// <summary>
    /// Does nothing, as there is nothing to start.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting the launcher.</param>
    /// <returns>A completed task.</returns>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing, as there is nothing to stop.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels stopping the launcher.</param>
    /// <returns>A completed task.</returns>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attaches to the browser, returning a <see cref="BrowserInstance"/> whose connection string is its WebSocket URL.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the launch.</param>
    /// <returns>The browser instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a browser instance is already attached.</exception>
    public override async Task<BrowserInstance> LaunchBrowserAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (this.isAttached)
        {
            throw new InvalidOperationException("A browser instance from this launcher is still attached; close it before attaching another.");
        }

        this.ConnectionString = this.webSocketUrl.AbsoluteUri;
        this.isAttached = true;
        await this.LogAsync($"Attaching to the browser at {this.ConnectionString}").ConfigureAwait(false);
        return this.CreateBrowserInstance(this.ConnectionString, 0);
    }

    /// <summary>
    /// Detaches from the browser, which keeps running.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels detaching.</param>
    /// <returns>A completed task.</returns>
    public override Task QuitBrowserAsync(CancellationToken cancellationToken = default)
    {
        this.isAttached = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a <see cref="Transport"/> for the browser: through the WebDriver BiDi mapper for a
    /// Chromium DevTools endpoint, or directly otherwise.
    /// </summary>
    /// <returns>The transport.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the launcher has been disposed.</exception>
    public override Transport CreateTransport()
    {
        this.ThrowIfDisposed();
        return this.IsDevToolsEndpoint
            ? new ChromiumTransport(this.CreateConnection()) { InitializationTimeout = this.InitializationTimeout }
            : new Transport(this.CreateConnection());
    }
}
