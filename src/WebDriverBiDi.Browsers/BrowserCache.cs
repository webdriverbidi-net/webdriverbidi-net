// <copyright file="BrowserCache.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Lists and removes the browsers and drivers downloaded into the cache (see
/// <see cref="BrowserDownloadOptions.CacheDirectory"/>).
/// </summary>
public static class BrowserCache
{
    private const string DriversDirectoryName = "drivers";

    /// <summary>
    /// Lists the browser and driver versions completely installed in the cache.
    /// </summary>
    /// <param name="options">The options naming the cache directory, or <see langword="null"/> for the defaults.</param>
    /// <returns>The installations, ordered by name, channel, and version.</returns>
    /// <exception cref="BrowserDownloadException">Thrown when the cache was written by a version of this package whose layout this one cannot read.</exception>
    public static IReadOnlyList<CachedInstallation> List(BrowserDownloadOptions? options = null)
    {
        options ??= new BrowserDownloadOptions();
        string cacheDirectory = options.CacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            return [];
        }

        CacheLayout.EnsureReadable(cacheDirectory, markUnmarked: false);
        List<CachedInstallation> installations = [];
        foreach (string browserDirectory in GetEntryDirectories(cacheDirectory).Where(directory => Path.GetFileName(directory) != DriversDirectoryName))
        {
            foreach (string channelDirectory in GetEntryDirectories(browserDirectory))
            {
                AddInstallations(installations, options, channelDirectory, Path.GetFileName(browserDirectory), Path.GetFileName(channelDirectory));
            }
        }

        string driversDirectory = Path.Combine(cacheDirectory, DriversDirectoryName);
        foreach (string driverDirectory in GetEntryDirectories(driversDirectory))
        {
            AddInstallations(installations, options, driverDirectory, Path.GetFileName(driverDirectory), null);
        }

        return [.. installations
            .OrderBy(installation => installation.Name, StringComparer.Ordinal)
            .ThenBy(installation => installation.Channel, StringComparer.Ordinal)
            .ThenBy(installation => installation.Version, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Removes an installation listed by <see cref="List"/>, waiting for any download into the same browser channel,
    /// or of the same driver, to finish first.
    /// </summary>
    /// <param name="installation">The installation.</param>
    /// <param name="options">The options it was listed with, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the installation is not in the cache the options name.</exception>
    /// <exception cref="IOException">Thrown when the installation cannot be removed, as when a browser in it is running on Windows.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the cache stays locked longer than <see cref="BrowserDownloadOptions.LockTimeout"/>.</exception>
    public static async Task RemoveAsync(CachedInstallation installation, BrowserDownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new BrowserDownloadOptions();
        string installationsDirectory = Path.GetDirectoryName(installation.Directory)!;
        string expectedDirectory = installation.Channel is null
            ? Path.Combine(options.CacheDirectory, DriversDirectoryName, installation.Name)
            : Path.Combine(options.CacheDirectory, installation.Name, installation.Channel);
        if (!string.Equals(Path.GetFullPath(installationsDirectory), Path.GetFullPath(expectedDirectory), StringComparison.Ordinal))
        {
            throw new ArgumentException($"{installation} is not in the cache {options.CacheDirectory}.", nameof(installation));
        }

        InstallCache cache = new(installationsDirectory, options);
        using IDisposable lockHandle = await cache.LockAsync(cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(installation.Version).ConfigureAwait(false);
    }

    private static IEnumerable<string> GetEntryDirectories(string directory)
    {
        return Directory.Exists(directory)
            ? Directory.GetDirectories(directory).Where(subdirectory => !Path.GetFileName(subdirectory).StartsWith(".", StringComparison.Ordinal))
            : [];
    }

    private static void AddInstallations(List<CachedInstallation> installations, BrowserDownloadOptions options, string directory, string name, string? channel)
    {
        foreach (InstallCache.InstalledVersion installed in new InstallCache(directory, options).GetInstallations())
        {
            installations.Add(new CachedInstallation(name, channel, installed.Version, installed.Directory, GetSize(installed.Directory), installed.LastResolved));
        }
    }

    // Links, such as those within a macOS application bundle, are not followed, so that no file is counted twice.
    private static long GetSize(string directory)
    {
        long size = 0;
        foreach (string file in Directory.GetFiles(directory))
        {
            FileInfo info = new(file);
            if ((info.Attributes & FileAttributes.ReparsePoint) == 0)
            {
                size += info.Length;
            }
        }

        foreach (string subdirectory in Directory.GetDirectories(directory))
        {
            if ((new DirectoryInfo(subdirectory).Attributes & FileAttributes.ReparsePoint) == 0)
            {
                size += GetSize(subdirectory);
            }
        }

        return size;
    }
}
