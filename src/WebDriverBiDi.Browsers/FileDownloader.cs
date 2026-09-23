// <copyright file="FileDownloader.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Downloads files, reporting their progress.
/// </summary>
internal sealed class FileDownloader
{
    // 1 MB buffer for downloads
    private const int BufferSize = 1024 * 1024;

    // Reads return far less than the buffer, so progress is reported only this often.
    private const long ProgressReportInterval = 1024 * 1024;

    private readonly string name;
    private readonly IProgress<BrowserDownloadProgress>? progress;
    private readonly Func<string, Task> logAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloader"/> class.
    /// </summary>
    /// <param name="name">The name and version of what is being downloaded.</param>
    /// <param name="progress">Receives progress reports, or <see langword="null"/> for none.</param>
    /// <param name="logAsync">Logs a message at every 10% of the download.</param>
    public FileDownloader(string name, IProgress<BrowserDownloadProgress>? progress, Func<string, Task> logAsync)
    {
        this.name = name;
        this.progress = progress;
        this.logAsync = logAsync;
    }

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination path.
    /// </summary>
    /// <param name="client">The HTTP client to use for the download.</param>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destPath">The path where the downloaded file should be saved.</param>
    /// <param name="cancellationToken">A token that cancels the download.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DownloadFileAsync(HttpClient client, string url, string destPath, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using FileStream fileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

        byte[] buffer = new byte[BufferSize];
        long totalRead = 0;
        long lastReported = 0;
        int lastLoggedPercent = -1;
        this.progress?.Report(new BrowserDownloadProgress(this.name, 0, totalBytes));
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalRead += bytesRead;
            if (totalRead - lastReported >= ProgressReportInterval || totalRead == totalBytes)
            {
                this.progress?.Report(new BrowserDownloadProgress(this.name, totalRead, totalBytes));
                lastReported = totalRead;
            }

            if (totalBytes > 0)
            {
                int percent = (int)(totalRead * 100 / totalBytes.Value);
                if (percent != lastLoggedPercent && percent % 10 == 0)
                {
                    await this.logAsync($"  Download progress: {percent}%").ConfigureAwait(false);
                    lastLoggedPercent = percent;
                }
            }
        }

        if (lastReported != totalRead)
        {
            this.progress?.Report(new BrowserDownloadProgress(this.name, totalRead, totalBytes));
        }
    }
}
