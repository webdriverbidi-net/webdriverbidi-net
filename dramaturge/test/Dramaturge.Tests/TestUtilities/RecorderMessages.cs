// <copyright file="RecorderMessages.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TestUtilities;

using System.Text.Json.Nodes;
using WebDriverBiDi;

/// <summary>
/// Sends messages to a code recording as the code recorder in its pages does.
/// </summary>
public static class RecorderMessages
{
    /// <summary>
    /// Gets the recording's channel, from its first install.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>The channel's ID.</returns>
    public static async Task<string> GetChannelAsync(FakeSession session)
    {
        JsonObject install = await session.RemoteEnd.WaitForCommandAsync("script.callFunction");
        return (string)install["params"]!["arguments"]![0]!["value"]!["channel"]!;
    }

    /// <summary>
    /// Sends a message, and waits until the driver has delivered it.
    /// </summary>
    /// <param name="driver">The driver.</param>
    /// <param name="session">The session.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="contextId">The browsing context that sends it, or <see langword="null"/> for none.</param>
    /// <param name="message">The message.</param>
    /// <param name="elementIds">The shared IDs of the element and its ancestors; by default one element, named for its CSS path.</param>
    /// <returns>A task that completes when the message is delivered.</returns>
    public static async Task SendAsync(BiDiDriver driver, FakeSession session, string channel, string? contextId, JsonObject message, params string[] elementIds)
    {
        JsonArray data = [new JsonObject() { ["type"] = "string", ["value"] = message.ToJsonString() }];
        string element = message["target"] is JsonObject facts ? $"node-{facts["cssPath"]}" : "node-none";
        foreach (string elementId in elementIds.Length == 0 ? [element] : elementIds)
        {
            data.Add(Node(elementId));
        }

        JsonObject source = new() { ["realm"] = "realm-1" };
        if (contextId is not null)
        {
            source["context"] = contextId;
        }

        await session.RemoteEnd.RaiseEventAsync("script.message", new JsonObject() { ["channel"] = channel, ["data"] = new JsonObject() { ["type"] = "array", ["value"] = data }, ["source"] = source });
        await NetworkEvents.FlushAsync(driver);
    }

    /// <summary>
    /// Creates an element node.
    /// </summary>
    /// <param name="sharedId">The node's shared ID.</param>
    /// <returns>The node, serialized.</returns>
    public static JsonObject Node(string sharedId)
    {
        return new JsonObject() { ["type"] = "node", ["sharedId"] = sharedId, ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } };
    }
}
