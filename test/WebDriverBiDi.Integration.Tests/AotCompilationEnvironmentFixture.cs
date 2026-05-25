// <copyright file="AotCompilationEnvironmentFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit.Sdk;

public class AotCompilationEnvironmentFixture : IAsyncLifetime
{
    // The AOT test application must be published as a native binary at test time.
    // We locate the project directory relative to the test assembly's base directory.
    private static readonly string SmokeTestProjectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WebDriverBiDi.AotTestApplication"));

    public string PublishDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        // Publish to a dedicated directory to avoid conflicts with regular builds.
        // Use -p:TreatWarningsAsErrors=true to convert static AOT warnings (IL2026,
        // IL3050, IL2090, etc.) emitted by the trim/AOT analyzers during native
        // compilation into a build failure. There should be zero warnings; a future
        // change that adds a reflection-based JsonSerializer.Serialize(object) call
        // will see this test fail immediately rather than shipping an AOT-broken
        // binary.
        this.PublishDir = Path.Combine(SmokeTestProjectDir, "bin", "AotTestPublish");

        int publishExit = await RunProcessAsync(
            "dotnet",
            $"publish \"{SmokeTestProjectDir}\" -c Release -o \"{this.PublishDir}\" -p:TreatWarningsAsErrors=true",
            workingDirectory: SmokeTestProjectDir,
            timeoutSeconds: 300);

        Assert.Equal(0, publishExit);

        string executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WebDriverBiDi.AotTestApplication.exe"
            : "WebDriverBiDi.AotTestApplication";
        this.ExecutablePath = Path.Combine(this.PublishDir, executableName);

        Assert.True(File.Exists(this.ExecutablePath));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    internal static async Task<int> RunProcessAsync(string fileName, string arguments, string workingDirectory, int timeoutSeconds)
    {
        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        // Read stdout/stderr concurrently to avoid deadlocks.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        bool exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stdout:\n{stdout}");
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stderr:\n{stderr}");
        }

        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            throw new XunitException($"Process '{fileName}' timed out after {timeoutSeconds}s.");
        }

        return process.ExitCode;
    }
}
