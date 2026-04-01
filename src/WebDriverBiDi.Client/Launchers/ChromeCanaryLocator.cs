// <copyright file="ChromeCanaryLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Runtime.InteropServices;

/// <summary>
/// Locator for Chrome binaries on the Canary channel.
/// </summary>
public class ChromeCanaryLocator : ChromeLocator
{
    private static readonly string ChromeCanaryCacheDir = Path.Combine(
        BaseCacheDir,
        "chrome-canary");

    /// <summary>
    /// Gets the user-friendly name of the browser being located, used for logging and error messages.
    /// </summary>
    protected override string BrowserName => "Chrome Canary";

    /// <summary>
    /// Gets the cache directory specific to this browser, which is a subdirectory of the base cache directory.
    /// </summary>
    protected override string CacheDir => ChromeCanaryCacheDir;

    /// <summary>
    /// Gets the file name used for the downloaded installer for the current platform.
    /// </summary>
    /// <returns>The file name for the downloaded installer.</returns>
    protected override string GetInstallerFileName()
    {
        return "chrome-canary.zip";
    }
}
