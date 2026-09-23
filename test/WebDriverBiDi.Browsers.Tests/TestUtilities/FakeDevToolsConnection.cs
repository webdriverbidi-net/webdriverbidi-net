// <copyright file="FakeDevToolsConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;
using WebDriverBiDi.Protocol;

/// <summary>
/// An in-memory connection standing in for a Chromium browser's DevTools endpoint. It answers the
/// commands that bootstrap the WebDriver BiDi mapper, and answers each WebDriver BiDi command
/// relayed through the mapper with an empty success result, as the mapper would.
/// </summary>
public sealed class FakeDevToolsConnection : Connection
{
    /// <summary>
    /// The target ID of the mapper tab.
    /// </summary>
    public const string MapperTargetId = "mapper-target";

    /// <summary>
    /// The DevTools session ID of the mapper tab.
    /// </summary>
    public const string MapperSessionId = "mapper-session";

    private readonly ConcurrentQueue<JsonObject> sentCommands = new();
    private readonly ConcurrentDictionary<string, int> remainingErrors = new();
    private readonly ConcurrentDictionary<string, JsonObject> results = new();
    private readonly HashSet<string> unansweredMethods = [];
    private TaskCompletionSource<bool> closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int isOpenFlag;

    /// <inheritdoc/>
    public override ConnectionKind ConnectionKind => ConnectionKind.WebSocket;

    /// <summary>
    /// Gets the DevTools commands sent, in order.
    /// </summary>
    public IReadOnlyList<JsonObject> SentCommands => [.. this.sentCommands];

    /// <inheritdoc/>
    protected override bool IsConnectionOpen => Interlocked.CompareExchange(ref this.isOpenFlag, 0, 0) == 1;

    /// <summary>
    /// Gets or sets a value indicating whether stopping the connection fails.
    /// </summary>
    public bool FailsToStop { get; set; }

    /// <summary>
    /// Answers the next requests for a method with errors.
    /// </summary>
    /// <param name="method">The DevTools method.</param>
    /// <param name="count">The number of requests answered with an error.</param>
    public void FailNext(string method, int count)
    {
        this.remainingErrors[method] = count;
    }

    /// <summary>
    /// Leaves requests for a method unanswered.
    /// </summary>
    /// <param name="method">The DevTools method.</param>
    public void NeverAnswer(string method)
    {
        this.unansweredMethods.Add(method);
    }

    /// <summary>
    /// Answers requests for a method with a result in place of the usual one.
    /// </summary>
    /// <param name="method">The DevTools method.</param>
    /// <param name="result">The result.</param>
    public void AnswerWith(string method, JsonObject result)
    {
        this.results[method] = result;
    }

    /// <summary>
    /// Delivers a message, as if the browser had sent it.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task DeliverAsync(JsonObject message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message.ToJsonString());
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        return this.NotifyDataReceivedObserverAsync(owner, bytes.Length);
    }

    /// <inheritdoc/>
    protected override Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        this.closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref this.isOpenFlag, 1);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        if (this.FailsToStop)
        {
            throw new InvalidOperationException("The connection could not be stopped.");
        }

        Interlocked.Exchange(ref this.isOpenFlag, 0);
        this.closed.TrySetResult(true);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        JsonObject command = JsonNode.Parse(Encoding.UTF8.GetString(messageBuffer.Span))!.AsObject();
        this.sentCommands.Enqueue(command);
        string method = (string)command["method"]!;
        if (this.unansweredMethods.Contains(method))
        {
            return Task.CompletedTask;
        }

        JsonObject response = new() { ["id"] = (long)command["id"]! };
        if (this.remainingErrors.TryGetValue(method, out int errors) && errors > 0)
        {
            this.remainingErrors[method] = errors - 1;
            response["error"] = new JsonObject() { ["code"] = -32000, ["message"] = $"{method} failed" };
        }
        else
        {
            response["result"] = this.results.TryGetValue(method, out JsonObject? result) ? result.DeepClone() : this.CreateResult(method);
        }

        // Answered asynchronously, as a browser would, rather than within the send.
        string? relayedBiDiCommand = GetRelayedBiDiCommand(command);
        _ = Task.Run(async () =>
        {
            await this.DeliverAsync(response).ConfigureAwait(false);
            if (relayedBiDiCommand is not null)
            {
                long bidiId = (long)JsonNode.Parse(relayedBiDiCommand)!["id"]!;
                JsonObject bidiResponse = new() { ["type"] = "success", ["id"] = bidiId, ["result"] = new JsonObject() };
                JsonObject binding = new()
                {
                    ["method"] = "Runtime.bindingCalled",
                    ["params"] = new JsonObject() { ["name"] = "sendBidiResponse", ["payload"] = bidiResponse.ToJsonString() },
                    ["sessionId"] = MapperSessionId,
                };
                await this.DeliverAsync(binding).ConfigureAwait(false);
            }
        });

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async Task ReceiveDataAsync()
    {
        Task cancellation = Task.Delay(Timeout.InfiniteTimeSpan, this.ConnectionCancellationToken);
        await Task.WhenAny(this.closed.Task, cancellation).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsyncCore()
    {
        return default;
    }

    // A WebDriver BiDi command reaches the mapper as the JSON string argument of window.onBidiMessage.
    private static string? GetRelayedBiDiCommand(JsonObject command)
    {
        const string Prefix = "window.onBidiMessage(";
        string? expression = (string?)command["params"]?["expression"];
        return expression is not null && expression.StartsWith(Prefix, StringComparison.Ordinal)
            ? (string)JsonNode.Parse(expression[Prefix.Length..^1])!
            : null;
    }

    private JsonObject CreateResult(string method)
    {
        return method switch
        {
            "Target.createTarget" => new JsonObject() { ["targetId"] = MapperTargetId },
            "Target.attachToTarget" => new JsonObject() { ["sessionId"] = MapperSessionId },
            _ => [],
        };
    }
}
