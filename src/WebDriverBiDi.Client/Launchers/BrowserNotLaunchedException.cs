// <copyright file="BrowserNotLaunchedException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// The exception that is thrown when a browser is not successfully launched.
/// </summary>
public class BrowserNotLaunchedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserNotLaunchedException"/> class.
    /// </summary>
    public BrowserNotLaunchedException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserNotLaunchedException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BrowserNotLaunchedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserNotLaunchedException"/> class with
    /// a specified error message and a reference to the inner exception that is the
    /// cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or <see langword="null"/> if no inner exception is specified.</param>
    public BrowserNotLaunchedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
