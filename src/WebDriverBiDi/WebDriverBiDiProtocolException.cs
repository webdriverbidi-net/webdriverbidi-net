// <copyright file="WebDriverBiDiProtocolException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The exception thrown when the remote end of the WebDriver BiDi protocol sends an error
/// response that is not associated with a specific command (i.e., an unsolicited error).
/// </summary>
public class WebDriverBiDiProtocolException : WebDriverBiDiErrorResponseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiProtocolException"/> class.
    /// </summary>
    public WebDriverBiDiProtocolException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiProtocolException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    public WebDriverBiDiProtocolException(string message, ErrorResult errorResult)
        : base(message, errorResult)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiProtocolException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public WebDriverBiDiProtocolException(string message, ErrorResult errorResult, Exception innerException)
        : base(message, errorResult, innerException)
    {
    }
}
