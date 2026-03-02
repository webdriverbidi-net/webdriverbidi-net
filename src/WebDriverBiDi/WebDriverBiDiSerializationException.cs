// <copyright file="WebDriverBiDiSerializationException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The exception thrown when a JSON serialization or deserialization error occurs
/// while processing a WebDriver BiDi protocol message.
/// </summary>
public class WebDriverBiDiSerializationException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiSerializationException" /> class.
    /// </summary>
    public WebDriverBiDiSerializationException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiSerializationException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public WebDriverBiDiSerializationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiSerializationException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public WebDriverBiDiSerializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
