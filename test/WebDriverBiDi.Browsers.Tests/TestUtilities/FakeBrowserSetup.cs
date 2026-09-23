// <copyright file="FakeBrowserSetup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// Configures the fake browser for one test through the environment variables it inherits, and
/// reads back what it recorded. Tests using this must not run in parallel with each other.
/// </summary>
public sealed class FakeBrowserSetup : IDisposable
{
    /// <summary>
    /// The path of the fake browser executable.
    /// </summary>
    public static readonly string ExecutablePath = Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "WebDriverBiDi.FakeBrowser.exe" : "WebDriverBiDi.FakeBrowser");

    private const string VariablePrefix = "WEBDRIVERBIDI_FAKE_BROWSER_";
    private static readonly string[] VariableNames = ["MODE", "LOG", "CHILD_PID_FILE", "EXIT_FILE"];

    private readonly TemporaryDirectory recordingDirectory = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeBrowserSetup"/> class.
    /// </summary>
    /// <param name="mode">The fake browser's mode, or <see langword="null"/> to report readiness normally.</param>
    /// <param name="startChild">A value indicating whether the fake browser starts a child process.</param>
    public FakeBrowserSetup(string? mode = null, bool startChild = false)
    {
        SetVariable("MODE", mode);
        SetVariable("LOG", this.LogFile);
        SetVariable("CHILD_PID_FILE", startChild ? this.ChildProcessIdFile : null);
        SetVariable("EXIT_FILE", this.ExitFile);
    }

    /// <summary>
    /// Gets the arguments of each launch of the fake browser, in order.
    /// </summary>
    public IReadOnlyList<string[]> Launches => File.Exists(this.LogFile)
        ? [.. File.ReadAllLines(this.LogFile).Select(line => JsonSerializer.Deserialize<string[]>(line)!)]
        : [];

    /// <summary>
    /// Gets a value indicating whether the fake browser exited because it was asked to.
    /// </summary>
    public bool ExitedGracefully => File.Exists(this.ExitFile);

    private string LogFile => Path.Combine(this.recordingDirectory.Path, "launches.log");

    private string ChildProcessIdFile => Path.Combine(this.recordingDirectory.Path, "child.pid");

    private string ExitFile => Path.Combine(this.recordingDirectory.Path, "exit");

    /// <summary>
    /// Waits for the fake browser to record a launch.
    /// </summary>
    /// <returns>The arguments of the most recent launch.</returns>
    /// <exception cref="TimeoutException">Thrown when no launch is recorded within 10 seconds.</exception>
    public async Task<string[]> WaitForLaunchAsync()
    {
        Stopwatch waitStopwatch = Stopwatch.StartNew();
        while (this.Launches.Count == 0)
        {
            if (waitStopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                throw new TimeoutException("The fake browser did not record a launch within 10 seconds.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        return this.Launches[^1];
    }

    /// <summary>
    /// Gets the child process the fake browser started.
    /// </summary>
    /// <returns>The child process.</returns>
    public Process GetChildProcess()
    {
        return Process.GetProcessById(int.Parse(File.ReadAllText(this.ChildProcessIdFile)));
    }

    /// <summary>
    /// Gets the value of an argument passed to the most recent launch, either as "name=value" or as
    /// "name" followed by the value.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value.</returns>
    public string GetLastLaunchArgument(string name)
    {
        string[] arguments = this.Launches[^1];
        int index = Array.IndexOf(arguments, name);
        return index >= 0 ? arguments[index + 1] : arguments.Single(argument => argument.StartsWith(name + "=", StringComparison.Ordinal))[(name.Length + 1)..];
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (string name in VariableNames)
        {
            SetVariable(name, null);
        }

        this.recordingDirectory.Dispose();
    }

    private static void SetVariable(string name, string? value)
    {
        Environment.SetEnvironmentVariable(VariablePrefix + name, value);
    }
}
