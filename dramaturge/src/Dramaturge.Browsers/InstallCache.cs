// <copyright file="InstallCache.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The cached installations of one browser channel or one driver: a directory per installed
/// version, and a record of what each version request (such as "latest") last resolved to.
/// Callers hold the lock from <see cref="LockAsync"/> while reading or changing the cache.
/// </summary>
internal sealed class InstallCache
{
    /// <summary>
    /// The name of the file marking a version directory as completely installed.
    /// </summary>
    internal const string InstallationMarkerFileName = "INSTALLATION_COMPLETE";

    private const string ResolvedVersionsFileName = "resolved-versions.json";
    private const string LockFileName = ".lock";
    private const string TemporaryDirectoryPrefix = ".tmp-";
    private const string DiscardedDirectoryPrefix = ".discarded-";
    private const int DirectoryMoveAttempts = 5;

    private static readonly TimeSpan ResolvedVersionLifetime = TimeSpan.FromHours(24);

    private readonly string directory;
    private readonly BrowserDownloadOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallCache"/> class.
    /// </summary>
    /// <param name="directory">The directory holding this cache's installations.</param>
    /// <param name="options">The download options.</param>
    public InstallCache(string directory, BrowserDownloadOptions options)
    {
        this.directory = directory;
        this.options = options;
    }

