// <copyright file="TemporaryProfile.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// A browser profile directory created in the temporary directory for one launch. Each records
/// the process that owns it, so that one left behind when its owner dies can be recognized and
/// removed by a later launch.
/// </summary>
internal sealed class TemporaryProfile
{
    private const string DirectoryPrefix = "webdriverbidi-net-";
    private const string OwnerFileName = ".webdriverbidi-owner";
    private const int DeleteAttempts = 5;

    // Profiles created before owners were recorded are removed once untouched for this long.
    private static readonly TimeSpan UnownedProfileLifetime = TimeSpan.FromHours(1);

    private TemporaryProfile(string path)
    {
        this.Path = path;
    }

    /// <summary>
    /// Gets the full path of the profile directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Removes abandoned profiles, then creates a profile owned by the current process until
    /// <see cref="SetOwner"/> hands it to the browser.
    /// </summary>
    /// <param name="browserName">The name of the browser, used in the directory name.</param>
    /// <returns>The created profile.</returns>
    public static TemporaryProfile Create(string browserName)
    {
        RemoveAbandonedProfiles();
        string path = Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{DirectoryPrefix}{browserName}-data-{Guid.NewGuid()}")).FullName;
        TemporaryProfile profile = new(path);
        using Process currentProcess = Process.GetCurrentProcess();
        profile.SetOwner(currentProcess);
        return profile;
    }

    /// <summary>
    /// Records the process that owns the profile.
    /// </summary>
    /// <param name="process">The owning process.</param>
    [ExcludeFromCodeCoverage] // The owner can exit before its start time is read.
    public void SetOwner(Process process)
    {
        long startTimeTicks;
        try
        {
            startTimeTicks = process.StartTime.ToUniversalTime().Ticks;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
        {
            // The process has already exited; the profile stays with its previous owner.
            return;
        }

        File.WriteAllText(System.IO.Path.Combine(this.Path, OwnerFileName), string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", process.Id, startTimeTicks));
    }

    /// <summary>
    /// Deletes the profile, retrying while files are still held by the exiting browser. A profile
    /// that cannot be deleted is left for a later launch to remove.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync()
    {
        for (int attempt = 1; attempt <= DeleteAttempts; attempt++)
        {
            if (TryDeleteDirectory(this.Path))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt)).ConfigureAwait(false);
        }
    }

    private static void RemoveAbandonedProfiles()
    {
        string[] profileDirectories;
        try
        {
            profileDirectories = Directory.GetDirectories(System.IO.Path.GetTempPath(), $"{DirectoryPrefix}*-data-*");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return;
        }

        foreach (string profileDirectory in profileDirectories)
        {
            if (IsAbandoned(profileDirectory))
            {
                TryDeleteDirectory(profileDirectory);
            }
        }
    }

    private static bool IsAbandoned(string profileDirectory)
    {
        try
        {
            string ownerFile = System.IO.Path.Combine(profileDirectory, OwnerFileName);
            string[] owner = File.Exists(ownerFile) ? File.ReadAllText(ownerFile).Split('\n') : [];
            if (owner.Length == 2
                && int.TryParse(owner[0], NumberStyles.None, CultureInfo.InvariantCulture, out int processId)
                && long.TryParse(owner[1], NumberStyles.None, CultureInfo.InvariantCulture, out long startTimeTicks))
            {
                return !IsRunning(processId, startTimeTicks);
            }

            return DateTime.UtcNow - Directory.GetLastWriteTimeUtc(profileDirectory) > UnownedProfileLifetime;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return false;
        }
    }

    // A process ID can be reused, so the owner is only running if the start time matches too.
    private static bool IsRunning(int processId, long startTimeTicks)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            return IsStartedAt(process, startTimeTicks);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    [ExcludeFromCodeCoverage] // Whether another user's process's start time can be read depends on the platform.
    private static bool IsStartedAt(Process process, long startTimeTicks)
    {
        try
        {
            return Math.Abs(process.StartTime.ToUniversalTime().Ticks - startTimeTicks) < TimeSpan.TicksPerSecond;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception || ex is NotSupportedException)
        {
            // The owner's start time cannot be read (for instance, it belongs to another user), so keep the profile.
            return true;
        }
    }

    private static bool TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return false;
        }
    }
}
