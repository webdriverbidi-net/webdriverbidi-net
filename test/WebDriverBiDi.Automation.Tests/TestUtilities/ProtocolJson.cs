// <copyright file="ProtocolJson.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using System.Text.Json.Nodes;

/// <summary>
/// Command results shaped as the protocol requires, for answering commands in tests.
/// </summary>
public static class ProtocolJson
{
    /// <summary>
    /// Creates a "browsingContext.locateNodes" result with element nodes "node-1" to "node-{count}".
    /// </summary>
    /// <param name="count">The number of nodes.</param>
    /// <returns>The result.</returns>
    public static JsonObject Nodes(int count)
    {
        return Nodes([.. Enumerable.Range(1, count).Select(i => $"node-{i}")]);
    }

    /// <summary>
    /// Creates a "browsingContext.locateNodes" result with element nodes of the given shared IDs, in order.
    /// </summary>
    /// <param name="sharedIds">The nodes' shared IDs, which may repeat.</param>
    /// <returns>The result.</returns>
    public static JsonObject Nodes(params string[] sharedIds)
    {
        JsonArray nodes = [];
        foreach (string sharedId in sharedIds)
        {
            nodes.Add(new JsonObject()
            {
                ["type"] = "node",
                ["sharedId"] = sharedId,
                ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 },
            });
        }

        return new JsonObject() { ["nodes"] = nodes };
    }

    /// <summary>
    /// Creates a successful "script.callFunction" result.
    /// </summary>
    /// <param name="value">The serialized remote value returned.</param>
    /// <returns>The result.</returns>
    public static JsonObject Success(JsonObject value)
    {
        return new JsonObject() { ["type"] = "success", ["result"] = value, ["realm"] = "realm-1" };
    }

    /// <summary>
    /// Creates a successful "script.callFunction" result returning a boolean.
    /// </summary>
    /// <param name="value">The boolean.</param>
    /// <returns>The result.</returns>
    public static JsonObject Boolean(bool value)
    {
        return Success(new JsonObject() { ["type"] = "boolean", ["value"] = value });
    }

    /// <summary>
    /// Creates a successful "script.callFunction" result returning an array of booleans.
    /// </summary>
    /// <param name="values">The booleans.</param>
    /// <returns>The result.</returns>
    public static JsonObject Booleans(params bool[] values)
    {
        return Success(new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. values.Select(value => (JsonNode)new JsonObject() { ["type"] = "boolean", ["value"] = value })]) });
    }

    /// <summary>
    /// Creates a "script.callFunction" result reporting a thrown exception.
    /// </summary>
    /// <param name="text">The exception's text, such as "Error: failed".</param>
    /// <returns>The result.</returns>
    public static JsonObject Exception(string text)
    {
        return new JsonObject()
        {
            ["type"] = "exception",
            ["exceptionDetails"] = new JsonObject()
            {
                ["columnNumber"] = 0,
                ["exception"] = new JsonObject() { ["type"] = "error" },
                ["lineNumber"] = 0,
                ["stackTrace"] = new JsonObject() { ["callFrames"] = new JsonArray() },
                ["text"] = text,
            },
            ["realm"] = "realm-1",
        };
    }
}
