// <copyright file="ConnectionLostEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for the event raised when an established WebDriver BiDi connection ends
/// without the client stopping it: the remote end closed it, or it failed.
/// </summary>
public record ConnectionLostEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionLostEventArgs" /> class.
    /// </summary>
    /// <param name="exception">The exception describing the loss, of the kind commands in flight failed with.</param>
    public ConnectionLostEventArgs(WebDriverBiDiConnectionException exception)
    {
        this.Exception = exception;
    }

    /// <summary>
    /// Gets the exception describing the loss, of the kind commands in flight failed with: its message says
    /// whether the remote end closed the connection or the connection failed, and for a failure, its inner
    /// exception is the connection's error.
    /// </summary>
    public WebDriverBiDiConnectionException Exception { get; }
}
