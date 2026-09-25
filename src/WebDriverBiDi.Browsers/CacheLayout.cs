// <copyright file="CacheLayout.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// The layout of the cache's directories and records, which is marked in the cache so that a version of this
/// package that cannot read a cache refuses to use it, rather than misreading it or writing into it.
/// </summary>
internal static class CacheLayout
{
    /// <summary>
    /// The name of the file, in the cache directory, holding the layout version.
    /// </summary>
    internal const string MarkerFileName = ".layout-version";

    private const string CurrentVersion = "1";

    /// <summary>
    /// Checks that the cache uses the layout this package reads, marking a cache that has no marker yet.
    /// </summary>
    /// <param name="cacheDirectory">The cache directory.</param>
    /// <param name="markUnmarked">A value indicating whether to create the cache directory and mark it when it is not marked.</param>
    /// <exception cref="BrowserDownloadException">Thrown when the cache uses another layout.</exception>
    public static void EnsureReadable(string cacheDirectory, bool markUnmarked)
    {
        string markerPath = Path.Combine(cacheDirectory, MarkerFileName);
        if (File.Exists(markerPath))
        {
            string layout = File.ReadAllText(markerPath).Trim();
            if (layout != CurrentVersion)
            {
                throw new BrowserDownloadException($"The browser cache in {cacheDirectory} has layout version {layout}, which this version of WebDriverBiDi.Browsers cannot use. Update the package, or use another cache directory.");
            }

            return;
        }

        if (markUnmarked)
        {
            // Written whole and moved into place, so that no reader sees a partly written marker.
            Directory.CreateDirectory(cacheDirectory);
            string temporaryPath = $"{markerPath}.{Guid.NewGuid():N}.tmp";
            File.WriteAllText(temporaryPath, CurrentVersion);
            try
            {
                File.Move(temporaryPath, markerPath);
            }
            catch (IOException) when (File.Exists(markerPath))
            {
                // Another process marked the cache first.
                File.Delete(temporaryPath);
            }
        }
    }
}
