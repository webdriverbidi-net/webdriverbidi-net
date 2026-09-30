// <copyright file="FakeResponse.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using System.Text.Json.Nodes;

/// <summary>
/// A fake remote end's answer to a command: the result, and the events delivered before it.
/// </summary>
/// <param name="Result">The command's result.</param>
/// <param name="EventsBefore">The events, each a method and its parameters, delivered before the result.</param>
public sealed record FakeResponse(JsonNode Result, IReadOnlyList<(string Method, JsonObject Parameters)> EventsBefore)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeResponse"/> class with no events.
    /// </summary>
    /// <param name="result">The command's result.</param>
    public FakeResponse(JsonNode result)
        : this(result, [])
    {
    }

    /// <summary>
    /// Gets the protocol error code the command fails with instead of returning its result, or <see langword="null"/>
    /// when it succeeds.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the error message sent with <see cref="Error"/>.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Creates an answer that fails the command, delivering events before the error.
    /// </summary>
    /// <param name="error">The protocol error code, such as "unknown error".</param>
    /// <param name="message">The error message.</param>
    /// <param name="eventsBefore">The events, each a method and its parameters, delivered before the error.</param>
    /// <returns>The answer.</returns>
    public static FakeResponse Failure(string error, string message, params (string Method, JsonObject Parameters)[] eventsBefore)
    {
        return new FakeResponse(new JsonObject(), eventsBefore) { Error = error, ErrorMessage = message };
    }
}
