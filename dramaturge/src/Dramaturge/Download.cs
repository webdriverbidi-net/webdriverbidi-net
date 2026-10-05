// <copyright file="Download.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;

/// <summary>
/// A download a page began. Where the file is saved, or whether downloads are allowed at all, follows the
/// browser's download behavior, such as set with <see cref="Browser.AllowDownloadsAsync"/>.
/// </summary>
public sealed class Download
{
    private readonly TaskCompletionSource<DownloadOutcome> ended = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="Download"/> class.
    /// </summary>
    /// <param name="frame">The frame whose document began the download.</param>
    /// <param name="url">The URL of the download.</param>
    /// <param name="suggestedFileName">The name the browser suggests for the file.</param>
    internal Download(Frame frame, string url, string suggestedFileName)
    {
        this.Frame = frame;
        this.Url = url;
        this.SuggestedFileName = suggestedFileName;
    }

    /// <summary>
    /// Gets the frame whose document began the download.
    /// </summary>
    public Frame Frame { get; }

    /// <summary>
    /// Gets the page that began the download.
    /// </summary>
    public Page Page => this.Frame.Page;

    /// <summary>
    /// Gets the URL of the download.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the name the browser suggests for the file.
    /// </summary>
    public string SuggestedFileName { get; }

    /// <summary>
    /// Waits for the download to end, completed or canceled.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>How the download ended.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the download does not end in time.</exception>
    public Task<DownloadOutcome> WaitForEndAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        DramaturgeOptions options = this.Page.Browser.Group.Options;
        TimeBudget budget = new(timeout ?? options.NavigationTimeout, options.TimeProvider, cancellationToken);
        TracedCall call = TraceRecording.Call("Download", "Wait for download to end", "{url}", "waitForEnd", ("url", this.Url));
        return TraceRecording.RunAsync(this.Page.Browser, this.Page, budget, call, actionBudget => actionBudget.WaitAsync(this.ended.Task, $"the download of {this.Url} to end"));
    }

    /// <summary>
    /// Records how the download ended.
    /// </summary>
    /// <param name="outcome">How it ended.</param>
    internal void RecordEnd(DownloadOutcome outcome)
    {
        this.ended.TrySetResult(outcome);
    }
}
