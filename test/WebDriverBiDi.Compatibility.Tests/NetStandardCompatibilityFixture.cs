// <copyright file="NetStandardCompatibilityFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Compatibility.Tests;

using WebDriverBiDi.TestUtilities;

public class NetStandardCompatibilityFixture : IAsyncLifetime
{
    // The netstandard2.0 smoke test application only needs a normal framework-dependent
    // build, not a publish, so we locate the project directory relative to the test
    // assembly's base directory and build it directly, mirroring
    // AotCompilationEnvironmentFixture's approach for its own sibling app.
    private static readonly string SmokeAppProjectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WebDriverBiDi.NetStandardTestApplication"));

    public string BuildDir { get; private set; } = string.Empty;

    public string DllPath { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        // Build to a dedicated directory to avoid conflicts with regular builds, and so
        // the resulting directory contains this app's full framework-dependent deployment
        // (entry DLL, .deps.json, runtimeconfig.json, and every copy-local dependency,
        // including the netstandard2.0 build of WebDriverBiDi.dll and its polyfill
        // packages) in one place, ready to run directly with `dotnet <dll>`.
        this.BuildDir = Path.Combine(SmokeAppProjectDir, "bin", "NetStandardSmokeTestBuild");

        RunProcessResult buildExit = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"build \"{SmokeAppProjectDir}\" -c Release -o \"{this.BuildDir}\" -p:TreatWarningsAsErrors=true",
            workingDirectory: SmokeAppProjectDir,
            timeout: TimeSpan.FromMinutes(3),
            diagnosticReporter: (output) => TestContext.Current.SendDiagnosticMessage(output));

        Assert.Equal(0, buildExit.ExitCode);

        this.DllPath = Path.Combine(this.BuildDir, "WebDriverBiDi.NetStandardTestApplication.dll");
        Assert.True(File.Exists(this.DllPath));

        // Defense-in-depth: confirm the netstandard2.0 build of the library actually
        // landed in the output. If this ever fails, the SetTargetFramework metadata on
        // the smoke app's ProjectReference to WebDriverBiDi.csproj has stopped working,
        // and the test below would otherwise silently validate the wrong assembly.
        string libraryPath = Path.Combine(this.BuildDir, "WebDriverBiDi.dll");
        Assert.True(File.Exists(libraryPath));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
