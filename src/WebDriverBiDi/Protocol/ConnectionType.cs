// <copyright file="ConnectionType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Enumerated value indicating the type of connection used when communicating with a WebDriver BiDi remote end.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// The connection communicates with the browser over a WebSocket.
    /// </summary>
    /// <remarks>
    /// WebSocket connections are the standard and recommended transport for all scenarios.
    /// They support all browsers with WebDriver BiDi and work for both local and remote connections.
    /// </remarks>
    WebSocket,

    /// <summary>
    /// The connection communicates with the browser over anonymous pipes.
    /// </summary>
    /// <remarks>
    /// Pipe connections provide slightly lower latency for local automation scenarios but are only
    /// supported by Chromium-based browsers and require the browser and tests to be on the same machine.
    /// Use only for specialized high-performance scenarios.
    /// </remarks>
    Pipes,
}
