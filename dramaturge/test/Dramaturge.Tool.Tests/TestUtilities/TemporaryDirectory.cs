// <copyright file="TemporaryDirectory.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool.TestUtilities;

/// <summary>
/// A directory that is deleted when disposed.
/// </summary>
public sealed class TemporaryDirectory : IDisposable
{
    /// <summary>
    /// Gets the directory's path.
    /// </summary>
    public string Path { get; } = Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dramaturge-tool-tests-{Guid.NewGuid():N}")).FullName;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(this.Path, true);
        }
        catch (IOException)
        {
            // Left for the operating system's temporary file cleanup.
        }
    }
}
