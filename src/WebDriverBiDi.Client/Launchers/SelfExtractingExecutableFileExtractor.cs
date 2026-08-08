// <copyright file="SelfExtractingExecutableFileExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Browser extractor for browsers distributed as self-extracting executables on Windows. This extractor
/// runs the installer with silent and extraction options to extract the browser to the specified directory,
/// and then deletes the installer file.
/// </summary>
public class SelfExtractingExecutableFileExtractor : FileExtractor
{
    private readonly string extractedSourceDirectoryName;
    private readonly string extractedDestinationDirectoryName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfExtractingExecutableFileExtractor"/> class.
    /// </summary>
    /// <param name="extractedSourceDirectoryName">The directory within the archive contents containing the files to be extracted.</param>
    /// <param name="extractedDestinationDirectoryName">The directory to which the source files should be moved after extraction.</param>
    public SelfExtractingExecutableFileExtractor(string extractedSourceDirectoryName, string extractedDestinationDirectoryName)
        : base()
    {
        this.extractedSourceDirectoryName = extractedSourceDirectoryName;
        this.extractedDestinationDirectoryName = extractedDestinationDirectoryName;
    }

    /// <summary>
    /// Extracts the file from the downloaded self-extracting executable installer to the specified directory,
    /// and deletes the installer file.
    /// </summary>
    /// <param name="installerPath">Path to the self-extracting executable installer.</param>
    /// <param name="extractDirectory">Directory to extract the file to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExtractFileContentsAsync(string installerPath, string extractDirectory)
    {
        string temporaryExtractionPath = Path.Combine(extractDirectory, "extract");
        string destinationPath = Path.Combine(extractDirectory, this.extractedDestinationDirectoryName);
        try
        {
            if (Directory.Exists(temporaryExtractionPath))
            {
                Directory.Delete(temporaryExtractionPath, true);
            }

            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.CreateDirectory(temporaryExtractionPath);
            await this.RunProcessAsync(installerPath, $"/ExtractDir={temporaryExtractionPath}");
            string sourcePath = Path.Combine(temporaryExtractionPath, this.extractedSourceDirectoryName);
            Directory.Move(sourcePath, destinationPath);
        }
        finally
        {
            if (Directory.Exists(temporaryExtractionPath))
            {
                Directory.Delete(temporaryExtractionPath, true);
            }

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }
        }
    }
}
