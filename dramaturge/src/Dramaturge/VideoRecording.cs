// <copyright file="VideoRecording.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// The recording of a page's video, which disposing it, or <see cref="StopAsync"/>, ends and delivers. The browser
/// writes the file, in the format it chooses, such as WebM; a browser on this machine writes it, or it is moved, to
/// the path asked for, and a browser on another machine, such as a remote grid's, leaves it there.
/// </summary>
public sealed class VideoRecording : IAsyncDisposable
{
    private readonly Page page;
    private readonly string screencastId;
    private readonly SemaphoreSlim stopLock = new(1, 1);
    private bool isStopped;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoRecording"/> class.
    /// </summary>
    /// <param name="page">The recorded page.</param>
    /// <param name="screencastId">The ID of the browser's screencast.</param>
    /// <param name="path">The full path asked for.</param>
    internal VideoRecording(Page page, string screencastId, string path)
    {
        this.page = page;
        this.screencastId = screencastId;
        this.Path = path;
    }

    /// <summary>
    /// Gets the path of the video: the full path asked for, or, once stopped, of a browser on another machine, the
    /// path of the file there.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Ends the recording and delivers the file. Stopping again returns the same path.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The video's path.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the browser reports an error writing the video.</exception>
    public async Task<string> StopAsync(CancellationToken cancellationToken = default)
    {
        await this.stopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.isStopped)
            {
                return this.Path;
            }

            StopScreencastCommandResult result = await this.page.Browser.Group.Driver.BrowsingContext.StopScreencastAsync(new StopScreencastCommandParameters(this.screencastId), cancellationToken: cancellationToken).ConfigureAwait(false);
            this.isStopped = true;
            if (result.Error is not null)
            {
                throw new InvalidOperationException($"The browser could not write the video of the page: {result.Error}");
            }

            this.Path = Deliver(result.Path, this.Path);
            return this.Path;
        }
        finally
        {
            this.stopLock.Release();
        }
    }

    /// <summary>
    /// Ends the recording and delivers the file, as <see cref="StopAsync"/> does.
    /// </summary>
    /// <returns>A task that completes when the file is delivered.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.StopAsync().ConfigureAwait(false);
    }

    // A browser that writes elsewhere on this machine, such as one that ignores the folder it is given, has its file
    // moved; a file that is not on this machine stays where the browser wrote it.
    private static string Deliver(string written, string requested)
    {
        if (written.Length == 0 || !File.Exists(written))
        {
            return written.Length == 0 ? requested : written;
        }

        string source = System.IO.Path.GetFullPath(written);
        if (source != requested)
        {
            if (File.Exists(requested))
            {
                File.Delete(requested);
            }

            File.Move(source, requested);
        }

        return requested;
    }
}
