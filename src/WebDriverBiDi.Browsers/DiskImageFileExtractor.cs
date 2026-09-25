// <copyright file="DiskImageFileExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// File extractor for files distributed as disk images (DMG files) on macOS. This extractor
/// mounts the DMG, copies the contents to the specified extraction directory, and then unmounts
/// and deletes the DMG.
/// </summary>
[ExcludeFromCodeCoverage] // Runs only on macOS (hdiutil), and no CI job runs on macOS; the tests verify it locally.
public class DiskImageFileExtractor : FileExtractor
{
    /// <summary>
    /// Extracts the contents from the downloaded DMG installer to the specified directory,
    /// and deletes the DMG file.
    /// </summary>
    /// <param name="dmgPath">Path to the DMG file.</param>
    /// <param name="extractDirectory">Directory to extract the contents to.</param>
    /// <param name="cancellationToken">A token that cancels the extraction.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no .app bundle is found in the DMG.</exception>
    public override async Task ExtractFileContentsAsync(string dmgPath, string extractDirectory, CancellationToken cancellationToken = default)
    {
        string mountPoint = Path.Combine(extractDirectory, "dmg-mount");
        Directory.CreateDirectory(mountPoint);

        try
        {
            await this.RunProcessAsync("hdiutil", $"attach \"{dmgPath}\" -mountpoint \"{mountPoint}\" -nobrowse -quiet", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Find the .app bundle in the mounted DMG
            string? appBundle = Directory.GetDirectories(mountPoint, "*.app").FirstOrDefault();
            if (appBundle is null)
            {
                throw new BrowserDownloadException($"No .app bundle found in DMG downloaded to {dmgPath}.");
            }

            string destApp = Path.Combine(extractDirectory, Path.GetFileName(appBundle));
            if (Directory.Exists(destApp))
            {
                Directory.Delete(destApp, true);
            }

            await this.RunProcessAsync("cp", $"-R \"{appBundle}\" \"{destApp}\"", cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                await this.RunProcessAsync("hdiutil", $"detach \"{mountPoint}\" -quiet").ConfigureAwait(false);
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
}
