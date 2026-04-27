// <copyright file="CommandExecutionBenchmarks.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using WebDriverBiDi.Protocol;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// End-to-end benchmark for <see cref="BiDiDriver.ExecuteCommandAsync{T}(WebDriverBiDi.CommandParameters{T}, TimeSpan?, CancellationToken)"/>
/// with an in-memory echo connection. This measures the full happy-path
/// cost of sending a command and receiving its response: JSON serialization,
/// pending-command bookkeeping, channel write/read, response deserialization,
/// and TaskCompletionSource completion. The echo connection has zero
/// network latency, so the number reflects library overhead only.
/// </summary>
[MemoryDiagnoser]
public class CommandExecutionBenchmarks
{
    private BenchmarkEchoConnection connection = null!;
    private BiDiDriver driver = null!;
    private BenchmarkCommandParameters parameters = null!;

    /// <summary>
    /// Creates the driver wired to an echo connection and starts the transport.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.connection = new BenchmarkEchoConnection();
        Transport transport = new(this.connection);
        this.driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        this.driver.StartAsync("benchmark://localhost").GetAwaiter().GetResult();
        this.parameters = new BenchmarkCommandParameters();
    }

    /// <summary>
    /// Stops the driver and releases the transport between benchmark runs.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        this.driver.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes a single command against the echo connection and awaits its
    /// response. The round trip goes all the way through the transport
    /// pipeline: SendCommandAsync → Connection.SendDataAsync (echo) →
    /// Connection.OnDataReceived → Transport message queue → reader task →
    /// Command.SetResult → caller await unblocks.
    /// </summary>
    [Benchmark]
    public async Task ExecuteCommandRoundTrip()
    {
        await this.driver.ExecuteCommandAsync<BenchmarkCommandResult>(this.parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Minimal command parameters class for benchmark use. No fields are
    /// required — the outgoing JSON carries only the id/method/params
    /// envelope that every command shares.
    /// </summary>
    public sealed class BenchmarkCommandParameters : CommandParameters<BenchmarkCommandResult>
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string MethodName => "benchmark.echoCommand";
    }

    /// <summary>
    /// Minimal command result class that matches the empty result body
    /// produced by <see cref="BenchmarkEchoConnection"/>.
    /// </summary>
    public sealed record BenchmarkCommandResult : CommandResult;
}
