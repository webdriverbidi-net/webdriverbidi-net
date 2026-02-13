// <copyright file="FirefoxNightlyFetcher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Downloads and caches Firefox Nightly for integration testing.
/// </summary>
internal static class FirefoxNightlyFetcher
{
    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".webdriverbidi-net",
        "firefox-nightly");

    private static readonly string TimestampFile = Path.Combine(CacheDir, ".last-download");

    /// <summary>
    /// Gets the path to the Firefox Nightly executable, downloading it if necessary.
    /// If the FIREFOX_EXECUTABLE environment variable is set, returns that path directly.
    /// </summary>
    /// <returns>The path to the Firefox executable.</returns>
    public static async Task<string> GetFirefoxPathAsync()
    {
        string? envPath = Environment.GetEnvironmentVariable("FIREFOX_EXECUTABLE");
        if (!string.IsNullOrEmpty(envPath))
        {
            Console.WriteLine($"Using FIREFOX_EXECUTABLE: {envPath}");
            return envPath;
        }

        string executablePath = GetExpectedExecutablePath();
        if (File.Exists(executablePath) && !IsCacheStale())
        {
            Console.WriteLine("Using cached Firefox Nightly.");
            return executablePath;
        }

        Console.WriteLine("Downloading Firefox Nightly...");
        await DownloadAndExtractAsync();

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Firefox executable not found after extraction at: {executablePath}");
        }

        File.WriteAllText(TimestampFile, DateTimeOffset.UtcNow.ToString("o"));
        Console.WriteLine($"Firefox Nightly ready at: {executablePath}");
        return executablePath;
    }

    private static bool IsCacheStale()
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

    private static string GetExpectedExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(CacheDir, "Firefox Nightly.app", "Contents", "MacOS", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(CacheDir, "firefox", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(CacheDir, "firefox", "firefox.exe");
        }

        throw new PlatformNotSupportedException("Unsupported platform for Firefox Nightly download.");
    }

    private static string GetDownloadUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "https://download.mozilla.org/?product=firefox-nightly-latest-ssl&os=osx&lang=en-US";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "https://download.mozilla.org/?product=firefox-nightly-latest-ssl&os=linux64&lang=en-US";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "https://download.mozilla.org/?product=firefox-nightly-latest-ssl&os=win64&lang=en-US";
        }

        throw new PlatformNotSupportedException("Unsupported platform for Firefox Nightly download.");
    }

    private static async Task DownloadAndExtractAsync()
    {
        Directory.CreateDirectory(CacheDir);

        string url = GetDownloadUrl();
        using HttpClient client = new();
        client.Timeout = TimeSpan.FromMinutes(10);

        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await DownloadAndExtractMacOsAsync(client, url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await DownloadAndExtractLinuxAsync(client, url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await DownloadAndExtractWindowsAsync(client, url);
        }
    }

    private static async Task DownloadAndExtractMacOsAsync(HttpClient client, string url)
    {
        string dmgPath = Path.Combine(CacheDir, "firefox-nightly.dmg");
        await DownloadFileAsync(client, url, dmgPath);

        string mountPoint = Path.Combine(CacheDir, "dmg-mount");
        Directory.CreateDirectory(mountPoint);

        try
        {
            await RunProcessAsync("hdiutil", $"attach \"{dmgPath}\" -mountpoint \"{mountPoint}\" -nobrowse -quiet");

            // Find the .app bundle in the mounted DMG
            string? appBundle = Directory.GetDirectories(mountPoint, "*.app").FirstOrDefault();
            if (appBundle is null)
            {
                throw new InvalidOperationException("No .app bundle found in Firefox Nightly DMG.");
            }

            string destApp = Path.Combine(CacheDir, Path.GetFileName(appBundle));
            if (Directory.Exists(destApp))
            {
                Directory.Delete(destApp, true);
            }

            await RunProcessAsync("cp", $"-R \"{appBundle}\" \"{destApp}\"");
        }
        finally
        {
            try
            {
                await RunProcessAsync("hdiutil", $"detach \"{mountPoint}\" -quiet");
            }
            catch
            {
                // Best effort detach
            }

            if (File.Exists(dmgPath))
            {
                File.Delete(dmgPath);
            }
        }
    }

    private static async Task DownloadAndExtractLinuxAsync(HttpClient client, string url)
    {
        string tarPath = Path.Combine(CacheDir, "firefox-nightly.tar.xz");
        await DownloadFileAsync(client, url, tarPath);

        try
        {
            string firefoxDir = Path.Combine(CacheDir, "firefox");
            if (Directory.Exists(firefoxDir))
            {
                Directory.Delete(firefoxDir, true);
            }

            await RunProcessAsync("tar", $"xJf \"{tarPath}\" -C \"{CacheDir}\"");
        }
        finally
        {
            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }
        }
    }

    private static async Task DownloadAndExtractWindowsAsync(HttpClient client, string url)
    {
        string installerPath = Path.Combine(CacheDir, "firefox-nightly-installer.exe");
        await DownloadFileAsync(client, url, installerPath);

        try
        {
            string installDir = Path.Combine(CacheDir, "firefox");
            if (Directory.Exists(installDir))
            {
                Directory.Delete(installDir, true);
            }

            Directory.CreateDirectory(installDir);
            await RunProcessAsync(installerPath, $"/S /D={installDir}");
        }
        finally
        {
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }
        }
    }

    private static async Task DownloadFileAsync(HttpClient client, string url, string destPath)
    {
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = new(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

        byte[] buffer = new byte[81920];
        long totalRead = 0;
        int bytesRead;
        int lastPercent = -1;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
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

    private static async Task RunProcessAsync(string fileName, string arguments)
    {
        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process '{fileName} {arguments}' exited with code {process.ExitCode}.\nstdout: {stdout}\nstderr: {stderr}");
        }
    }
}
