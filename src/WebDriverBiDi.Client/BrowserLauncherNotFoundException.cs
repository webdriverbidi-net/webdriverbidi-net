// <copyright file="BrowserLauncherNotFoundException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

/// <summary>
/// The exception that is thrown when an element is not visible.
/// </summary>
public class BrowserLauncherNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherNotFoundException"/> class.
    /// </summary>
    public BrowserLauncherNotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherNotFoundException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BrowserLauncherNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherNotFoundException"/> class with
    /// a specified error message and a reference to the inner exception that is the
    /// cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or <see langword="null"/> if no inner exception is specified.</param>
    public BrowserLauncherNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
