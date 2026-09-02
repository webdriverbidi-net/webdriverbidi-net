// <copyright file="TransportState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Enumerated value indicating the lifecycle state of a <see cref="Transport"/> with respect to its
/// connection to a WebDriver BiDi remote end.
/// </summary>
public enum TransportState
{
    /// <summary>
    /// The transport is not connected to a remote end. This is the initial state, the state after a
    /// completed disconnect, and the state a failed connection attempt rolls back to.
    /// </summary>
    Disconnected,

    /// <summary>
    /// A connection attempt is in flight but has not yet completed. The transport is not yet able to
    /// send commands, and a caller must treat it as not-yet-connected.
    /// </summary>
    Connecting,

    /// <summary>
    /// The transport is connected to a remote end and able to exchange messages.
    /// </summary>
    Connected,
}
