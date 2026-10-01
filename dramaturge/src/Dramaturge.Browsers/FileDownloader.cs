// <copyright file="FileDownloader.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Security.Cryptography;

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
    /// Downloads a file, downloading it again if it is interrupted or corrupted in transit, and
    /// verifies it against its published checksum and size, if given.
    /// </summary>
    /// <param name="client">The HTTP client to use for the download.</param>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destPath">The path where the downloaded file should be saved.</param>
    /// <param name="expectedSha256">The file's published SHA-256 hash in hexadecimal, or <see langword="null"/> if none is published.</param>
    /// <param name="expectedSize">The file's published size in bytes, or <see langword="null"/> if none is published.</param>
    /// <param name="cancellationToken">A token that cancels the download.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="DownloadVerificationException">Thrown when the file does not match its published checksum or size.</exception>
    public async Task DownloadFileAsync(HttpClient client, string url, string destPath, string? expectedSha256, long? expectedSize, CancellationToken cancellationToken)
    {
        string sha256 = await DownloadHttpClient.RetryAsync(() => this.DownloadOnceAsync(client, new Uri(url), destPath, cancellationToken), cancellationToken).ConfigureAwait(false);
        if (expectedSha256 is not null && !sha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new DownloadVerificationException($"{this.name} from {url} has SHA-256 hash {sha256}, but its publisher lists {expectedSha256}.");
        }

        long size = new FileInfo(destPath).Length;
        if (expectedSize is not null && size != expectedSize)
        {
            throw new DownloadVerificationException($"{this.name} from {url} is {size} bytes, but its publisher lists {expectedSize} bytes.");
        }
    }

    // Google Cloud Storage, which serves Chrome for Testing, sends an MD5 hash of the file, as does the
    // msedgedriver server in Content-MD5. It comes from the same server as the file, so it detects
    // corruption in transit, not tampering.
    private static string? GetStorageMd5(HttpResponseMessage response)
    {
        const string Md5Prefix = "md5=";
        if (response.Content.Headers.ContentMD5 is byte[] contentMd5)
        {
            return Convert.ToBase64String(contentMd5);
        }

        if (response.Headers.TryGetValues("x-goog-hash", out IEnumerable<string>? values))
        {
            foreach (string value in values.SelectMany(value => value.Split(',')))
            {
                string hash = value.Trim();
                if (hash.StartsWith(Md5Prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return hash.Substring(Md5Prefix.Length);
                }
            }
        }

        return null;
    }

    private static string ToHex(byte[] bytes)
    {
        return string.Concat(bytes.Select(value => value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static async Task<int> ReadAsync(Stream stream, byte[] buffer, Uri url, CancellationToken cancellationToken)
    {
        try
        {
            return await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new TransientDownloadException($"The download from {url} was interrupted: {ex.Message}", ex);
        }
    }

    // Returns the SHA-256 hash of the downloaded file in hexadecimal.
    private async Task<string> DownloadOnceAsync(HttpClient client, Uri url, string destPath, CancellationToken cancellationToken)
    {
        if (url.IsFile)
        {
            using FileStream source = new(url.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
            return await this.CopyAsync(source, source.Length, null, url, destPath, cancellationToken).ConfigureAwait(false);
        }

        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        DownloadServiceException.ThrowIfUnsuccessful(response, url);
        using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return await this.CopyAsync(contentStream, response.Content.Headers.ContentLength, GetStorageMd5(response), url, destPath, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> CopyAsync(Stream contentStream, long? totalBytes, string? expectedMd5, Uri url, string destPath, CancellationToken cancellationToken)
    {
        using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
#pragma warning disable CA5351 // Verifies the server's own transfer checksum; not used for security.
        using IncrementalHash? md5 = expectedMd5 is null ? null : IncrementalHash.CreateHash(HashAlgorithmName.MD5);
#pragma warning restore CA5351
        using FileStream fileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

        byte[] buffer = new byte[BufferSize];
        long totalRead = 0;
        long lastReported = 0;
        int lastLoggedTenth = 0;
        this.progress?.Report(new BrowserDownloadProgress(this.name, 0, totalBytes));
        int bytesRead;
        while ((bytesRead = await ReadAsync(contentStream, buffer, url, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            sha256.AppendData(buffer, 0, bytesRead);
            md5?.AppendData(buffer, 0, bytesRead);
            totalRead += bytesRead;
            if (totalRead - lastReported >= ProgressReportInterval || totalRead == totalBytes)
            {
                this.progress?.Report(new BrowserDownloadProgress(this.name, totalRead, totalBytes));
                lastReported = totalRead;
            }

            // Logged at each tenth of the download, whichever read reaches it, since one read can span several percent.
            if (totalBytes > 0)
            {
                int tenth = (int)(totalRead * 10 / totalBytes.Value);
                if (tenth > lastLoggedTenth)
                {
                    await this.logAsync($"  Download progress: {tenth * 10}%").ConfigureAwait(false);
                    lastLoggedTenth = tenth;
                }
            }
        }

        if (lastReported != totalRead)
        {
            this.progress?.Report(new BrowserDownloadProgress(this.name, totalRead, totalBytes));
        }

        if (totalBytes is not null && totalRead != totalBytes)
        {
            throw new TransientDownloadException($"The download of {this.name} from {url} ended after {totalRead} of {totalBytes} bytes.");
        }

        if (md5 is not null && Convert.ToBase64String(md5.GetHashAndReset()) != expectedMd5)
        {
            throw new TransientDownloadException($"The download of {this.name} from {url} does not match the MD5 hash the server sent.");
        }

        return ToHex(sha256.GetHashAndReset());
    }
}
