// <copyright file="WebDriverBiDiCommandException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The exception thrown when a WebDriver BiDi protocol command returns an error response.
/// </summary>
public class WebDriverBiDiCommandException : WebDriverBiDiErrorResponseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiCommandException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    public WebDriverBiDiCommandException(string message, ErrorResult errorResult)
        : base(message, errorResult)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiCommandException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public WebDriverBiDiCommandException(string message, ErrorResult errorResult, Exception innerException)
        : base(message, errorResult, innerException)
    {
    }
}
