// <copyright file="InstalledBrowserVersion.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/// <summary>
/// Reads the version of a browser installed on this machine, for a driver that must match it.
/// </summary>
internal static class InstalledBrowserVersion
{
    private static readonly TimeSpan VersionOutputTimeout = TimeSpan.FromSeconds(10);
    private static readonly Regex VersionPattern = new(@"\d+(\.\d+){3}");
    private static readonly Regex BundleVersionPattern = new(@"<key>CFBundleShortVersionString</key>\s*<string>([^<]*)</string>");

    /// <summary>
    /// Reads the version of the browser at a path: from its macOS application bundle, from its file version on
    /// Windows, or otherwise from what it writes when run with "--version".
    /// </summary>
    /// <param name="executablePath">The path of the browser executable.</param>
    /// <param name="timeProvider">The time provider that times the wait for "--version" output.</param>
    /// <param name="cancellationToken">A token that cancels reading the version.</param>
    /// <returns>The version, such as "130.0.2849.80", or <see langword="null"/> if it cannot be read.</returns>
    public static async Task<string?> ReadAsync(string executablePath, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (!File.Exists(executablePath))
        {
            return null;
        }

        // An executable in a macOS application bundle is in Contents/MacOS, beside the bundle's Info.plist.
        string bundleInfoPath = Path.Combine(Path.GetDirectoryName(executablePath)!, "..", "Info.plist");
        if (File.Exists(bundleInfoPath))
        {
            Match match = BundleVersionPattern.Match(File.ReadAllText(bundleInfoPath));
            return match.Success ? FindVersion(match.Groups[1].Value) : null;
        }

        // A Chromium browser on Windows writes nothing for "--version", but its file version is the browser's.
        return ReadWindowsFileVersion(executablePath) ?? await ReadVersionOutputAsync(executablePath, timeProvider, cancellationToken).ConfigureAwait(false);
    }

    private static string? FindVersion(string text)
    {
        Match match = VersionPattern.Match(text);
        return match.Success ? match.Value : null;
    }

    [ExcludeFromCodeCoverage] // Takes only the branch for the operating system it runs on.
    private static string? ReadWindowsFileVersion(string executablePath)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? FindVersion(FileVersionInfo.GetVersionInfo(executablePath).FileVersion ?? string.Empty) : null;
    }

    private static async Task<string?> ReadVersionOutputAsync(string executablePath, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo(executablePath, "--version")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return null;
        }

        // Drained concurrently: a process blocked writing to a full stderr pipe never closes stdout.
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
#if NET8_0_OR_GREATER
        using CancellationTokenSource timeoutSource = new(VersionOutputTimeout, timeProvider);
#else
        using CancellationTokenSource timeoutSource = timeProvider.CreateCancellationTokenSource(VersionOutputTimeout);
#endif
        using CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        Task outputRead = Task.WhenAll(outputTask, errorTask);
        if (await Task.WhenAny(outputRead, Task.Delay(Timeout.Infinite, waitSource.Token)).ConfigureAwait(false) != outputRead)
        {
            ProcessTermination.KillTree(process);
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }

        waitSource.Cancel();
        return FindVersion(await outputTask.ConfigureAwait(false));
    }
}
