// <copyright file="TarballBrowserExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Browser extractor for browsers distributed as tar.xz files on Linux. This extractor uses the 'tar' command-line tool
/// to extract the browser from the downloaded tarball to the specified directory, and then deletes the tarball file.
/// </summary>
public class TarballBrowserExtractor : BrowserExtractor
{
    /// <summary>
    /// Extracts the browser from the downloaded tar.xz installer to the specified directory,
    /// and deletes the tarball file.
    /// </summary>
    /// <param name="tarFilePath">Path to the tar.xz file.</param>
    /// <param name="extractDirectory">Directory to extract the browser to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExtractBrowserAsync(string tarFilePath, string extractDirectory)
    {
        try
        {
            if (Directory.Exists(extractDirectory))
            {
                Directory.Delete(extractDirectory, true);
            }

            await this.RunProcessAsync("tar", $"xJf \"{tarFilePath}\" -C \"{extractDirectory}\"");
        }
        finally
        {
            if (File.Exists(tarFilePath))
            {
                File.Delete(tarFilePath);
            }
        }
    }
}
