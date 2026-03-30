// <copyright file="FirefoxStableLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Runtime.InteropServices;

/// <summary>
/// Locator for Firefox binaries on the Stable channel.
/// </summary>
public class FirefoxStableLocator : FirefoxLocator
{
    private static readonly string FirefoxStableCacheDir = Path.Combine(
        BaseCacheDir,
        "firefox-stable");

    /// <summary>
    /// Gets the user-friendly name of the browser being located, used for logging and error messages.
    /// </summary>
    protected override string BrowserName => "Firefox Stable";

    /// <summary>
    /// Gets the cache directory specific to this browser, which is a subdirectory of the base cache directory.
    /// </summary>
    protected override string CacheDir => FirefoxStableCacheDir;

    /// <summary>
    /// Gets the file name used for the downloaded installer for the current platform.
    /// </summary>
    /// <returns>The file name for the downloaded installer.</returns>
    protected override string GetInstallerFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "firefox-stable.dmg";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "firefox-stable.tar.xz";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "firefox-stable-installer.exe";
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
    }

    /// <summary>
    /// Gets the direct download URL for the Firefox installer for the current platform.
    /// </summary>
    /// <returns>The direct download URL for the Firefox installer for the current platform.</returns>
    protected override Task<string> GetDownloadUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Task.FromResult("https://download.mozilla.org/?product=firefox-latest-ssl&os=osx&lang=en-US");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Task.FromResult("https://download.mozilla.org/?product=firefox-latest-ssl&os=linux64&lang=en-US");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Task.FromResult("https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=en-US");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
    }

    /// <summary>
    /// Gets the expected path to the Firefox executable after extraction, based on the platform.
    /// </summary>
    /// <returns>The expected path to the Firefox executable after extraction, based on the platform.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
    protected override string GetExpectedExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(this.CacheDir, "Firefox.app", "Contents", "MacOS", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(this.CacheDir, "firefox", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(this.CacheDir, "firefox", "firefox.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserName} download.");
    }
}
