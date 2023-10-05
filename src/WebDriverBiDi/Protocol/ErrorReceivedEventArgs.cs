// <copyright file="ErrorReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when a protocol error is received from a WebDriver Bidi connection.
/// </summary>
public class ErrorReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="errorData">The data about the error received from the connection.</param>
    public ErrorReceivedEventArgs(ErrorResult? errorData)
    {
        this.ErrorData = errorData;
    }

    /// <summary>
    /// Gets the error response data.
    /// </summary>
    public ErrorResult? ErrorData { get; }
}
