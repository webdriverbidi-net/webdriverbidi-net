// <copyright file="SelfExtractingExecutableBrowserExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Browser extractor for browsers distributed as self-extracting executables on Windows. This extractor
/// runs the installer with silent and extraction options to extract the browser to the specified directory,
/// and then deletes the installer file.
/// </summary>
public class SelfExtractingExecutableBrowserExtractor : BrowserExtractor
{
    /// <summary>
    /// Extracts the browser from the downloaded self-extracting executable installer to the specified directory,
    /// and deletes the installer file.
    /// </summary>
    /// <param name="installerPath">Path to the self-extracting executable installer.</param>
    /// <param name="extractDirectory">Directory to extract the browser to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExtractBrowserAsync(string installerPath, string extractDirectory)
    {
        try
        {
            if (Directory.Exists(extractDirectory))
            {
                Directory.Delete(extractDirectory, true);
            }

            Directory.CreateDirectory(extractDirectory);
            await this.RunProcessAsync(installerPath, $"/S /D={extractDirectory}");
        }
        finally
        {
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }
        }
    }
}
