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
    /// The variable naming the browser <see cref="BrowserLauncher.ConfigureFromEnvironment"/> launches.
    /// </summary>
    internal const string BrowserVariableName = "DRAMATURGE_BROWSER";

    /// <summary>
    /// The variable naming the release channel <see cref="BrowserLauncher.ConfigureFromEnvironment"/> launches.
    /// </summary>
    internal const string ChannelVariableName = "DRAMATURGE_CHANNEL";

    /// <summary>
    /// The variable that, set to "1" or "true", makes <see cref="BrowserLauncher.ConfigureFromEnvironment"/> show the browser.
    /// </summary>
    internal const string HeadedVariableName = "DRAMATURGE_HEADED";

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
    /// Gets the value of an environment variable naming a member of an enumeration, in any case.
    /// </summary>
    /// <typeparam name="T">The enumeration.</typeparam>
    /// <param name="name">The variable name.</param>
    /// <param name="defaultValue">The value if the variable is unset.</param>
    /// <returns>The member the variable names, or <paramref name="defaultValue"/> if it is unset.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when the variable names no member.</exception>
    public static T GetEnumVariable<T>(string name, T defaultValue)
        where T : struct, Enum
    {
        string? value = GetVariable(name)?.Trim();
        if (value is null)
        {
            return defaultValue;
        }

        // Names only: Enum.TryParse also accepts numbers.
        string? member = Array.Find(Enum.GetNames(typeof(T)), candidate => string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase));
        if (member is null)
        {
            throw new BrowserLauncherConfigurationException($"Environment variable {name} is '{value}'; it must be one of {string.Join(", ", Enum.GetNames(typeof(T)))}.");
        }

        return (T)Enum.Parse(typeof(T), member);
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
