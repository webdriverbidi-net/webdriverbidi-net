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
    WebSocket,

    /// <summary>
    /// The connection communicates with the browser over named pipes.
    /// </summary>
    Pipes,
}
