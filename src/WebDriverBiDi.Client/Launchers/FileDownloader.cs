// <copyright file="FileDownloader.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1402 // File may only contain a single type

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Class for downloading files with progress reporting.
/// </summary>
internal class FileDownloader
{
    // 1 MB buffer for downloads
    private const int BufferSize = 1024 * 1024;

    /// <summary>
    /// Gets an observable event that notifies when download progress is updated.
    /// </summary>
    public ObservableEvent<FileDownloadProgressEventArgs> OnDownloadProgress { get; } = new("fileDownloader.downloadProgress");

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination path.
    /// </summary>
    /// <param name="client">The HTTP client to use for the download.</param>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destPath">The path where the downloaded file should be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task DownloadFileAsync(HttpClient client, string url, string destPath)
    {
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

        byte[] buffer = new byte[BufferSize];
        long totalRead = 0;
        int bytesRead;
        int lastPercent = -1;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalRead += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                int percent = (int)(totalRead * 100 / totalBytes.Value);
                if (percent != lastPercent && percent % 10 == 0)
                {
                    FileDownloadProgressEventArgs progressArgs = new(percent);
                    await this.OnDownloadProgress.NotifyObserversAsync(progressArgs).ConfigureAwait(false);
                    lastPercent = percent;
                }
            }
        }
    }
}
