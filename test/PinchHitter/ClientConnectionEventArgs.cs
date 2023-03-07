// <copyright file="ClientConnectionEventArgs.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Object containing event data for events raised when a client connects to the PinchHitter server.
/// </summary>
public class ClientConnectionEventArgs : EventArgs
{
    private readonly string connectionId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnectionEventArgs"/> class.
    /// </summary>
    /// <param name="connectionId">The ID of the client connection.</param>
    public ClientConnectionEventArgs(string connectionId)
    {
        this.connectionId = connectionId;
    }

    /// <summary>
    /// Gets the ID of the client connection.
    /// </summary>
    public string ConnectionId => this.connectionId;
}