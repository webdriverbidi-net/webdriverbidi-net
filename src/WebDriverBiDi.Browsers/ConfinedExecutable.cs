// <copyright file="ConfinedExecutable.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Recognizes executables that run inside a Snap or Flatpak sandbox, which cannot read a
/// profile created in the system temporary directory.
/// </summary>
internal static class ConfinedExecutable
{
    // Larger files are binaries, not launcher scripts.
    private const int MaximumWrapperScriptLength = 64 * 1024;

    private static readonly string[] ConfinedPathMarkers = ["/snap/", "/flatpak/exports/bin/"];
    private static readonly string[] WrapperScriptMarkers = ["/snap/", "snap run", "flatpak run"];

    /// <summary>
    /// Gets a value indicating whether an executable runs confined by Snap or Flatpak: it is
    /// installed by one of them, links to such an installation, or is a script that launches one.
    /// </summary>
    /// <param name="executablePath">The path of the executable.</param>
    /// <returns><see langword="true"/> if the executable is confined; otherwise, <see langword="false"/>.</returns>
    public static bool IsConfined(string executablePath)
    {
        return IsConfinedPath(executablePath) || IsConfinedPath(ResolveLinkTarget(executablePath)) || IsWrapperScript(executablePath);
    }

    private static bool IsConfinedPath(string? path)
    {
        return path is not null && ConfinedPathMarkers.Any(marker => path.IndexOf(marker, StringComparison.Ordinal) >= 0);
    }

    private static string? ResolveLinkTarget(string path)
    {
#if NET
        try
        {
            return new FileInfo(path).ResolveLinkTarget(returnFinalTarget: true)?.FullName;
        }
        catch (IOException)
        {
            return null;
        }
#else
        return null;
#endif
    }

    private static bool IsWrapperScript(string path)
    {
        try
        {
            FileInfo file = new(path);
            if (!file.Exists || file.Length > MaximumWrapperScriptLength)
            {
                return false;
            }

            string contents = File.ReadAllText(path);
            return contents.StartsWith("#!", StringComparison.Ordinal) && WrapperScriptMarkers.Any(marker => contents.IndexOf(marker, StringComparison.Ordinal) >= 0);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return false;
        }
    }
}
