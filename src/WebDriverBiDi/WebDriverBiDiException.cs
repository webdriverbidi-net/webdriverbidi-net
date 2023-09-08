// <copyright file="WebDriverBiDiException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The base exception class for all WebDriver Bidi errors.
/// </summary>
public class WebDriverBiDiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public WebDriverBiDiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public WebDriverBiDiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}