// <copyright file="EdgeLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Object for launching Microsoft Edge to connect to using a WebDriverBiDi session. Edge is Chromium, so it is
/// launched as Chrome is, and reached through its DevTools endpoint. This launcher does not rely on any external
/// executable except for the browser itself.
/// </summary>
public class EdgeLauncher : ChromeLauncher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeLauncher"/> class.
    /// </summary>
    /// <param name="browserLocatorSettings">The settings to use for locating the Edge executable.</param>
    /// <param name="port">The port on which the browser should listen for connections.</param>
    internal EdgeLauncher(EdgeBrowserLocatorSettings browserLocatorSettings, int port = 0)
        : base(browserLocatorSettings, port)
    {
    }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by the browser launcher.
    /// </summary>
    protected override ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new("edgeLauncher.logMessage");

    /// <inheritdoc/>
    private protected override string ProductName => "Edge";
}
