// <copyright file="PendingCommandCollectionBenchmarks.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BenchmarkDotNet.Attributes;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// Benchmarks for the transport's pending-command bookkeeping. Every command
/// traverses an add-then-remove cycle through <see cref="PendingCommandCollection"/>:
/// acquire the addition semaphore, insert into the backing concurrent
/// dictionary, release the semaphore, then later remove from the dictionary
/// when the response arrives. This benchmark exercises that round trip in
/// isolation from the rest of the transport.
/// </summary>
[MemoryDiagnoser]
public class PendingCommandCollectionBenchmarks
{
    private PendingCommandCollection collection = null!;
    private Command command = null!;

    /// <summary>
    /// Sets up test fixtures shared across all iterations.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.collection = new PendingCommandCollection();
        // A single reused Command keeps Command construction cost out of the
        // benchmark body. The add/remove pair leaves the collection empty at
        // the end of every iteration, so the same CommandId can be re-added.
        this.command = new Command(1, new GetRealmsCommandParameters());
    }

    /// <summary>
    /// Releases the collection between benchmark runs.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        this.collection.Dispose();
    }

    /// <summary>
    /// Measures the full add-then-remove round trip through the collection.
    /// </summary>
    [Benchmark]
    public bool AddRemovePendingCommand()
    {
        this.collection.AddPendingCommandAsync(this.command).GetAwaiter().GetResult();
        return this.collection.RemovePendingCommand(this.command.CommandId, out _);
    }
}
