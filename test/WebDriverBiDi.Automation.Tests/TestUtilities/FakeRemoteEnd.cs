// <copyright file="FakeRemoteEnd.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;
using WebDriverBiDi.Protocol;

/// <summary>
/// An in-memory connection standing in for a browser's WebDriver BiDi remote end. It answers each command
/// with a success result, by default one shaped as the protocol requires for commands whose results carry
/// identifiers, records what was sent, and delivers events on demand or, as a browser does, before a
/// command's response.
/// </summary>
public sealed class FakeRemoteEnd : Connection
{
    private readonly ConcurrentQueue<JsonObject> sentCommands = new();
    private readonly ConcurrentQueue<(string Method, JsonNode Result)> sentResults = new();
    private readonly ConcurrentDictionary<string, Func<JsonObject, FakeResponse>> results = new();
    private readonly ConcurrentDictionary<string, (string Error, string Message)> errors = new();
    private readonly ConcurrentDictionary<string, bool> unansweredMethods = new();
    private TaskCompletionSource<bool> closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int isOpenFlag;
    private int identifierCount;

    /// <inheritdoc/>
    public override ConnectionKind ConnectionKind => ConnectionKind.WebSocket;

    /// <summary>
    /// Gets the commands sent, in order.
    /// </summary>
    public IReadOnlyList<JsonObject> SentCommands => [.. this.sentCommands];

    /// <inheritdoc/>
    protected override bool IsConnectionOpen => Interlocked.CompareExchange(ref this.isOpenFlag, 0, 0) == 1;

    /// <summary>
    /// Creates a driver connected to a new fake remote end.
    /// </summary>
    /// <returns>The started driver and its remote end.</returns>
    public static async Task<(BiDiDriver Driver, FakeRemoteEnd RemoteEnd)> ConnectAsync()
    {
        FakeRemoteEnd remoteEnd = new();
        BiDiDriver driver = new(TimeSpan.FromSeconds(10), new Transport(remoteEnd));
        await driver.StartAsync("ws://fake.remote.end/session");
        return (driver, remoteEnd);
    }

    /// <summary>
    /// Answers a command with a result computed from its parameters.
    /// </summary>
    /// <param name="method">The command's method, such as "script.callFunction".</param>
    /// <param name="createResult">Creates the result from the command's parameters.</param>
    public void AnswerWith(string method, Func<JsonObject, JsonNode> createResult)
    {
        this.AnswerWith(method, parameters => new FakeResponse(createResult(parameters)));
    }

    /// <summary>
    /// Answers a command with a result, and events delivered before it, computed from its parameters.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <param name="createResponse">Creates the response from the command's parameters.</param>
    public void AnswerWith(string method, Func<JsonObject, FakeResponse> createResponse)
    {
        this.results[method] = createResponse;
        this.errors.TryRemove(method, out _);
        this.unansweredMethods.TryRemove(method, out _);
    }

    /// <summary>
    /// Answers a command with a fixed result.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <param name="result">The result.</param>
    public void AnswerWith(string method, JsonObject result)
    {
        this.AnswerWith(method, _ => result.DeepClone());
    }

    /// <summary>
    /// Answers a command with an error response.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <param name="error">The protocol error code, such as "no such frame".</param>
    /// <param name="message">The error message.</param>
    public void FailWith(string method, string error, string message)
    {
        this.errors[method] = (error, message);
    }

    /// <summary>
    /// Leaves a command unanswered, as a remote end that never responds would.
    /// </summary>
    /// <param name="method">The command's method.</param>
    public void NeverAnswer(string method)
    {
        this.unansweredMethods[method] = true;
    }

    /// <summary>
    /// Gets the commands sent with a method, in order.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <returns>The commands.</returns>
    public IReadOnlyList<JsonObject> CommandsFor(string method)
    {
        return [.. this.sentCommands.Where(command => (string?)command["method"] == method)];
    }

    /// <summary>
    /// Gets the successful results returned for a command's method, in order.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <returns>The results.</returns>
    public IReadOnlyList<JsonNode> ResultsFor(string method)
    {
        return [.. this.sentResults.Where(entry => entry.Method == method).Select(entry => entry.Result)];
    }

    /// <summary>
    /// Waits for a command to have been sent, for work a handler does after its first await.
    /// </summary>
    /// <param name="method">The command's method.</param>
    /// <param name="occurrence">Which sending of the command to wait for, counting from 1.</param>
    /// <returns>The command.</returns>
    /// <exception cref="TimeoutException">Thrown when the command is not sent within ten seconds.</exception>
    public async Task<JsonObject> WaitForCommandAsync(string method, int occurrence = 1)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            IReadOnlyList<JsonObject> commands = this.CommandsFor(method);
            if (commands.Count >= occurrence)
            {
                return commands[occurrence - 1];
            }

            await Task.Delay(10);
        }

        throw new TimeoutException($"Command {method} was not sent {occurrence} time(s).");
    }

    /// <summary>
    /// Delivers an event, as the remote end would.
    /// </summary>
    /// <param name="method">The event's method, such as "network.beforeRequestSent".</param>
    /// <param name="parameters">The event's parameters.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task RaiseEventAsync(string method, JsonObject parameters)
    {
        return this.DeliverAsync(new JsonObject() { ["type"] = "event", ["method"] = method, ["params"] = parameters });
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
        if (this.unansweredMethods.ContainsKey(method))
        {
            return Task.CompletedTask;
        }

        JsonObject response = new() { ["id"] = (long)command["id"]! };
        IReadOnlyList<JsonObject> events = [];
        if (this.errors.TryGetValue(method, out (string Error, string Message) error))
        {
            response["type"] = "error";
            response["error"] = error.Error;
            response["message"] = error.Message;
        }
        else
        {
            JsonObject parameters = command["params"]?.AsObject() ?? [];
            FakeResponse answer = this.results.TryGetValue(method, out Func<JsonObject, FakeResponse>? createResponse) ? createResponse(parameters) : new FakeResponse(this.CreateDefaultResult(method));
            if (answer.Error is null)
            {
                response["type"] = "success";
                this.sentResults.Enqueue((method, answer.Result.DeepClone()));
                response["result"] = answer.Result;
            }
            else
            {
                response["type"] = "error";
                response["error"] = answer.Error;
                response["message"] = answer.ErrorMessage;
            }

            events = [.. answer.EventsBefore.Select(e => new JsonObject() { ["type"] = "event", ["method"] = e.Method, ["params"] = e.Parameters })];
        }

        // Answered asynchronously, as a remote end would, rather than within the send.
        _ = Task.Run(async () =>
        {
            foreach (JsonObject message in events)
            {
                await this.DeliverAsync(message);
            }

            await this.DeliverAsync(response);
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

    private Task DeliverAsync(JsonObject message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message.ToJsonString());
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        return this.NotifyDataReceivedObserverAsync(owner, bytes.Length);
    }

    private JsonObject CreateDefaultResult(string method)
    {
        string NextId(string prefix) => $"{prefix}-{Interlocked.Increment(ref this.identifierCount)}";
        return method switch
        {
            "session.subscribe" => new JsonObject() { ["subscription"] = NextId("subscription") },
            "network.addIntercept" => new JsonObject() { ["intercept"] = NextId("intercept") },
            "network.addDataCollector" => new JsonObject() { ["collector"] = NextId("collector") },
            "script.addPreloadScript" => new JsonObject() { ["script"] = NextId("preload-script") },
            "browsingContext.getTree" => new JsonObject() { ["contexts"] = new JsonArray() },
            "session.status" => new JsonObject() { ["ready"] = true, ["message"] = "fake" },
            _ => [],
        };
    }
}
