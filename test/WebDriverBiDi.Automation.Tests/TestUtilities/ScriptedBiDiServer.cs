// <copyright file="ScriptedBiDiServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using PinchHitter;

/// <summary>
/// A WebSocket server standing in for a browser's WebDriver BiDi endpoint, for code that connects through a
/// launcher rather than to a <see cref="FakeRemoteEnd"/>. It answers each command with a result shaped as the
/// protocol requires, for a browser with only the default user context and no pages, unless told to fail it, and
/// records the methods it receives.
/// </summary>
public sealed class ScriptedBiDiServer : IAsyncDisposable
{
    private readonly Server server = new();
    private readonly ConcurrentQueue<JsonObject> receivedCommands = new();
    private readonly ConcurrentDictionary<string, (string Error, string Message)> errors = new();

    private ScriptedBiDiServer()
    {
        this.server.OnDataReceived.AddObserver(this.AnswerAsync);
    }

    /// <summary>
    /// Gets the URL of the server's WebSocket endpoint.
    /// </summary>
    public Uri Url => new($"ws://localhost:{this.server.Port}/session");

    /// <summary>
    /// Gets the methods of the commands received, in order.
    /// </summary>
    public IReadOnlyList<string> ReceivedMethods => [.. this.receivedCommands.Select(command => (string)command["method"]!)];

    /// <summary>
    /// Gets the commands received, in order.
    /// </summary>
    public IReadOnlyList<JsonObject> ReceivedCommands => [.. this.receivedCommands];

    /// <summary>
    /// Starts a server.
    /// </summary>
    /// <returns>The started server.</returns>
    public static async Task<ScriptedBiDiServer> StartAsync()
    {
        ScriptedBiDiServer scriptedServer = new();
        await scriptedServer.server.StartAsync();
        return scriptedServer;
    }

    /// <summary>
    /// Answers a command with an error response.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <param name="error">The protocol error code.</param>
    /// <param name="message">The error message.</param>
    public void FailWith(string method, string error, string message)
    {
        this.errors[method] = (error, message);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.server.StopAsync();
    }

    private async Task AnswerAsync(ServerDataReceivedEventArgs e)
    {
        // The HTTP upgrade handshake arrives here too; only WebSocket frames hold commands.
        if (!e.Data.TrimStart().StartsWith('{'))
        {
            return;
        }

        JsonObject command = JsonNode.Parse(e.Data)!.AsObject();
        string method = (string)command["method"]!;
        this.receivedCommands.Enqueue(command);
        JsonObject response = new() { ["id"] = (long)command["id"]! };
        if (this.errors.TryGetValue(method, out (string Error, string Message) error))
        {
            response["type"] = "error";
            response["error"] = error.Error;
            response["message"] = error.Message;
        }
        else
        {
            response["type"] = "success";
            response["result"] = method switch
            {
                "session.new" => CreateNewSessionResult(),
                "session.subscribe" => new JsonObject() { ["subscription"] = "scripted-subscription" },
                "script.addPreloadScript" => new JsonObject() { ["script"] = "scripted-preload-script" },
                "browser.getUserContexts" => new JsonObject() { ["userContexts"] = new JsonArray(new JsonObject() { ["userContext"] = "default" }) },
                "browsingContext.getTree" => new JsonObject() { ["contexts"] = new JsonArray() },
                _ => new JsonObject(),
            };
        }

        await this.server.SendWebSocketDataAsync(e.ConnectionId, response.ToJsonString());
    }

    private static JsonObject CreateNewSessionResult()
    {
        return new JsonObject()
        {
            ["sessionId"] = "scripted-session",
            ["capabilities"] = new JsonObject()
            {
                ["acceptInsecureCerts"] = false,
                ["browserName"] = "scripted",
                ["browserVersion"] = "1.0",
                ["platformName"] = "scripted",
                ["setWindowRect"] = true,
                ["userAgent"] = "Scripted/1.0",
            },
        };
    }
}
