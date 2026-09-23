// <copyright file="FakeBrowserSetup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Configures the fake browser for one launch through the environment variables a launcher sets,
/// and reads back what it recorded.
/// </summary>
public sealed class FakeBrowserSetup : IDisposable
{
    /// <summary>
    /// The path of the fake browser executable.
    /// </summary>
    public static readonly string ExecutablePath = Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "WebDriverBiDi.FakeBrowser.exe" : "WebDriverBiDi.FakeBrowser");

    /// <summary>
    /// The variable whose value the fake browser records with each launch.
    /// </summary>
    public const string EchoVariableName = VariablePrefix + "ECHO";

    private const string VariablePrefix = "WEBDRIVERBIDI_FAKE_BROWSER_";

    private readonly TemporaryDirectory recordingDirectory = new();
    private readonly Dictionary<string, string?> variables = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeBrowserSetup"/> class.
    /// </summary>
    /// <param name="mode">The fake browser's mode, or <see langword="null"/> to report readiness normally.</param>
    /// <param name="startChild">A value indicating whether the fake browser starts a child process.</param>
    public FakeBrowserSetup(string? mode = null, bool startChild = false)
    {
        this.variables[VariablePrefix + "MODE"] = mode;
        this.variables[VariablePrefix + "LOG"] = this.LogFile;
        this.variables[VariablePrefix + "CHILD_PID_FILE"] = startChild ? this.ChildProcessIdFile : null;
        this.variables[VariablePrefix + "EXIT_FILE"] = this.ExitFile;
    }

    /// <summary>
    /// Gets each launch of the fake browser, in order.
    /// </summary>
    public IReadOnlyList<FakeBrowserLaunch> Launches => [.. this.ReadLog("arguments").Select(entry => new FakeBrowserLaunch(entry["arguments"]!.Deserialize<string[]>()!, (string?)entry["echo"]))];

    /// <summary>
    /// Gets the body of each new session request the fake browser, acting as a driver, received.
    /// </summary>
    public IReadOnlyList<JsonNode> SessionRequests => [.. this.ReadLog("sessionRequest").Select(entry => JsonNode.Parse((string)entry["sessionRequest"]!)!)];

    /// <summary>
    /// Gets a value indicating whether the fake browser exited because it was asked to.
    /// </summary>
    public bool ExitedGracefully => File.Exists(this.ExitFile);

    private string LogFile => Path.Combine(this.recordingDirectory.Path, "launches.log");

    private string ChildProcessIdFile => Path.Combine(this.recordingDirectory.Path, "child.pid");

    private string ExitFile => Path.Combine(this.recordingDirectory.Path, "exit");

    /// <summary>
    /// Configures a launcher to launch the fake browser with this setup.
    /// </summary>
    /// <param name="builder">The launcher builder.</param>
    /// <returns>The builder.</returns>
    public BrowserLauncherBuilder Apply(BrowserLauncherBuilder builder)
    {
        foreach (KeyValuePair<string, string?> variable in this.variables)
        {
            builder.WithEnvironmentVariable(variable.Key, variable.Value);
        }

        return builder;
    }

    /// <summary>
    /// Waits for the fake browser to record a launch.
    /// </summary>
    /// <returns>The most recent launch.</returns>
    /// <exception cref="TimeoutException">Thrown when no launch is recorded within 10 seconds.</exception>
    public async Task<FakeBrowserLaunch> WaitForLaunchAsync()
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
    /// Waits for the child process started by the fake browser to exit, if it has not already.
    /// </summary>
    /// <param name="timeout">How long to wait.</param>
    /// <returns><see langword="true"/> if the child has exited; otherwise, <see langword="false"/>.</returns>
    public bool WaitForChildExit(TimeSpan timeout)
    {
        Process child;
        try
        {
            child = this.GetChildProcess();
        }
        catch (ArgumentException)
        {
            // No process has the ID any longer.
            return true;
        }

        using (child)
        {
            return child.WaitForExit(timeout);
        }
    }

    /// <summary>
    /// Gets the value of an argument passed to the most recent launch, either as "name=value" or as
    /// "name" followed by the value.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value.</returns>
    public string GetLastLaunchArgument(string name)
    {
        string[] arguments = this.Launches[^1].Arguments;
        int index = Array.IndexOf(arguments, name);
        return index >= 0 ? arguments[index + 1] : arguments.Single(argument => argument.StartsWith(name + "=", StringComparison.Ordinal))[(name.Length + 1)..];
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.recordingDirectory.Dispose();
    }

    private IEnumerable<JsonObject> ReadLog(string entryKind)
    {
        return File.Exists(this.LogFile)
            ? File.ReadAllLines(this.LogFile).Select(line => JsonNode.Parse(line)!.AsObject()).Where(entry => entry.ContainsKey(entryKind))
            : [];
    }
}

/// <summary>
/// A launch of the fake browser.
/// </summary>
/// <param name="Arguments">The command line arguments.</param>
/// <param name="Echo">The value of <see cref="FakeBrowserSetup.EchoVariableName"/> in the launched process.</param>
public sealed record FakeBrowserLaunch(string[] Arguments, string? Echo);
