// <copyright file="AotCompilationEnvironmentFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using System.Runtime.InteropServices;

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

        RunProcessResult publishExit = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"publish \"{SmokeTestProjectDir}\" -c Release -o \"{this.PublishDir}\" -p:TreatWarningsAsErrors=true",
            workingDirectory: SmokeTestProjectDir,
            timeout: TimeSpan.FromMinutes(5));

        Assert.Equal(0, publishExit.ExitCode);

        string executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WebDriverBiDi.AotTestApplication.exe"
            : "WebDriverBiDi.AotTestApplication";
        this.ExecutablePath = Path.Combine(this.PublishDir, executableName);

        Assert.True(File.Exists(this.ExecutablePath));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
