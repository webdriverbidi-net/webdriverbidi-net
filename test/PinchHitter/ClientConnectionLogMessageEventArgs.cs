// <copyright file="ClientConnectionLogMessageEventArgs.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Object containing event data for events raised when a message is logged by a client connection to the PinchHitter server.
/// </summary>
public class ClientConnectionLogMessageEventArgs : EventArgs
{
    private readonly string message;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnectionLogMessageEventArgs"/> class.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public ClientConnectionLogMessageEventArgs(string message)
    {
        this.message = message;
    }

    /// <summary>
    /// Gets the message to be logged.
    /// </summary>
    public string Message => this.message;
}