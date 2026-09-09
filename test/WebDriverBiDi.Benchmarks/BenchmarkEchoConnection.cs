// <copyright file="BenchmarkEchoConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Buffers;
using System.Text;
using System.Text.Json;
using WebDriverBiDi.Protocol;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// In-memory <see cref="Connection"/> implementation for benchmark scenarios.
/// When a command is sent, this connection synthesizes a success response
/// bearing the same command ID and fires it back synchronously via
/// <see cref="Connection.OnDataReceived"/>, simulating a no-latency remote end.
/// The synthesis happens in <see cref="SendConnectionDataAsync"/>, the seam a real
/// connection writes its bytes at, so that a benchmark running through this class
/// still pays for the base class's send orchestration.
/// </summary>
/// <remarks>
/// This connection is only suitable for benchmarking. It performs no real
/// network I/O and does not validate the content of commands it receives
/// beyond extracting the "id" field for response correlation.
/// </remarks>
public sealed class BenchmarkEchoConnection : Connection
{
    private volatile bool isActive;

    /// <inheritdoc/>
    public override bool IsActive => this.isActive;

    /// <inheritdoc/>
    public override ConnectionKind ConnectionKind => ConnectionKind.WebSocket;

    /// <inheritdoc/>
    public override Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        this.ConnectionString = connectionString;
        this.isActive = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        this.isActive = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// The echo is implemented here, at the transport-write seam, rather than by overriding
    /// <see cref="Connection.SendDataAsync"/>. That keeps the whole of the base send orchestration
    /// inside the measurement: the two <see cref="Connection.IsActive"/> checks, the trace-guarded
    /// <c>LogMessageContentAsync</c> call, the send semaphore and its release, and the linked
    /// <see cref="CancellationTokenSource"/> built for a caller-supplied token. Overriding
    /// <c>SendDataAsync</c> skipped all of it, so a benchmark documented as end-to-end library
    /// overhead measured less than the library actually does on a send.
    /// </para>
    /// <para>
    /// Everything here runs synchronously on the sender's stack, inside the send semaphore. That is
    /// safe because the transport's data-received observer only writes the message to its unbounded
    /// channel and returns; the reader task that completes the command runs separately, so it cannot
    /// re-enter a send while this one still holds the semaphore.
    /// </para>
    /// </remarks>
    protected override async Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        // Extract the command ID from the outgoing JSON and synthesize a
        // matching success response. The transport layer's incoming-message
        // channel decouples this from the sender's await on WaitForCompletionAsync.
        long commandId = ExtractCommandId(messageBuffer);
        byte[] response = BuildSuccessResponse(commandId);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(response.Length);
        response.CopyTo(owner.Memory);
        await this.InvocableConnectionDataReceivedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDataReceivedEventArgs(owner, response.Length)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override Task ReceiveDataAsync() => Task.CompletedTask;

    /// <inheritdoc/>
    protected override ValueTask DisposeAsyncCore()
    {
        this.SetDisposed();
        return default;
    }

    private static long ExtractCommandId(ReadOnlyMemory<byte> commandJson)
    {
        using JsonDocument document = JsonDocument.Parse(commandJson);
        return document.RootElement.GetProperty("id").GetInt64();
    }

    private static byte[] BuildSuccessResponse(long commandId)
    {
        // Minimal success response envelope. The result body is empty; the
        // CommandExecutionBenchmarks benchmark uses an empty result type so
        // the deserializer can accept this shape.
        // Triple-$ so the literal "{}" after the id is not parsed as an
        // interpolation hole. With $$$, only "{{{...}}}" is a hole.
        string json = $$$"""{"type":"success","id":{{{commandId}}},"result":{}}""";
        return Encoding.UTF8.GetBytes(json);
    }
}
