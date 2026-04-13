// <copyright file="LaunchStrategy.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Specifies how the browser should be launched.
/// </summary>
/// <remarks>
/// This enum is internal as the launch strategy is typically inferred from the builder methods used
/// (e.g., LaunchViaDriver, OnRemoteGrid) rather than specified explicitly.
/// </remarks>
internal enum LaunchStrategy
{
    /// <summary>
    /// Launch the browser process directly with remote debugging enabled.
    /// For Chrome, this uses --remote-debugging-port or --remote-debugging-pipe.
    /// For Firefox, this uses --remote-debugging-port.
    /// </summary>
    Direct,

    /// <summary>
    /// Launch the browser via a WebDriver Classic driver executable (chromedriver, geckodriver).
    /// Creates a WebDriver Classic session and upgrades to use WebDriver BiDi.
    /// </summary>
    UsingDriver,

    /// <summary>
    /// Connect to a remote WebDriver grid or cloud service.
    /// Creates a session on the remote endpoint and connects to its BiDi WebSocket URL.
    /// </summary>
    UsingRemoteGrid,
}
