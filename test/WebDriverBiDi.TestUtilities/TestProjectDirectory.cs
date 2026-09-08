// <copyright file="TestProjectDirectory.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.TestUtilities;

using System.Reflection;

/// <summary>
/// Resolves the directory of a sibling project that a test fixture builds, publishes, or launches
/// out of process.
/// </summary>
/// <remarks>
/// The path is stamped into the test assembly at build time by an <c>AssemblyMetadata</c> item in
/// the project file, rather than derived at run time by walking up from
/// <see cref="AppContext.BaseDirectory"/>. A relative walk hard-codes the shape of the output path
/// — <c>bin/&lt;configuration&gt;/&lt;target framework&gt;</c> — so anything that changes that shape
/// moves the target without the walk noticing: <c>--output</c>, <c>UseArtifactsOutput</c>, or a
/// runtime-identifier subdirectory. The failure then surfaces from whatever the fixture does next,
/// naming a directory that does not exist rather than the assumption that broke.
/// </remarks>
public static class TestProjectDirectory
{
    /// <summary>
    /// Gets the directory recorded under the given metadata key, verifying that it exists.
    /// </summary>
    /// <param name="assembly">The assembly carrying the metadata, normally the calling test assembly.</param>
    /// <param name="key">The <c>AssemblyMetadata</c> key the project file stamped the path under.</param>
    /// <returns>The full path of the project directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the assembly carries no such metadata.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the recorded directory does not exist.</exception>
    public static string Resolve(Assembly assembly, string key)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        string? directory = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(metadata => string.Equals(metadata.Key, key, StringComparison.Ordinal))?.Value;

        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException($"'{assembly.GetName().Name}' carries no AssemblyMetadata entry named '{key}'. The project file must stamp the sibling project's directory under that key.");
        }

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The '{key}' metadata on '{assembly.GetName().Name}' names '{directory}', which does not exist.");
        }

        return directory;
    }
}
