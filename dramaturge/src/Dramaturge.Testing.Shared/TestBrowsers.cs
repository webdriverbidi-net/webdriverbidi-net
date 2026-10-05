// <copyright file="TestBrowsers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

/// <summary>
/// The browsers one test opens, and their pages, numbered in the order they open: traced and recorded, if traces and
/// videos are on; saved, if the test fails; and closed when it ends.
/// </summary>
internal sealed class TestBrowsers
{
    private const int MaximumDirectoryNameLength = 120;

    private readonly object lockObject = new();
    private readonly List<Browser> browsers = [];
    private readonly List<TrackedPage> pages = [];
    private readonly List<IDisposable> observers = [];
    private readonly List<TrackedTrace> traces = [];
    private readonly string temporaryFolder = Path.Combine(Path.GetTempPath(), $"dramaturge-captures-{Guid.NewGuid():N}");
    private NotSupportedException? videoUnsupported;

    /// <summary>
    /// Gets the default directory under which a failed test's screenshots and videos are written.
    /// </summary>
    public static string DefaultArtifactsDirectory => Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge");

    /// <summary>
    /// Gets the settings of the traces of a test package's <c>TraceOnFailure</c>, unless it gives its own: snapshots,
    /// screenshots, and sources.
    /// </summary>
    public static TraceRecordingOptions DefaultTraceOptions { get; } = new() { Snapshots = true, Screenshots = true, Sources = true };

