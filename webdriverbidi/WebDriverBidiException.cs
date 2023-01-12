// <copyright file="WebDriverBidiException.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// The base exception class for all WebDriver Bidi errors.
/// </summary>
public class WebDriverBidiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public WebDriverBidiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiException" /> class with a given message and inner exception.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception wrapped by this exception.</param>
    public WebDriverBidiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}