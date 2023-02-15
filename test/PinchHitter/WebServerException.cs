// <copyright file="WebServerException.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// An exception thrown by the test web server.
/// </summary>
public class WebServerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebServerException"/> class.
    /// </summary>
    /// <param name="message">The message to use when throwing the exception.</param>
    public WebServerException(string message)
        : base(message)
    {
    }
}