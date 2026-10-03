// <copyright file="HarRecording.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Network;

/// <summary>
/// The recording of a page's or a browser's network traffic to an HTTP Archive file, which disposing it writes. A
/// request is written once it has completed or failed and its bodies have been retrieved.
/// </summary>
public sealed class HarRecording : IAsyncDisposable
{
    private readonly NetworkTrafficMonitor monitor;
    private readonly Func<NetworkRequest, bool> include;
    private readonly TimeSpan defaultWait;
    private readonly List<NetworkRequest> recorded = [];
    private readonly SemaphoreSlim saveLock = new(1, 1);
    private int isDisposed;

    private HarRecording(string path, NetworkTrafficMonitor monitor, Func<NetworkRequest, bool> include, TimeSpan defaultWait)
    {
        this.Path = System.IO.Path.GetFullPath(path);
        this.monitor = monitor;
        this.include = include;
        this.defaultWait = defaultWait;
    }

    /// <summary>
    /// Gets the full path of the file the recording is written to.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Writes the requests recorded so far to the file, replacing what an earlier save wrote, after waiting for those
    /// in flight to complete. A request still in flight when the wait ends is written by a later save.
    /// </summary>
    /// <param name="timeout">How long to wait for requests in flight, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the file is written.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the recording has been disposed.</exception>
    public async Task SaveAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref this.isDisposed) == 1)
        {
            throw new ObjectDisposedException(nameof(HarRecording));
        }

        await this.CollectAndWriteAsync(timeout ?? this.defaultWait, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stops recording and writes the file, after waiting, for at most <see cref="DramaturgeOptions.NavigationTimeout"/>,
    /// for requests in flight; a request still in flight then is written as failed.
    /// </summary>
    /// <returns>A task that completes when the file is written.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.isDisposed, 1) == 1)
        {
            return;
        }

        await this.CollectAndWriteAsync(this.defaultWait, CancellationToken.None).ConfigureAwait(false);
        await this.monitor.DisposeAsync().ConfigureAwait(false);
        await this.CollectAndWriteAsync(TimeSpan.Zero, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="group">The group whose driver records.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="limit">Limits the monitor to the page or browser that records.</param>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>The recording.</returns>
    internal static async Task<HarRecording> StartAsync(BrowserGroup group, string path, HarRecordingOptions? options, Action<NetworkTrafficMonitorOptions> limit, CancellationToken cancellationToken)
    {
        NetworkTrafficMonitorOptions monitorOptions = new() { CaptureBodies = options?.CaptureBodies ?? true };
        limit(monitorOptions);
        NetworkTrafficMonitor monitor = new(group.Driver, monitorOptions);
        await monitor.StartMonitoringAsync(cancellationToken).ConfigureAwait(false);
        return new HarRecording(path, monitor, options?.Include ?? (_ => true), group.Options.NavigationTimeout);
    }

    private async Task CollectAndWriteAsync(TimeSpan wait, CancellationToken cancellationToken)
    {
        await this.saveLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IReadOnlyList<NetworkRequest> captured = await this.monitor.GetCapturedTrafficAsync(wait, cancellationToken).ConfigureAwait(false);
            this.recorded.AddRange(captured.Where(this.include));
            string? directory = System.IO.Path.GetDirectoryName(this.Path);
            Directory.CreateDirectory(directory!);
            File.WriteAllText(this.Path, HarGenerator.Generate(this.recorded));
        }
        finally
        {
            this.saveLock.Release();
        }
    }
}
