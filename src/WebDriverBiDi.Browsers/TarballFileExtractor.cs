// <copyright file="TarballFileExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// File extractor for files distributed as compressed tarballs. This extractor uses the 'tar' command-line tool
/// to extract the file from the downloaded tarball to the specified directory, and then deletes the tarball file.
/// </summary>
public class TarballFileExtractor : FileExtractor
{
    /// <summary>
    /// Extracts the file from the downloaded tar.xz installer to the specified directory,
    /// and deletes the tarball file.
    /// </summary>
    /// <param name="tarFilePath">Path to the tar.xz file.</param>
    /// <param name="extractDirectory">Directory to extract the file to.</param>
    /// <param name="cancellationToken">A token that cancels the extraction.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExtractFileContentsAsync(string tarFilePath, string extractDirectory, CancellationToken cancellationToken = default)
    {
        try
        {
            // tar detects the compression (gzip, bzip2, or xz) from the file's contents.
            await this.RunProcessAsync("tar", $"-xf \"{tarFilePath}\" -C \"{extractDirectory}\"", cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            File.Delete(tarFilePath);
        }
    }
}
