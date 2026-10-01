// <copyright file="FileLock.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics;
using WebDriverBiDi;

/// <summary>
/// A cross-process file-based lock. Call <see cref="AcquireAsync"/> to wait until the lock
/// is available, then dispose the returned handle to release it.
/// </summary>
internal sealed class FileLock
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly string lockFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLock"/> class.
    /// </summary>
    /// <param name="lockFilePath">The path to the lock file.</param>
    internal FileLock(string lockFilePath)
    {
        this.lockFilePath = lockFilePath;
    }

    /// <summary>
    /// Waits until the lock is available, acquires it, and returns a handle that releases
    /// the lock when disposed.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the lock.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the lock is not acquired within <paramref name="timeout"/>.</exception>
    internal async Task<IDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.lockFilePath)!);
        Stopwatch waitStopwatch = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                // The file is left in place on release: deleting it lets a waiter that opened the old
                // file and a newcomer that creates a new one both believe they hold the lock.
                return new FileStream(this.lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                if (timeout != Timeout.InfiniteTimeSpan && waitStopwatch.Elapsed >= timeout)
                {
                    throw new WebDriverBiDiTimeoutException($"Timed out after {timeout.TotalSeconds} seconds waiting for the lock file {this.lockFilePath}; another process may be installing the same browser or driver.");
                }

                await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
