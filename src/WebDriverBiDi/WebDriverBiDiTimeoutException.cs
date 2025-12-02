// <copyright file="WebDriverBiDiTimeoutException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The exception thrown when a WebDriver Bidi operation exceeds its timeout.
/// </summary>
public class WebDriverBiDiTimeoutException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiTimeoutException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public WebDriverBiDiTimeoutException(string message)
        : base(message)
    {
    }
}
