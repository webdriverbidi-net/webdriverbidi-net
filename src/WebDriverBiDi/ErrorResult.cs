// <copyright file="ErrorResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Protocol;

/// <summary>
/// Object containing an error response to a command.
/// </summary>
public record ErrorResult : CommandResult
{
    private static readonly Lazy<StringEnumValueConverter<ErrorCode>> ErrorCodeConverter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult"/> class with default values.
    /// </summary>
    internal ErrorResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult"/> class.
    /// </summary>
    /// <param name="response">The error response containing the error data.</param>
    internal ErrorResult(ErrorResponseMessage response)
    {
        this.ErrorType = response.ErrorType;
        this.ErrorCode = response.ErrorCode;
        this.ErrorMessage = response.ErrorMessage;
        this.StackTrace = response.StackTrace;
        this.AdditionalData = response.AdditionalData;
    }

    /// <summary>
    /// Gets a value indicating whether the response data is an error.
    /// </summary>
    public override bool IsError => true;

    /// <summary>
    /// Gets the type of error encountered.
    /// </summary>
    public string ErrorType { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the error code of error encountered.
    /// </summary>
    public ErrorCode ErrorCode { get; private init; } = ErrorCode.UnsetErrorCode;

    /// <summary>
    /// Gets the message of the error.
    /// </summary>
    public string ErrorMessage { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the stack trace associated with this error.
    /// </summary>
    public string? StackTrace { get; private init; }

    /// <summary>
    /// Creates an <see cref="ErrorResult"/> object from the given error information.
    /// </summary>
    /// <param name="errorType">The type of the error.</param>
    /// <param name="errorMessage">The message of the error.</param>
    /// <param name="stackTrace">The stack trace associated with the error.</param>
    /// <returns>An <see cref="ErrorResult"/> object containing the error information.</returns>
    /// <remarks>
    /// This method can be used to create an <see cref="ErrorResult"/> for use with
    /// <see cref="WebDriverBiDiCommandException"/> when an error is encountered
    /// for custom commands. This allows users to create extension methods that
    /// mimic the behavior of built-in commands.
    /// </remarks>
    public static ErrorResult FromErrorInformation(string errorType, string errorMessage, string? stackTrace = null)
    {
        return new ErrorResult
        {
            ErrorType = errorType,
            ErrorCode = ErrorCodeConverter.Value.GetValue(errorType),
            ErrorMessage = errorMessage,
            StackTrace = stackTrace,
        };
    }
}
