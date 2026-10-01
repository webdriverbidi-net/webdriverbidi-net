// <copyright file="CompatibleVersion.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Chooses the driver version for a browser version that has no driver of its own, as a respin of Chrome for one
/// platform has none: the newest of the same build, and then of the same major version.
/// </summary>
internal static class CompatibleVersion
{
    /// <summary>
    /// Finds the version, among those available, that best matches a requested one.
    /// </summary>
    /// <param name="requested">The requested version.</param>
    /// <param name="available">The available versions.</param>
    /// <returns>The requested version if it is available, else the closest compatible one, or <see langword="null"/> if none is.</returns>
    public static string? FindClosest(string requested, IEnumerable<string> available)
    {
        List<string> versions = [.. available];
        if (versions.FirstOrDefault(version => version.Equals(requested, StringComparison.OrdinalIgnoreCase)) is string exact)
        {
            return exact;
        }

        if (!System.Version.TryParse(requested, out Version? parsedRequest))
        {
            return null;
        }

        List<(string Version, Version Parsed)> sameMajor = [.. versions
            .Select(version => (Version: version, Parsed: System.Version.TryParse(version, out Version? parsed) ? parsed : null))
            .Where(candidate => candidate.Parsed?.Major == parsedRequest.Major)
            .Select(candidate => (candidate.Version, candidate.Parsed!))
            .OrderByDescending(candidate => candidate.Item2)];
        return sameMajor.Where(candidate => candidate.Parsed.Minor == parsedRequest.Minor && candidate.Parsed.Build == parsedRequest.Build)
            .Concat(sameMajor)
            .Select(candidate => candidate.Version)
            .FirstOrDefault();
    }
}
