// <copyright file="ErrorResponse.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Object containing an error response to a command.
/// </summary>
[JsonObject]
public class ErrorResponse : CommandResult
{
    private string error = string.Empty;
    private string message = string.Empty;

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
    /// Gets a value indicating whether this response is an error response.
    /// </summary>
    public override bool IsError => true;
}