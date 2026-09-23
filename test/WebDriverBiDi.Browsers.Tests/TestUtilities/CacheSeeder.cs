// <copyright file="CacheSeeder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

/// <summary>
/// Places installations directly into a test's cache directory, laid out as the locators install them.
/// </summary>
public static class CacheSeeder
{
    /// <summary>
    /// The name of the file marking a version directory as completely installed.
    /// </summary>
    public const string InstallationMarkerFileName = "INSTALLATION_COMPLETE";

    /// <summary>
    /// Creates a completely installed version.
    /// </summary>
    /// <param name="cache">The cache directory.</param>
    /// <param name="scope">The '/'-separated directory of the browser channel or driver, such as "firefox/stable".</param>
    /// <param name="version">The version.</param>
    /// <param name="relativeExecutablePath">The '/'-separated path of the executable relative to the version's directory.</param>
    /// <returns>The full path of the executable.</returns>
    public static string SeedInstallation(TemporaryDirectory cache, string scope, string version, string relativeExecutablePath)
    {
        string executablePath = SeedPartialInstallation(cache, scope, version, relativeExecutablePath);
        File.WriteAllText(Path.Combine(GetVersionDirectory(cache, scope, version), InstallationMarkerFileName), version);
        return executablePath;
    }

    /// <summary>
    /// Creates a version's files without the marker, as an interrupted installation leaves them.
    /// </summary>
    /// <param name="cache">The cache directory.</param>
    /// <param name="scope">The '/'-separated directory of the browser channel or driver, such as "firefox/stable".</param>
    /// <param name="version">The version.</param>
    /// <param name="relativeExecutablePath">The '/'-separated path of the executable relative to the version's directory.</param>
    /// <returns>The full path of the executable.</returns>
    public static string SeedPartialInstallation(TemporaryDirectory cache, string scope, string version, string relativeExecutablePath)
    {
        string executablePath = Path.Combine(GetVersionDirectory(cache, scope, version), TestDownloadOptions.ToLocalPath(relativeExecutablePath));
        Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
        File.WriteAllText(executablePath, "seeded");
        return executablePath;
    }

    /// <summary>
    /// Gets the directory of a version.
    /// </summary>
    /// <param name="cache">The cache directory.</param>
    /// <param name="scope">The '/'-separated directory of the browser channel or driver, such as "firefox/stable".</param>
    /// <param name="version">The version.</param>
    /// <returns>The full path of the version's directory.</returns>
    public static string GetVersionDirectory(TemporaryDirectory cache, string scope, string version)
    {
        return Path.Combine(cache.Path, TestDownloadOptions.ToLocalPath(scope), version);
    }
}
