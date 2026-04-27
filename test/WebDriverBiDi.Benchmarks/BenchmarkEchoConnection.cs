// <copyright file="BenchmarkEchoConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;
using System.Text.Json;
using WebDriverBiDi.Protocol;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// In-memory <see cref="Connection"/> implementation for benchmark scenarios.
/// When a command is sent, this connection synthesizes a success response
/// bearing the same command ID and fires it back synchronously via
/// <see cref="Connection.OnDataReceived"/>, simulating a no-latency remote end.
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
    public override ConnectionType ConnectionType => ConnectionType.WebSocket;

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
    public override async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        // Extract the command ID from the outgoing JSON and synthesize a
        // matching success response. Everything runs synchronously on the
        // sender's stack; the transport layer's incoming-message channel
        // decouples this from the sender's await on WaitForCompletionAsync.
        long commandId = ExtractCommandId(data);
        byte[] response = BuildSuccessResponse(commandId);
        await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(response)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override Task ReceiveDataAsync() => Task.CompletedTask;

    /// <inheritdoc/>
    protected override ValueTask DisposeAsyncCore()
    {
        this.SetDisposed();
        return default;
    }

    private static long ExtractCommandId(byte[] commandJson)
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
