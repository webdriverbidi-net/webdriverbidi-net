// <copyright file="LauncherEnvironment.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Reads the environment variables that configure browser location and download. Every read goes
/// through here, so that an empty value is treated as unset everywhere.
/// </summary>
internal static class LauncherEnvironment
{
    /// <summary>
    /// The variable naming the directory in which browsers and drivers are cached.
    /// </summary>
    internal const string BrowsersPathVariableName = "DRAMATURGE_BROWSERS_PATH";

    /// <summary>
    /// The variable that, set to "1" or "true", restricts locating browsers and drivers to the cache.
    /// </summary>
    internal const string SkipDownloadVariableName = "DRAMATURGE_SKIP_DOWNLOAD";

    /// <summary>
    /// The variable naming the download manifest, by URL or file path.
    /// </summary>
    internal const string DownloadManifestVariableName = "DRAMATURGE_DOWNLOAD_MANIFEST";

    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The value, or <see langword="null"/> if the variable is unset, empty, or whitespace.</returns>
    public static string? GetVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Gets a value indicating whether an environment variable is set to "1" or "true".
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns><see langword="true"/> if the variable is set to "1" or "true" (in any case); otherwise, <see langword="false"/>.</returns>
    public static bool IsEnabled(string name)
    {
        string? value = GetVariable(name)?.Trim();
        return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
