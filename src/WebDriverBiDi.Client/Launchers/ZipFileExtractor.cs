// <copyright file="ZipFileExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.IO.Compression;

/// <summary>
/// File extractor for files distributed as zip files. This extractor uses the built-in ZipFile class to extract
/// the file from the downloaded zip file to the specified directory, and then deletes the zip file after extraction.
/// </summary>
public class ZipFileExtractor : FileExtractor
{
    /// <summary>
    /// Extracts the file from the downloaded zip file installer to the specified directory,
    /// and deletes the zip file after extraction.
    /// </summary>
    /// <param name="zipFilePath">Path to the zip file.</param>
    /// <param name="extractDirectory">Directory to extract the file to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task ExtractFileContentsAsync(string zipFilePath, string extractDirectory)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipFilePath, extractDirectory);
        }
        finally
        {
            // Clean up the zip file after extraction, regardless of success or failure.
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
        }

        return Task.CompletedTask;
    }
}
