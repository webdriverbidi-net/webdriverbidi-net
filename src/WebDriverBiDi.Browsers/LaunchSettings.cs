// <copyright file="LaunchSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;

/// <summary>
/// The settings a <see cref="BrowserLauncherBuilder"/> collects for the launched browser process.
/// </summary>
internal sealed class LaunchSettings
{
    /// <summary>
    /// Gets the arguments added to the browser's command line.
    /// </summary>
    public List<string> Arguments { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether every default argument is omitted.
    /// </summary>
    public bool OmitAllDefaultArguments { get; set; }

    /// <summary>
    /// Gets the default arguments omitted, each matching an argument either exactly or as its "name=" prefix.
    /// </summary>
    public List<string> OmittedDefaultArguments { get; } = [];

    /// <summary>
    /// Gets the environment variables set for the launched process; a <see langword="null"/> value removes an inherited variable.
    /// </summary>
    public Dictionary<string, string?> EnvironmentVariables { get; } = [];

    /// <summary>
    /// Gets or sets the user-owned profile directory, or <see langword="null"/> to use a temporary one.
    /// </summary>
    public string? UserDataDirectory { get; set; }

    /// <summary>
    /// Gets the Firefox preferences, which override the launcher's defaults.
    /// </summary>
    public Dictionary<string, object> FirefoxPreferences { get; } = [];

    /// <summary>
    /// Gets a value indicating whether any setting changes the browser's command line or profile.
    /// </summary>
    public bool ChangesBrowserConfiguration => this.Arguments.Count > 0 || this.OmitAllDefaultArguments || this.OmittedDefaultArguments.Count > 0 || this.UserDataDirectory is not null || this.FirefoxPreferences.Count > 0;

    /// <summary>
    /// Creates a copy of these options.
    /// </summary>
    /// <returns>The copy.</returns>
    public LaunchSettings Copy()
    {
        LaunchSettings copy = new() { OmitAllDefaultArguments = this.OmitAllDefaultArguments, UserDataDirectory = this.UserDataDirectory };
        copy.Arguments.AddRange(this.Arguments);
        copy.OmittedDefaultArguments.AddRange(this.OmittedDefaultArguments);
        foreach (KeyValuePair<string, string?> variable in this.EnvironmentVariables)
        {
            copy.EnvironmentVariables[variable.Key] = variable.Value;
        }

        foreach (KeyValuePair<string, object> preference in this.FirefoxPreferences)
        {
            copy.FirefoxPreferences[preference.Key] = preference.Value;
        }

        return copy;
    }

    /// <summary>
    /// Removes the omitted arguments from a launcher's default arguments.
    /// </summary>
    /// <param name="defaultArguments">The launcher's default arguments.</param>
    /// <returns>The default arguments that are not omitted.</returns>
    public IEnumerable<string> FilterDefaultArguments(IEnumerable<string> defaultArguments)
    {
        return this.OmitAllDefaultArguments
            ? []
            : defaultArguments.Where(argument => !this.OmittedDefaultArguments.Any(omitted => argument == omitted || argument.StartsWith(omitted + "=", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Applies the environment variables to a process about to be started.
    /// </summary>
    /// <param name="startInfo">The start information of the process.</param>
    public void ApplyEnvironmentVariables(ProcessStartInfo startInfo)
    {
        foreach (KeyValuePair<string, string?> variable in this.EnvironmentVariables)
        {
            if (variable.Value is null)
            {
                startInfo.Environment.Remove(variable.Key);
            }
            else
            {
                startInfo.Environment[variable.Key] = variable.Value;
            }
        }
    }
}
