// <copyright file="ErrorResponse.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Response class that contains the result of a command when an error is encountered.
/// </summary>
public class ErrorResponse : WebDriverBidiMessage
{
    private string error = string.Empty;
    private string message = string.Empty;

    /// <summary>
    /// Gets the ID for the command causing this error during execution, if any.
    /// </summary>
    [JsonProperty("id")]
    public long? CommandId { get; internal set; }

    /// <summary>
    /// Gets the type of error encountered.
    /// </summary>
    [JsonProperty("error")]
    public string ErrorType { get => this.error; internal set => this.error = value; }

    /// <summary>
    /// Gets the message of the error.
    /// </summary>
    [JsonProperty("message")]
    public string ErrorMessage { get => this.message; internal set => this.message = value; }

    /// <summary>
    /// Gets the stack trace associated with this error.
    /// </summary>
    [JsonProperty("stacktrace", NullValueHandling = NullValueHandling.Ignore)]
    public string? StackTrace { get; internal set; }

    /// <summary>
    /// Gets the data associated with the error without the command information.
    /// </summary>
    /// <returns>An ErrorResponseData object containing the data about the error.</returns>
    public ErrorResponseData GetErrorResponseData()
    {
        return new ErrorResponseData(this);
    }
}