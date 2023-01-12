// <copyright file="ProtocolErrorReceivedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing event data for events raised when a protocol error is received from a WebDriver Bidi connection.
/// </summary>
public class ProtocolErrorReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolErrorReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="errorData">The data about the error received from the connection.</param>
    public ProtocolErrorReceivedEventArgs(ErrorResponse? errorData)
    {
        this.ErrorData = errorData;
    }

    /// <summary>
    /// Gets the error response data.
    /// </summary>
    public ErrorResponse? ErrorData { get; }
}