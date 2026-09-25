// <copyright file="EnvironmentTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Tool;

using System.Runtime.InteropServices;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.Tool.TestUtilities;

// Tests that set environment variables, which the process shares, and so run one at a time in this class alone.
public sealed class EnvironmentTests : IDisposable
{
    private const string EdgeVariableName = "EDGE_EXECUTABLE";
    private const string CacheVariableName = "WEBDRIVERBIDI_BROWSERS_PATH";
    private readonly Mirror mirror = new();
    private readonly TemporaryDirectory cache = new();
    private readonly TemporaryDirectory edge = new();

    [Fact]
    public async Task MsEdgeDriverIsTheVersionOfTheInstalledEdge()
    {
        Environment.SetEnvironmentVariable(EdgeVariableName, this.CreateEdge(Mirror.EdgeDriverVersion));

        ToolResult dryRun = await this.RunAsync("install", "--dry-run", "msedgedriver");
        ToolResult result = await this.RunAsync("install", "msedgedriver@beta");

        Assert.Equal([$"msedgedriver: msedgedriver@{Mirror.EdgeDriverVersion} (to download) {this.mirror.BuildUrl("msedgedriver", Mirror.EdgeDriverVersion)}"], dryRun.OutputLines);
        Assert.Equal([$"msedgedriver@beta: {Path.Combine(this.cache.Path, "drivers", "msedgedriver", Mirror.EdgeDriverVersion, "msedgedriver")}"], result.OutputLines);
    }

    [Fact]
    public async Task MsEdgeDriverIsNotInstalledWithoutEdge()
    {
        Environment.SetEnvironmentVariable(EdgeVariableName, Path.Combine(this.edge.Path, "missing"));

        ToolResult result = await this.RunAsync("install", "msedgedriver");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("could not be read", result.Error);
    }

    [Fact]
    public async Task CacheTheEnvironmentNamesIsUsedWithoutPath()
    {
        Environment.SetEnvironmentVariable(CacheVariableName, this.cache.Path);
        StringWriter output = new();

        int exitCode = await WebDriverBiDiTool.RunAsync(["list"], output, new StringWriter(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Equal($"No browsers or drivers are cached in {this.cache.Path}.", output.ToString().Trim());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(EdgeVariableName, null);
        Environment.SetEnvironmentVariable(CacheVariableName, null);
        this.mirror.Dispose();
        this.cache.Dispose();
        this.edge.Dispose();
    }

    // An Edge application bundle, whose version is read from its Info.plist on any platform.
    private string CreateEdge(string version)
    {
        string contents = Path.Combine(this.edge.Path, "Microsoft Edge.app", "Contents");
        Directory.CreateDirectory(Path.Combine(contents, "MacOS"));
        File.WriteAllText(Path.Combine(contents, "Info.plist"), $"<plist><dict><key>CFBundleShortVersionString</key><string>{version}</string></dict></plist>");
        string executable = Path.Combine(contents, "MacOS", "Microsoft Edge");
        File.WriteAllText(executable, string.Empty);
        return executable;
    }

    private async Task<ToolResult> RunAsync(params string[] args)
    {
        StringWriter output = new();
        StringWriter error = new();
        int exitCode = await WebDriverBiDiTool.RunAsync(
            args,
            output,
            error,
            (path, progress) => new BrowserDownloadOptions()
            {
                CacheDirectory = path ?? this.cache.Path,
                ManifestUrl = new Uri(this.mirror.ManifestPath),
                Platform = new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.X64),
            },
            TestContext.Current.CancellationToken);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }
}
