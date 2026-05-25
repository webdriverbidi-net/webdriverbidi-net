// <copyright file="FileLock.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// A cross-process file-based lock. Call <see cref="AcquireAsync"/> to wait until the lock
/// is available, then dispose the returned handle to release it.
/// </summary>
internal sealed class FileLock
{
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
    /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
    internal async Task<IDisposable> AcquireAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.lockFilePath)!);
        while (true)
        {
            try
            {
                FileStream stream = new(this.lockFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                return new LockHandle(stream, this.lockFilePath);
            }
            catch (IOException)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
        }
    }

    private sealed class LockHandle : IDisposable
    {
        private readonly FileStream stream;
        private readonly string lockFilePath;
        private bool disposed;

        internal LockHandle(FileStream stream, string lockFilePath)
        {
            this.stream = stream;
            this.lockFilePath = lockFilePath;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.stream.Close();
            File.Delete(this.lockFilePath);
        }
    }
}
