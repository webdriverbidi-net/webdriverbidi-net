// <copyright file="TemporaryDirectory.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

/// <summary>
/// A directory created for one test and deleted when the test is done.
/// </summary>
public sealed class TemporaryDirectory : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
    /// </summary>
    public TemporaryDirectory()
    {
        this.Path = Directory.CreateTempSubdirectory("webdriverbidi-browsers-tests-").FullName;
    }

    /// <summary>
    /// Gets the full path of the directory.
    /// </summary>
    public string Path { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(this.Path, true);
        }
        catch (IOException)
        {
            // A file still held open by a killed process must not fail the test.
        }
    }
}
