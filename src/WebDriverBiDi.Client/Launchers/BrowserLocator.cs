// <copyright file="BrowserLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Base class for locating and downloading browsers for testing.
/// </summary>
public abstract class BrowserLocator
{
    /// <summary>
    /// Gets the base cache directory for downloaded browsers, which is a subdirectory of the user's profile directory.
    /// </summary>
    protected static readonly string BaseCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "webdriverbidi-net");

    // 1 MB buffer for downloads
    private const int BufferSize = 1024 * 1024;

    private static readonly string TimestampFile = Path.Combine(BaseCacheDir, "last-download.txt");

    /// <summary>
    /// Gets a locator for Firefox binaries.
    /// </summary>
    public static FirefoxLocator Firefox { get; } = new FirefoxLocator();

    /// <summary>
    /// Gets a locator for Chrome binaries.
    /// </summary>
    public static ChromeLocator Chrome { get; } = new ChromeLocator();

    /// <summary>
    /// Gets the name of the environment variable that can be used to specify a custom path to the Firefox executable.
    /// If this environment variable is set, the locator will return its value directly instead of downloading Firefox.
    /// </summary>
    protected abstract string EnvironmentVariableName { get; }

    /// <summary>
    /// Gets the user-friendly name of the browser being located, used for logging and error messages.
    /// </summary>
    protected abstract string BrowserName { get; }

    /// <summary>
    /// Gets the cache directory specific to this browser, which is a subdirectory of the base cache directory.
    /// </summary>
    protected abstract string CacheDir { get; }

    /// <summary>
    /// Gets the path to the browser executable, downloading it if necessary.
    /// If the browser-specific environment variable is set, returns that path directly.
    /// </summary>
    /// <returns>The path to the Firefox executable.</returns>
    public async Task<string> GetBrowserExecutablePathAsync()
    {
        string? envPath = Environment.GetEnvironmentVariable(this.EnvironmentVariableName);
        if (!string.IsNullOrEmpty(envPath))
        {
            Console.WriteLine($"Using '{this.EnvironmentVariableName}': {envPath}");
            return envPath;
        }

        // This is a very naive implementation. It will result in downloading once per day,
        // without regard to browser binary version. A future implementation should account
        // for the binary version number.
        string executablePath = this.GetExpectedExecutablePath();
        if (File.Exists(executablePath) && !this.IsCacheStale())
        {
            Console.WriteLine($"Using cached {this.BrowserName}.");
            return executablePath;
        }

        Console.WriteLine($"Downloading {this.BrowserName}...");
        await this.DownloadAndExtractAsync();

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"{this.BrowserName} executable not found after extraction at: {executablePath}");
        }

        File.WriteAllText(TimestampFile, DateTimeOffset.UtcNow.ToString("o"));
        Console.WriteLine($"{this.BrowserName} ready at: {executablePath}");
        return executablePath;
    }

    /// <summary>
    /// Gets the direct download URL for the Firefox installer for the current platform.
    /// </summary>
    /// <returns>The direct download URL for the Firefox installer for the current platform.</returns>
    protected abstract Task<string> GetDownloadUrl();

    /// <summary>
    /// Gets the file name used for the downloaded installer for the current platform.
    /// </summary>
    /// <returns>The file name for the downloaded installer.</returns>
    protected abstract string GetInstallerFileName();

    /// <summary>
    /// Gets the expected path to the Firefox executable after extraction, based on the platform.
    /// </summary>
    /// <returns>The expected path to the Firefox executable after extraction, based on the platform.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
    protected abstract string GetExpectedExecutablePath();

    /// <summary>
    /// Performs the download and extraction of the browser for the current platform.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task DownloadAndExtractAsync();

    /// <summary>
    /// Gets a value indicating whether the browser cache has been updated within the last 24 hours.
    /// </summary>
    /// <returns><see langword="true"/> if the cache needs to be refreshed; otherwise <see langword="false"/>.</returns>
    protected bool IsCacheStale()
    {
        if (!File.Exists(TimestampFile))
        {
            return true;
        }

        string text = File.ReadAllText(TimestampFile).Trim();
        if (DateTimeOffset.TryParse(text, out DateTimeOffset lastDownload))
        {
            return DateTimeOffset.UtcNow - lastDownload > TimeSpan.FromHours(24);
        }

        return true;
    }

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination path.
    /// </summary>
    /// <param name="client">The HTTP client to use for the download.</param>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destPath">The path where the downloaded file should be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DownloadFileAsync(HttpClient client, string url, string destPath)
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
                    Console.WriteLine($"  Download progress: {percent}%");
                    lastPercent = percent;
                }
            }
        }
    }
}
