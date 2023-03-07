// <copyright file="ClientConnectionDataReceivedEventArgs.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Object containing event data for events raised when data is received by a client connection to the PinchHitter server.
/// </summary>
public class ClientConnectionDataReceivedEventArgs : EventArgs
{
    private readonly string connectionId;
    private readonly string dataRecevied;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnectionDataReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="connectionId">The ID of the client connection from which the data is received.</param>
    /// <param name="dataRecevied">The data received from the client connection.</param>
    public ClientConnectionDataReceivedEventArgs(string connectionId, string dataRecevied)
    {
        this.connectionId = connectionId;
        this.dataRecevied = dataRecevied;
    }

    /// <summary>
    /// Gets the ID of the client connection from which the data is received.
    /// </summary>
    public string ConnectionId => this.connectionId;

    /// <summary>
    /// Gets the data received from the client connection.
    /// </summary>
    public string DataReceived => this.dataRecevied;
}