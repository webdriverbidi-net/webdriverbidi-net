// <copyright file="WebDriverBiDiProtocolException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The exception thrown when the remote end of the WebDriver BiDi protocol returns an error response.
/// This exception carries structured error data from the remote end, allowing callers to
/// programmatically inspect the error type, message, and optional stack trace.
/// </summary>
public class WebDriverBiDiProtocolException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiProtocolException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    public WebDriverBiDiProtocolException(string message, ErrorResult errorResult)
        : base(message)
    {
        this.ErrorResult = errorResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiProtocolException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public WebDriverBiDiProtocolException(string message, ErrorResult errorResult, Exception innerException)
        : base(message, innerException)
    {
        this.ErrorResult = errorResult;
    }

    /// <summary>
    /// Gets the error result data from the remote end, containing the error type, error message,
    /// and optional stack trace.
    /// </summary>
    public ErrorResult ErrorResult { get; }

    /// <summary>
    /// Gets the protocol error type string returned by the remote end (e.g., "invalid argument",
    /// "no such frame", "unknown command").
    /// </summary>
    public string ErrorType => this.ErrorResult.ErrorType;

    /// <summary>
    /// Gets the protocol error message returned by the remote end.
    /// </summary>
    public string ProtocolErrorMessage => this.ErrorResult.ErrorMessage;

    /// <summary>
    /// Gets the stack trace from the remote end, if available.
    /// </summary>
    public string? RemoteStackTrace => this.ErrorResult.StackTrace;
}
