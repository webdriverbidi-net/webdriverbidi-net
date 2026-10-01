// <copyright file="ExecutableOverrideFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

[assembly: AssemblyFixture(typeof(Dramaturge.Browsers.TestUtilities.ExecutableOverrideFixture))]

namespace Dramaturge.Browsers.TestUtilities;

/// <summary>
/// Clears the environment variables that override browser and driver locations, and the cache
/// settings, for the whole
/// test run, restoring them afterwards. Left set (as they are on a machine that runs the
/// integration tests), they would silently replace the executables the tests arrange.
/// </summary>
public sealed class ExecutableOverrideFixture : IDisposable
{
    private static readonly string[] VariableNames =
    [
        "CHROME_EXECUTABLE",
        "CHROMEDRIVER_EXECUTABLE",
        "FIREFOX_EXECUTABLE",
        "GECKODRIVER_EXECUTABLE",
        "SAFARI_EXECUTABLE",
        "SAFARIDRIVER_EXECUTABLE",
        "EDGE_EXECUTABLE",
        "MSEDGEDRIVER_EXECUTABLE",
        "DRAMATURGE_BROWSERS_PATH",
        "DRAMATURGE_SKIP_DOWNLOAD",
    ];

    private readonly Dictionary<string, string?> originalValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutableOverrideFixture"/> class.
    /// </summary>
    public ExecutableOverrideFixture()
    {
        foreach (string name in VariableNames)
        {
            this.originalValues[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (KeyValuePair<string, string?> pair in this.originalValues)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}
