// <copyright file="UnknownMessageReceivedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

/// <summary>
/// Object containing event data for events raised when an unknown protocol message is received from a WebDriver Bidi connection.
/// </summary>
public class UnknownMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownMessageReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="message">The message received from the protocol.</param>
    public UnknownMessageReceivedEventArgs(string message)
    {
        this.Message = message;
    }

    /// <summary>
    /// Gets the message received from the protocol.
    /// </summary>
    public string Message { get; }
}