    /// <summary>
    /// Creates a browser to be closed when the test ends, whose pages are tracked from when they open.
    /// </summary>
    /// <param name="group">The group in which to create it.</param>
    /// <param name="options">The browser's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="video">The settings of each page's video, or <see langword="null"/> not to record.</param>
    /// <param name="trace">The settings of the browser's trace, or <see langword="null"/> not to record one.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The browser.</returns>
    public async Task<Browser> CreateAsync(BrowserGroup group, BrowserOptions? options, VideoRecordingOptions? video, TraceRecordingOptions? trace, CancellationToken cancellationToken)
    {
        Browser browser = await group.CreateBrowserAsync(options, cancellationToken).ConfigureAwait(false);
        int number;
        lock (this.lockObject)
        {
            this.browsers.Add(browser);
            this.observers.Add(browser.OnPageCreated.AddObserver(e => this.TrackAsync(e.Page, video)));
            number = this.browsers.Count;
        }

        if (trace is not null)
        {
            TrackedTrace tracked = new(number);
            try
            {
                tracked.Recording = await browser.RecordTraceAsync(Path.Combine(this.temporaryFolder, $"browser-{number}.zip"), trace, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                tracked.Error = exception;
            }

            lock (this.lockObject)
            {
                this.traces.Add(tracked);
            }
        }

        return browser;
    }

    /// <summary>
    /// Ends the test's recordings and, if it failed, saves each open page's screenshot as page-1.png, page-2.png, and
    /// so on, each page's video beside it as page-1.webm and so on, and the browsers' traces, merged, as trace.zip, in
    /// a directory named for the test, replacing an earlier run's. A passed test's videos and traces are deleted. A
    /// browser that cannot record video is reported, failed or not.
    /// </summary>
    /// <param name="failed">A value indicating whether the test failed.</param>
    /// <param name="screenshots">A value indicating whether a failed test's pages are captured.</param>
    /// <param name="artifactsDirectory">The directory under which the test's directory is made.</param>
    /// <param name="testName">The test's name.</param>
    /// <returns>Each file saved, and each failure to save or record one.</returns>
    public async Task<IReadOnlyList<PageCapture>> FinishAsync(bool failed, bool screenshots, string artifactsDirectory, string testName)
    {
        List<TrackedPage> tracked;
        List<TrackedTrace> traced;
        lock (this.lockObject)
        {
            tracked = [.. this.pages];
            traced = [.. this.traces];
        }

        // The traces end first, so that they do not record the screenshots taken for the test.
        List<PageCapture> captures = [];
        List<string> traceFiles = [];
        foreach (TrackedTrace trace in traced)
        {
            PageCapture? failure = await EndTraceAsync(trace, traceFiles).ConfigureAwait(false);
            if (failure is not null && failed)
            {
                captures.Add(failure);
            }
        }

        string directory = Path.Combine(artifactsDirectory, ToDirectoryName(testName));
        bool saving = failed && ((screenshots && tracked.Any(page => !page.Page.IsClosed)) || tracked.Any(page => page.Video is not null) || traceFiles.Count > 0);
        if (saving)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }

                Directory.CreateDirectory(directory);
            }
            catch (Exception exception)
            {
                captures.Add(Failed(directory, PageCapture.Screenshot, $"Dramaturge could not save a screenshot to {directory}: {exception.Message}"));
                saving = false;
            }
        }

        if (saving && screenshots)
        {
            foreach (TrackedPage page in tracked.Where(page => !page.Page.IsClosed))
            {
                captures.Add(await SaveScreenshotAsync(page, Path.Combine(directory, $"page-{page.Number}.png")).ConfigureAwait(false));
            }
        }

        foreach (TrackedPage page in tracked)
        {
            if (page.Video is not null)
            {
                PageCapture? video = await EndVideoAsync(page, saving ? Path.Combine(directory, $"page-{page.Number}.webm") : null).ConfigureAwait(false);
                if (video is not null && failed)
                {
                    captures.Add(video);
                }
            }
            else if (page.VideoError is not null && failed)
            {
                captures.Add(Failed(string.Empty, PageCapture.Video, $"Dramaturge could not record a video of page {page.Number}: {page.VideoError.Message}"));
            }
        }

        if (saving && traceFiles.Count > 0)
        {
            captures.Add(SaveTrace(traceFiles, Path.Combine(directory, "trace.zip")));
        }

        if (this.videoUnsupported is not null)
        {
            captures.Add(Failed(string.Empty, PageCapture.Video, $"Dramaturge could not record video: {this.videoUnsupported.Message}"));
        }

        DeleteTemporaryFolder(this.temporaryFolder);
        return captures;
    }

    /// <summary>
    /// Closes the test's browsers, each even if closing another fails.
    /// </summary>
    /// <returns>The failures to close a browser.</returns>
    public async Task<IReadOnlyList<Exception>> CloseAsync()
    {
        List<Browser> closing;
        lock (this.lockObject)
        {
            closing = [.. this.browsers];
            this.browsers.Clear();
            foreach (IDisposable observer in this.observers)
            {
                observer.Dispose();
            }

            this.observers.Clear();
        }

        List<Exception> failures = [];
        foreach (Browser browser in closing)
        {
            try
            {
                await browser.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        return failures;
    }

    // Letters, digits, '.', '-', and '_' are kept, so that the name is valid on every platform.
    private static string ToDirectoryName(string testName)
    {
        StringBuilder name = new(testName.Length);
        foreach (char character in testName)
        {
            name.Append(char.IsLetterOrDigit(character) || character is '.' or '-' or '_' ? character : '_');
        }

        return name.Length > MaximumDirectoryNameLength ? name.ToString(0, MaximumDirectoryNameLength) : name.ToString();
    }

    private static PageCapture Failed(string path, string mediaType, string failure) => new(path, null, mediaType, failure);

    private static async Task<PageCapture> SaveScreenshotAsync(TrackedPage page, string path)
    {
        try
        {
            byte[] screenshot = await page.Page.ScreenshotAsync().ConfigureAwait(false);
            File.WriteAllBytes(path, screenshot);
            return new PageCapture(path, screenshot, PageCapture.Screenshot, null);
        }
        catch (Exception exception)
        {
            return Failed(path, PageCapture.Screenshot, $"Dramaturge could not save a screenshot to {path}: {exception.Message}");
        }
    }

    // A video is kept at the path given, if any, and deleted otherwise. A video the browser left on another machine
    // cannot be kept here, and is reported where it is.
    private static async Task<PageCapture?> EndVideoAsync(TrackedPage page, string? path)
    {
        try
        {
            string written = await page.Video!.StopAsync().ConfigureAwait(false);
            if (!File.Exists(written))
            {
                return Failed(written, PageCapture.Video, $"Dramaturge could not save the video of page {page.Number}: the browser wrote it on its own machine, at {written}");
            }

            if (path is null)
            {
                File.Delete(written);
                return null;
            }

            File.Move(written, path);
            return new PageCapture(path, File.ReadAllBytes(path), PageCapture.Video, null);
        }
        catch (Exception exception)
        {
            return Failed(path ?? string.Empty, PageCapture.Video, $"Dramaturge could not save the video of page {page.Number}: {exception.Message}");
        }
    }

    [ExcludeFromCodeCoverage] // Fails only while the browser still holds a video open.
    private static void DeleteTemporaryFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch (IOException)
        {
            // A video the browser still holds open is left in the temporary folder.
        }
    }

    // A trace that could not start or be written is a failure, reported for a failed test.
    private static Task<PageCapture?> EndTraceAsync(TrackedTrace trace, List<string> files)
    {
        return trace.Recording is null
            ? Task.FromResult<PageCapture?>(Failed(string.Empty, PageCapture.Trace, $"Dramaturge could not record a trace of browser {trace.Number}: {trace.Error!.Message}"))
            : StopTraceAsync(trace.Number, trace.Recording, files);
    }

    [ExcludeFromCodeCoverage] // Fails only when the trace cannot be written to the temporary folder.
    private static async Task<PageCapture?> StopTraceAsync(int number, TraceRecording recording, List<string> files)
    {
        try
        {
            files.Add(await recording.StopAsync().ConfigureAwait(false));
            return null;
        }
        catch (Exception exception)
        {
            return Failed(recording.Path, PageCapture.Trace, $"Dramaturge could not save the trace of browser {number}: {exception.Message}");
        }
    }

    [ExcludeFromCodeCoverage] // Fails only when the test's directory, just made, cannot be written.
    private static PageCapture SaveTrace(List<string> files, string path)
    {
        try
        {
            MergeTraces(files, path);
            return new PageCapture(path, File.ReadAllBytes(path), PageCapture.Trace, null);
        }
        catch (Exception exception)
        {
            return Failed(path, PageCapture.Trace, $"Dramaturge could not save the trace to {path}: {exception.Message}");
        }
    }

    // Each browser's trace is a context of its own in the merged trace, named by its index, as Playwright's test runner
    // merges them; the files the traces refer to, named for their content, are written once.
    private static void MergeTraces(List<string> files, string path)
    {
        using ZipArchive merged = ZipFile.Open(path, ZipArchiveMode.Create);
        HashSet<string> written = [];
        for (int index = 0; index < files.Count; index++)
        {
            using ZipArchive trace = ZipFile.OpenRead(files[index]);
            foreach (ZipArchiveEntry entry in trace.Entries)
            {
                string name = entry.FullName is "trace.trace" or "trace.network" ? $"{index}-{entry.FullName}" : entry.FullName;
                if (written.Add(name))
                {
                    using Stream source = entry.Open();
                    using Stream target = merged.CreateEntry(name).Open();
                    source.CopyTo(target);
                }
            }
        }
    }

    private async Task TrackAsync(Page page, VideoRecordingOptions? video)
    {
        TrackedPage tracked;
        lock (this.lockObject)
        {
            tracked = new TrackedPage(page, this.pages.Count + 1);
            this.pages.Add(tracked);
        }

        if (video is null)
        {
            return;
        }

        try
        {
            tracked.Video = await page.RecordVideoAsync(Path.Combine(this.temporaryFolder, $"page-{tracked.Number}.webm"), video).ConfigureAwait(false);
        }
        catch (NotSupportedException exception)
        {
            lock (this.lockObject)
            {
                this.videoUnsupported ??= exception;
            }
        }
        catch (Exception exception)
        {
            tracked.VideoError = exception;
        }
    }

    private sealed class TrackedTrace(int number)
    {
        public int Number { get; } = number;

        public TraceRecording? Recording { get; set; }

        public Exception? Error { get; set; }
    }

    private sealed class TrackedPage(Page page, int number)
    {
        public Page Page { get; } = page;

        public int Number { get; } = number;

        public VideoRecording? Video { get; set; }

        public Exception? VideoError { get; set; }
    }
}
