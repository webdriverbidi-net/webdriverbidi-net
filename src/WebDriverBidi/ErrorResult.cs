// <copyright file="ErrorResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using WebDriverBidi.Protocol;

/// <summary>
/// Object containing an error response to a command.
/// </summary>
public class ErrorResult : CommandResult
{
    private readonly string error = string.Empty;
    private readonly string message = string.Empty;
    private readonly string? stackTrace;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult"/> class.
    /// </summary>
    /// <param name="response">The error response containing the error data.</param>
    public ErrorResult(ErrorResponseMessage response)
    {
        this.error = response.ErrorType;
        this.message = response.ErrorMessage;
        this.stackTrace = response.StackTrace;
        this.AdditionalData = response.AdditionalData;
    }

    /// <summary>
    /// Gets a value indicating whether the response data is an error.
    /// </summary>
    public override bool IsError => true;

    /// <summary>
    /// Gets the type of error encountered.
    /// </summary>
    public string ErrorType => this.error;

    /// <summary>
    /// Gets the message of the error.
    /// </summary>
    public string ErrorMessage => this.message;

    /// <summary>
    /// Gets the stack trace associated with this error.
    /// </summary>
    public string? StackTrace => this.stackTrace;
}