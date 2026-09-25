// <copyright file="TestArchives.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Builds small archives in memory, standing in for downloaded browser and driver installers.
/// </summary>
public static class TestArchives
{
    /// <summary>
    /// Creates a zip archive containing a file at each of the given paths.
    /// </summary>
    /// <param name="entryPaths">The '/'-separated paths of the files to include.</param>
    /// <returns>The archive contents.</returns>
    public static byte[] Zip(params string[] entryPaths)
    {
        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string entryPath in entryPaths)
            {
                using StreamWriter writer = new(archive.CreateEntry(entryPath).Open());
                writer.Write(entryPath);
            }
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Creates a zip archive containing a file at each of the given paths, and an incompressible file of the given size.
    /// </summary>
    /// <param name="paddingSize">The size of the incompressible file.</param>
    /// <param name="entryPaths">The '/'-separated paths of the files to include.</param>
    /// <returns>The archive contents.</returns>
    public static byte[] LargeZip(int paddingSize, params string[] entryPaths)
    {
        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string entryPath in entryPaths)
            {
                using StreamWriter writer = new(archive.CreateEntry(entryPath).Open());
                writer.Write(entryPath);
            }

            using Stream padding = archive.CreateEntry("padding", CompressionLevel.NoCompression).Open();
            padding.Write(System.Security.Cryptography.RandomNumberGenerator.GetBytes(paddingSize));
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Creates a gzip-compressed tar archive containing a file at each of the given paths.
    /// </summary>
    /// <param name="entryPaths">The '/'-separated paths of the files to include.</param>
    /// <returns>The archive contents.</returns>
    public static byte[] TarGz(params string[] entryPaths)
    {
        using MemoryStream stream = new();
        using (GZipStream gzip = new(stream, CompressionLevel.Fastest, leaveOpen: true))
        using (TarWriter writer = new(gzip, TarEntryFormat.Pax, leaveOpen: true))
        {
            foreach (string entryPath in entryPaths)
            {
                PaxTarEntry entry = new(TarEntryType.RegularFile, entryPath)
                {
                    DataStream = new MemoryStream(Encoding.UTF8.GetBytes(entryPath)),
                };
                writer.WriteEntry(entry);
            }
        }

        return stream.ToArray();
    }
}
