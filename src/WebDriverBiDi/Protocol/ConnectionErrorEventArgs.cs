// <copyright file="ConnectionErrorEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when an error occurs on a WebDriver Bidi connection.
/// </summary>
public record ConnectionErrorEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionErrorEventArgs" /> class.
    /// </summary>
    /// <param name="exception">The exception that caused the connection error.</param>
    public ConnectionErrorEventArgs(Exception exception)
    {
        this.Exception = exception;
    }

    /// <summary>
    /// Gets the exception that caused the connection error.
    /// </summary>
    public Exception Exception { get; }
}
