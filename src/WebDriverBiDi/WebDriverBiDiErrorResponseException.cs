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
        this.ErrorDetails = new ErrorResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiErrorResponseException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="errorResult">The error result returned by the remote end.</param>
    protected WebDriverBiDiErrorResponseException(string message, ErrorResult errorResult)
        : base(message)
    {
        this.ErrorDetails = errorResult;
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
        this.ErrorDetails = errorResult;
    }

    /// <summary>
    /// Gets the error result data from the remote end, containing the error type, error message,
    /// and optional stack trace.
    /// </summary>
    public ErrorResult ErrorDetails { get; }

    /// <summary>
    /// Gets the protocol error code returned by the remote end. If the error type string does not
    /// match any known error code, this will return <see cref="ErrorCode.UnsetErrorCode"/>.
    /// </summary>
    public ErrorCode ErrorCode => this.ErrorDetails.ErrorCode;

    /// <summary>
    /// Gets the error type for the protocol error returned by the remote end.
    /// </summary>
    public string ProtocolErrorType => this.ErrorDetails.ErrorType;

    /// <summary>
    /// Gets the protocol error message returned by the remote end.
    /// </summary>
    public string ProtocolErrorMessage => this.ErrorDetails.ErrorMessage;

    /// <summary>
    /// Gets the stack trace from the remote end, if available.
    /// </summary>
    public string? RemoteStackTrace => this.ErrorDetails.StackTrace;
}
