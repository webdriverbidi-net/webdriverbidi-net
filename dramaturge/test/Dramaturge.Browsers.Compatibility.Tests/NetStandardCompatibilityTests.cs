// <copyright file="NetStandardCompatibilityTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers.Compatibility.Tests;

using Dramaturge.Browsers.Compatibility.Tests.TestUtilities;

public class NetStandardCompatibilityTests
{
    private static readonly string AppProjectDirectory = TestProjectDirectory.Resolve(typeof(NetStandardCompatibilityTests).Assembly, "NetStandardTestApplicationProjectDirectory");

    private static readonly string FakeBrowserPath = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "Dramaturge.FakeBrowser.exe" : "Dramaturge.FakeBrowser");

    // The unit tests run the net10.0 build, so the netstandard2.0-only code runs only here: the process tree kill,
    // the confined executable check, the cache layout marker, the version read's timeout, and the grid header.
    [Fact]
    public async Task NetStandardBuildRunsItsOwnCode()
    {
        string buildDirectory = Path.Combine(AppProjectDirectory, "bin", "NetStandardSmokeTestBuild");
        RunProcessResult build = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"build \"{AppProjectDirectory}\" -c Release -o \"{buildDirectory}\" -p:TreatWarningsAsErrors=true",
            workingDirectory: AppProjectDirectory,
            timeout: TimeSpan.FromMinutes(3),
            diagnosticReporter: (output) => TestContext.Current.SendDiagnosticMessage(output));
        Assert.Equal(0, build.ExitCode);
        Assert.True(File.Exists(FakeBrowserPath), $"Fake browser not found at {FakeBrowserPath}.");

        RunProcessResult run = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"\"{Path.Combine(buildDirectory, "Dramaturge.Browsers.NetStandardTestApplication.dll")}\" \"{FakeBrowserPath}\"",
            workingDirectory: buildDirectory,
            timeout: TimeSpan.FromSeconds(30),
            diagnosticReporter: (output) => TestContext.Current.SendDiagnosticMessage(output));

        Assert.Contains("Browsers: a browser was launched and killed through the netstandard2.0 build.", run.StandardOutputConsoleContent);
        Assert.Contains("Browsers: the installed browser's version was read and the new cache was marked.", run.StandardOutputConsoleContent);
        Assert.Contains("Browsers: a remote grid header was added.", run.StandardOutputConsoleContent);
        Assert.Contains("PASS:", run.StandardOutputConsoleContent);
        Assert.Equal(0, run.ExitCode);
    }
}
