// <copyright file="FirefoxLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Base class for locating and downloading Firefox browsers for testing.
/// </summary>
public class FirefoxLocator : BrowserLocator
{
    private static readonly FirefoxNightlyLocator NightlyLocator = new();
    private static readonly FirefoxStableLocator StableLocator = new();

    /// <summary>
    /// Gets a locator for Firefox nightly channel binaries.
    /// </summary>
    public FirefoxLocator Nightly { get; } = NightlyLocator;

    /// <summary>
    /// Gets a locator for Firefox stable channel binaries.
     ///
    /// </summary>
    public FirefoxLocator Stable { get; } = StableLocator;

    /// <summary>
    /// Gets the name of the environment variable that can be used to specify a custom path to the Firefox executable.
    /// If this environment variable is set, the locator will return its value directly instead of downloading Firefox.
    /// </summary>
    protected override string EnvironmentVariableName { get; } = "FIREFOX_EXECUTABLE";

    /// <summary>
    /// Gets the user-friendly name of the browser being located, used for logging and error messages.
    /// </summary>
    protected override string BrowserName => "Firefox";

    /// <summary>
    /// Gets the cache directory specific to this browser, which is a subdirectory of the base cache directory.
    /// </summary>
    protected override string CacheDir => BaseCacheDir;

    /// <summary>
    /// Performs the download and extraction of the browser for the current platform.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task DownloadAndExtractAsync()
    {
        Directory.CreateDirectory(this.CacheDir);

        string url = await this.GetDownloadUrl();
        using HttpClient client = new();
        client.Timeout = TimeSpan.FromMinutes(10);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await this.DownloadAndExtractMacOsAsync(client, url);
            return;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await this.DownloadAndExtractLinuxAsync(client, url);
            return;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await this.DownloadAndExtractWindowsAsync(client, url);
            return;
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
     }

    /// <summary>
    /// Gets the direct download URL for the Firefox installer for the current platform.
    /// </summary>
    /// <returns>The direct download URL for the Firefox installer for the current platform.</returns>
    protected override Task<string> GetDownloadUrl()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the expected path to the Firefox executable after extraction, based on the platform.
    /// </summary>
    /// <returns>The expected path to the Firefox executable after extraction, based on the platform.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
    protected override string GetExpectedExecutablePath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the file name used for the downloaded installer for the current platform.
    /// </summary>
    /// <returns>The file name for the downloaded installer.</returns>
    protected override string GetInstallerFileName()
    {
        throw new NotImplementedException();
    }

    private async Task DownloadAndExtractMacOsAsync(HttpClient client, string url)
    {
        string dmgPath = Path.Combine(this.CacheDir, this.GetInstallerFileName());
        await this.DownloadFileAsync(client, url, dmgPath);

        string mountPoint = Path.Combine(this.CacheDir, "dmg-mount");
        Directory.CreateDirectory(mountPoint);

        try
        {
            await this.RunProcessAsync("hdiutil", $"attach \"{dmgPath}\" -mountpoint \"{mountPoint}\" -nobrowse -quiet");

            // Find the .app bundle in the mounted DMG
            string? appBundle = Directory.GetDirectories(mountPoint, "*.app").FirstOrDefault();
            if (appBundle is null)
            {
                throw new InvalidOperationException("No .app bundle found in Firefox Nightly DMG.");
            }

            string destApp = Path.Combine(this.CacheDir, Path.GetFileName(appBundle));
            if (Directory.Exists(destApp))
            {
                Directory.Delete(destApp, true);
            }

            await this.RunProcessAsync("cp", $"-R \"{appBundle}\" \"{destApp}\"");
        }
        finally
        {
            try
            {
                await this.RunProcessAsync("hdiutil", $"detach \"{mountPoint}\" -quiet");
                Directory.Delete(mountPoint);
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

    private async Task DownloadAndExtractLinuxAsync(HttpClient client, string url)
    {
        string tarPath = Path.Combine(this.CacheDir, this.GetInstallerFileName());
        await this.DownloadFileAsync(client, url, tarPath);

        try
        {
            string firefoxDir = Path.Combine(this.CacheDir, "firefox");
            if (Directory.Exists(firefoxDir))
            {
                Directory.Delete(firefoxDir, true);
            }

            await this.RunProcessAsync("tar", $"xJf \"{tarPath}\" -C \"{this.CacheDir}\"");
        }
        finally
        {
            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }
        }
    }

    private async Task DownloadAndExtractWindowsAsync(HttpClient client, string url)
    {
        string installerPath = Path.Combine(this.CacheDir, this.GetInstallerFileName());
        await this.DownloadFileAsync(client, url, installerPath);

        try
        {
            string installDir = Path.Combine(this.CacheDir, "firefox");
            if (Directory.Exists(installDir))
            {
                Directory.Delete(installDir, true);
            }

            Directory.CreateDirectory(installDir);
            await this.RunProcessAsync(installerPath, $"/S /D={installDir}");
        }
        finally
        {
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }
        }
    }

    private async Task RunProcessAsync(string fileName, string arguments, TimeSpan? timeout = null)
    {
        if (timeout == null)
        {
            timeout = TimeSpan.FromMinutes(10);
        }

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
        if (!process.WaitForExit(Convert.ToInt32(timeout.Value.TotalMilliseconds)))
        {
            process.Kill();
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process '{fileName} {arguments}' exited with code {process.ExitCode}.\nstdout: {stdout}\nstderr: {stderr}");
        }
    }
}
