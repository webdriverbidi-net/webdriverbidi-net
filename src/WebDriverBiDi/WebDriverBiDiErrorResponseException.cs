// <copyright file="WebDriverBiDiErrorResponseException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Abstract base class for exceptions that carry a structured <see cref="ErrorResult"/>
/// received from the remote end of the WebDriver BiDi protocol. Subclasses distinguish
/// between command-level errors (see <see cref="WebDriverBiDiCommandException"/>) and
/// unsolicited protocol-level errors (see <see cref="WebDriverBiDiProtocolException"/>).
/// </summary>
public abstract class WebDriverBiDiErrorResponseException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiErrorResponseException"/> class.
    /// </summary>
    protected WebDriverBiDiErrorResponseException()
        : base()
    {
        this.ErrorResult = new ErrorResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiErrorResponseException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    protected WebDriverBiDiErrorResponseException(string message, ErrorResult errorResult)
        : base(message)
    {
        this.ErrorResult = errorResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiErrorResponseException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    protected WebDriverBiDiErrorResponseException(string message, ErrorResult errorResult, Exception innerException)
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
