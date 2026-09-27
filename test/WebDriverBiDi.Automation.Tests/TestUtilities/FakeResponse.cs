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
}