    /// <summary>
    /// Waits for and acquires the lock on this cache.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A handle that releases the lock when disposed.</returns>
    public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        CacheLayout.EnsureReadable(this.options.CacheDirectory, markUnmarked: true);
        return new FileLock(Path.Combine(this.directory, LockFileName)).AcquireAsync(this.options.LockTimeout, cancellationToken);
    }

    /// <summary>
    /// Gets the path of an installed executable, if the version is completely installed.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="relativeExecutablePath">The path of the executable relative to the version's directory.</param>
    /// <param name="executablePath">The full path of the executable, if installed.</param>
    /// <returns><see langword="true"/> if the version is completely installed; otherwise, <see langword="false"/>.</returns>
    public bool TryGetInstalledExecutable(string version, string relativeExecutablePath, [NotNullWhen(true)] out string? executablePath)
    {
        string installDirectory = Path.Combine(this.directory, version);
        string path = Path.Combine(installDirectory, relativeExecutablePath);
        executablePath = File.Exists(Path.Combine(installDirectory, InstallationMarkerFileName)) && File.Exists(path) ? path : null;
        return executablePath is not null;
    }

    /// <summary>
    /// Gets the version a request last resolved to, if it has been resolved.
    /// </summary>
    /// <param name="request">The version request, such as "latest".</param>
    /// <param name="version">The version the request last resolved to.</param>
    /// <param name="isFresh">A value indicating whether the resolution is recent enough to use without checking again.</param>
    /// <returns><see langword="true"/> if the request has been resolved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetResolvedVersion(string request, [NotNullWhen(true)] out string? version, out bool isFresh)
    {
        version = null;
        isFresh = false;
        if (this.LoadResolvedVersions().TryGetValue(request, out ResolvedVersion? resolved))
        {
            version = resolved.Version;
            isFresh = this.options.TimeProvider.GetUtcNow() - resolved.ResolvedAt < ResolvedVersionLifetime;
        }

        return version is not null;
    }

    /// <summary>
    /// Records the version a request resolved to, as of now.
    /// </summary>
    /// <param name="request">The version request, such as "latest".</param>
    /// <param name="version">The version the request resolved to.</param>
    public void SaveResolvedVersion(string request, string version)
    {
        Dictionary<string, ResolvedVersion> resolvedVersions = this.LoadResolvedVersions();
        resolvedVersions[request] = new ResolvedVersion() { Version = version, ResolvedAt = this.options.TimeProvider.GetUtcNow() };
        this.SaveResolvedVersions(resolvedVersions);
    }

    /// <summary>
    /// Gets the completely installed versions, and when a request last resolved to each.
    /// </summary>
    /// <returns>The installations.</returns>
    public IReadOnlyList<InstalledVersion> GetInstallations()
    {
        Dictionary<string, ResolvedVersion> resolvedVersions = this.LoadResolvedVersions();
        return [.. Directory.GetDirectories(this.directory)
            .Where(subdirectory => !Path.GetFileName(subdirectory).StartsWith(".", StringComparison.Ordinal) && File.Exists(Path.Combine(subdirectory, InstallationMarkerFileName)))
            .Select(subdirectory =>
            {
                string version = Path.GetFileName(subdirectory);
                DateTimeOffset? lastResolved = resolvedVersions.Values.Where(resolved => resolved.Version == version).Select(resolved => (DateTimeOffset?)resolved.ResolvedAt).Max();
                return new InstalledVersion(version, subdirectory, lastResolved);
            })];
    }

    /// <summary>
    /// Removes an installed version, and the record of every request that resolved to it.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="IOException">Thrown when the version's directory cannot be moved away, as when a browser in it is running on Windows.</exception>
    public async Task RemoveAsync(string version)
    {
        string installDirectory = Path.Combine(this.directory, version);
        if (Directory.Exists(installDirectory))
        {
            // Moved away first, so that the version disappears at once, even if deleting its files fails.
            string discardedDirectory = Path.Combine(this.directory, $"{DiscardedDirectoryPrefix}{Guid.NewGuid():N}");
            await MoveDirectoryAsync(installDirectory, discardedDirectory).ConfigureAwait(false);
            TryDeleteDirectory(discardedDirectory);
        }

        Dictionary<string, ResolvedVersion> resolvedVersions = this.LoadResolvedVersions();
        List<string> requests = [.. resolvedVersions.Where(resolved => resolved.Value.Version == version).Select(resolved => resolved.Key)];
        if (requests.Count > 0)
        {
            foreach (string request in requests)
            {
                resolvedVersions.Remove(request);
            }

            this.SaveResolvedVersions(resolvedVersions);
        }
    }

    /// <summary>
    /// Installs a version: populates a temporary directory, marks it complete, then moves it into
    /// place, replacing any existing installation of the version. An installation that fails
    /// partway leaves nothing behind that could be mistaken for a complete one.
    /// </summary>
    /// <param name="version">The version to install.</param>
    /// <param name="relativeExecutablePath">The path of the executable relative to the version's directory.</param>
    /// <param name="populateAsync">Downloads and extracts the version into the directory it is given.</param>
    /// <returns>The full path of the installed executable.</returns>
    public async Task<string> InstallAsync(string version, string relativeExecutablePath, Func<string, Task> populateAsync)
    {
        this.RemoveAbandonedDirectories();
        string installDirectory = Path.Combine(this.directory, version);
        string temporaryDirectory = Path.Combine(this.directory, $"{TemporaryDirectoryPrefix}{Guid.NewGuid():N}");
        string discardedDirectory = Path.Combine(this.directory, $"{DiscardedDirectoryPrefix}{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            await populateAsync(temporaryDirectory).ConfigureAwait(false);
            File.WriteAllText(Path.Combine(temporaryDirectory, InstallationMarkerFileName), version);
            if (Directory.Exists(installDirectory))
            {
                await MoveDirectoryAsync(installDirectory, discardedDirectory).ConfigureAwait(false);
            }

            await MoveDirectoryAsync(temporaryDirectory, installDirectory).ConfigureAwait(false);
        }
        finally
        {
            TryDeleteDirectory(temporaryDirectory);
            TryDeleteDirectory(discardedDirectory);
        }

        return Path.Combine(installDirectory, relativeExecutablePath);
    }

    // Anti-malware scanners briefly hold files that were just written, so a move can fail transiently on Windows.
    [ExcludeFromCodeCoverage] // A move fails only while another process holds the files.
    private static async Task MoveDirectoryAsync(string source, string destination)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Move(source, destination);
                return;
            }
            catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) && attempt < DirectoryMoveAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt)).ConfigureAwait(false);
            }
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            // Retried by the next installation into this cache.
        }
    }

    // Callers hold the lock, so no other installation into this cache is in progress: temporary
    // and discarded directories, and version directories without a marker, are all left over
    // from installations that were interrupted.
    private void RemoveAbandonedDirectories()
    {
        foreach (string subdirectory in Directory.GetDirectories(this.directory))
        {
            string name = Path.GetFileName(subdirectory);
            if (name.StartsWith(TemporaryDirectoryPrefix, StringComparison.Ordinal)
                || name.StartsWith(DiscardedDirectoryPrefix, StringComparison.Ordinal)
                || !File.Exists(Path.Combine(subdirectory, InstallationMarkerFileName)))
            {
                TryDeleteDirectory(subdirectory);
            }
        }
    }

    private Dictionary<string, ResolvedVersion> LoadResolvedVersions()
    {
        string resolvedVersionsFile = Path.Combine(this.directory, ResolvedVersionsFileName);
        if (File.Exists(resolvedVersionsFile))
        {
            try
            {
                return JsonSerializer.Deserialize(File.ReadAllText(resolvedVersionsFile), InstallCacheJsonSerializerContext.Default.DictionaryStringResolvedVersion) ?? [];
            }
            catch (JsonException)
            {
                // An unreadable record only costs a network request to resolve the version again.
            }
        }

        return [];
    }

    private void SaveResolvedVersions(Dictionary<string, ResolvedVersion> resolvedVersions)
    {
        string resolvedVersionsFile = Path.Combine(this.directory, ResolvedVersionsFileName);
        string temporaryFile = $"{resolvedVersionsFile}.{Guid.NewGuid():N}.tmp";
        File.WriteAllText(temporaryFile, JsonSerializer.Serialize(resolvedVersions, InstallCacheJsonSerializerContext.Default.DictionaryStringResolvedVersion));
        if (File.Exists(resolvedVersionsFile))
        {
            File.Replace(temporaryFile, resolvedVersionsFile, null);
        }
        else
        {
            File.Move(temporaryFile, resolvedVersionsFile);
        }
    }

    /// <summary>
    /// The version a request resolved to, and when.
    /// </summary>
    internal sealed class ResolvedVersion
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time at which the request was resolved.
        /// </summary>
        [JsonPropertyName("resolvedAt")]
        public DateTimeOffset ResolvedAt { get; set; }
    }

    /// <summary>
    /// A completely installed version.
    /// </summary>
    /// <param name="Version">The version.</param>
    /// <param name="Directory">The directory it is installed in.</param>
    /// <param name="LastResolved">When a request last resolved to it, or <see langword="null"/> if none has.</param>
    internal sealed record InstalledVersion(string Version, string Directory, DateTimeOffset? LastResolved);
}

#pragma warning disable SA1402 // File may only contain a single type
/// <summary>
/// A source generation context for JSON serialization of <see cref="InstallCache"/> records.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, InstallCache.ResolvedVersion>))]
internal partial class InstallCacheJsonSerializerContext : JsonSerializerContext
{
}
