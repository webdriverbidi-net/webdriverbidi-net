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
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult"/> class.
    /// </summary>
    /// <param name="response">The error response containing the error data.</param>
    internal ErrorResult(ErrorResponseMessage response)
    {
        this.ErrorType = response.ErrorType;
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
    public string ErrorType { get; } = string.Empty;

    /// <summary>
    /// Gets the message of the error.
    /// </summary>
    public string ErrorMessage { get; } = string.Empty;

    /// <summary>
    /// Gets the stack trace associated with this error.
    /// </summary>
    public string? StackTrace { get; }
}
