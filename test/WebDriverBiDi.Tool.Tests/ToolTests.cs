// <copyright file="ToolTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Tool;

using System.Runtime.InteropServices;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.Tool.TestUtilities;

// Each test runs the tool as the command line would, downloading from a mirror in a directory.
public sealed class ToolTests : IDisposable
{
    private readonly Mirror mirror = new();
    private readonly TemporaryDirectory cache = new();

    [Fact]
    public async Task InstallDownloadsEachTargetAndReportsWhereItIs()
    {
        ToolResult result = await this.RunAsync("install", "chrome", "chrome-headless-shell", "chromedriver@beta", "firefox@nightly", "geckodriver");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(
            [
                $"chrome: {this.CachePath("chrome", "stable", Mirror.ChromeVersion, "chrome-linux64", "chrome")}",
                $"chrome-headless-shell: {this.CachePath("chrome-headless-shell", "stable", Mirror.ChromeVersion, "chrome-headless-shell-linux64", "chrome-headless-shell")}",
                $"chromedriver@beta: {this.CachePath("drivers", "chromedriver", Mirror.ChromeBetaVersion, "chromedriver")}",
                $"firefox@nightly: {this.CachePath("firefox", "nightly", Mirror.FirefoxNightlyVersion, "firefox", "firefox")}",
                $"geckodriver: {this.CachePath("drivers", "geckodriver", Mirror.GeckoDriverVersion, "geckodriver")}",
            ],
            result.OutputLines);
        Assert.Contains($"Downloading Chrome Headless Shell Stable {Mirror.ChromeVersion}", result.Error);
        Assert.Contains(": 100%", result.Error);
    }

    [Fact]
    public async Task DryRunResolvesTargetsWithoutDownloadingThem()
    {
        await this.RunAsync("install", "firefox");

        ToolResult result = await this.RunAsync("install", "--dry-run", "chrome@131", $"chrome@{Mirror.ChromeBetaVersion}", "firefox", "geckodriver", "chromedriver@131", "chrome-headless-shell@131");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(
            [
                $"chrome@131: chrome@{Mirror.ChromeVersion} (to download) {this.mirror.BuildUrl("chrome", Mirror.ChromeVersion)}",
                $"chrome@{Mirror.ChromeBetaVersion}: chrome@{Mirror.ChromeBetaVersion} (to download) {this.mirror.BuildUrl("chrome", Mirror.ChromeBetaVersion)}",
                $"firefox: firefox@{Mirror.FirefoxVersion} (cached) {this.mirror.BuildUrl("firefox", Mirror.FirefoxVersion)}",
                $"geckodriver: geckodriver@{Mirror.GeckoDriverVersion} (to download) {this.mirror.BuildUrl("geckodriver", Mirror.GeckoDriverVersion)}",
                $"chromedriver@131: chromedriver@{Mirror.ChromeVersion} (to download) {this.mirror.BuildUrl("chromedriver", Mirror.ChromeVersion)}",
                $"chrome-headless-shell@131: chrome-headless-shell@{Mirror.ChromeVersion} (to download) {this.mirror.BuildUrl("chrome-headless-shell", Mirror.ChromeVersion)}",
            ],
            result.OutputLines);
        Assert.Equal(["firefox"], BrowserCache.List(this.Options(null)).Select(installation => installation.Name));
    }

    [Theory]
    [InlineData("edge", "Microsoft Edge is never downloaded")]
    [InlineData("safari", "Safari is part of macOS")]
    [InlineData("opera", "'opera' is not a browser or driver this tool installs")]
    [InlineData("chrome@", "name a channel or version after '@'")]
    [InlineData("firefox@canary", "'canary' is not a channel; firefox channels are stable, beta, dev, nightly, esr")]
    [InlineData("geckodriver@stable", "'stable' is not a channel; geckodriver has no channels")]
    [InlineData("chrome@99999999999", "too large to be a milestone")]
    [InlineData("firefox@134", "only Chrome and chromedriver are requested by milestone")]
    [InlineData("msedgedriver@131.0.2903.51", "msedgedriver is always the version of the installed Edge")]
    [InlineData("geckodriver@0.35.0", "geckodriver is always its latest release")]
    public async Task InvalidTargetStopsTheInstallationBeforeAnythingIsDownloaded(string target, string expectedError)
    {
        ToolResult result = await this.RunAsync("install", "chrome", target);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains(expectedError, result.Error);
        Assert.Empty(result.OutputLines);
        Assert.Empty(BrowserCache.List(this.Options(null)));
    }

    [Fact]
    public async Task TargetThatFailsDoesNotStopTheOthers()
    {
        ToolResult result = await this.RunAsync("install", "chrome@130.0.6723.58", "chrome@dev", "firefox@esr", "chromedriver");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("chrome@130.0.6723.58: ", result.Error);
        Assert.Contains("chrome@dev: ", result.Error);
        Assert.Contains("firefox@esr: ", result.Error);
        Assert.Equal([$"chromedriver: {this.CachePath("drivers", "chromedriver", Mirror.ChromeVersion, "chromedriver")}"], result.OutputLines);
    }

    [Fact]
    public async Task ListShowsEachInstallation()
    {
        await this.RunAsync("install", "chrome", $"chrome@{Mirror.ChromeBetaVersion}", "chromedriver");

        ToolResult result = await this.RunAsync("list");

        Assert.Equal(0, result.ExitCode);
        Assert.Collection(
            result.OutputLines,
            line => Assert.Matches($@"^chrome@{Mirror.ChromeVersion} +stable +0\.0 MB +\d{{4}}-\d\d-\d\d +{Escape(this.CachePath("chrome", "stable", Mirror.ChromeVersion))}$", line),
            line => Assert.Matches($@"^chrome@{Mirror.ChromeBetaVersion} +stable +5\.0 MB +- +{Escape(this.CachePath("chrome", "stable", Mirror.ChromeBetaVersion))}$", line),
            line => Assert.Matches($@"^chromedriver@{Mirror.ChromeVersion} +driver +0\.0 MB +\d{{4}}-\d\d-\d\d +{Escape(this.CachePath("drivers", "chromedriver", Mirror.ChromeVersion))}$", line));
    }

    [Fact]
    public async Task DownloadProgressIsReportedByQuarter()
    {
        ToolResult result = await this.RunAsync("install", "chrome@beta");

        string[] progress = [.. result.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Where(line => line.StartsWith("Downloading Chrome Beta", StringComparison.Ordinal))];
        Assert.Equal(["0%", "25%", "50%", "75%", "100%"], progress.Select(line => line[(line.LastIndexOf(' ') + 1)..]));
        Assert.All(progress, line => Assert.Contains($"{Mirror.ChromeBetaVersion} of 5.0 MB", line));
    }

    [Fact]
    public async Task RemovalThatFailsIsReported()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows() || Environment.UserName == "root", "A read-only directory prevents removal only on Unix, and not for root.");
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        await this.RunAsync("install", "geckodriver");
        string driverDirectory = this.CachePath("drivers", "geckodriver");
        File.SetUnixFileMode(driverDirectory, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        try
        {
            ToolResult result = await this.RunAsync("clear", "geckodriver");

            Assert.Equal(1, result.ExitCode);
            Assert.StartsWith($"geckodriver@{Mirror.GeckoDriverVersion}: ", result.Error);
        }
        finally
        {
            File.SetUnixFileMode(driverDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    [Fact]
    public async Task ListOfEmptyCacheSaysSoWithDefaultOptions()
    {
        StringWriter output = new();
        StringWriter error = new();

        int exitCode = await WebDriverBiDiTool.RunAsync(["list", "--path", this.cache.Path], output, error, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Equal($"No browsers or drivers are cached in {this.cache.Path}.", output.ToString().Trim());
    }

    [Fact]
    public async Task ClearRemovesWhatTheTargetsSelect()
    {
        await this.RunAsync("install", "chrome", "chrome@beta", "chromedriver", "chromedriver@beta", "firefox", "geckodriver");

        ToolResult dryRun = await this.RunAsync("clear", "--dry-run", "chrome@beta", "chromedriver@131", "geckodriver");
        ToolResult result = await this.RunAsync("clear", "chrome@beta", "chromedriver@131", "geckodriver", $"firefox@{Mirror.FirefoxVersion}");
        ToolResult nothing = await this.RunAsync("clear", "geckodriver");

        Assert.Equal(["Would remove chrome@132.0.6834.57 (beta)", "Would remove chromedriver@131.0.6778.204 (driver)", "Would remove geckodriver@0.36.0 (driver)"], dryRun.OutputLines);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(["Removed chrome@132.0.6834.57 (beta)", "Removed chromedriver@131.0.6778.204 (driver)", $"Removed firefox@{Mirror.FirefoxVersion} (stable)", "Removed geckodriver@0.36.0 (driver)"], result.OutputLines);
        Assert.Equal(["Nothing to remove."], nothing.OutputLines);
        Assert.Equal(["chrome@131.0.6778.204", "chromedriver@132.0.6834.57"], BrowserCache.List(this.Options(null)).Select(installation => installation.ToString()));
    }

    [Fact]
    public async Task ClearWithoutTargetsRemovesEverything()
    {
        await this.RunAsync("install", "chrome", "geckodriver");

        ToolResult result = await this.RunAsync("clear");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, result.OutputLines.Count);
        Assert.Empty(BrowserCache.List(this.Options(null)));
    }

    [Theory]
    [InlineData("chromedriver@stable", "drivers are not cached by channel")]
    [InlineData("edge", "Microsoft Edge is never downloaded")]
    public async Task ClearRejectsTargetsThatSelectNothingCached(string target, string expectedError)
    {
        ToolResult result = await this.RunAsync("clear", target);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains(expectedError, result.Error);
    }

    [Fact]
    public async Task CacheOfAnotherLayoutIsReported()
    {
        File.WriteAllText(Path.Combine(this.cache.Path, ".layout-version"), "2");

        ToolResult list = await this.RunAsync("list");
        ToolResult clear = await this.RunAsync("clear");
        ToolResult install = await this.RunAsync("install", "chrome");

        Assert.All([list, clear, install], result => Assert.Equal(1, result.ExitCode));
        Assert.All([list, clear, install], result => Assert.Contains("layout version 2", result.Error));
    }

    [Fact]
    public async Task HelpDescribesTheCommandsAndAnUnknownCommandFails()
    {
        ToolResult help = await this.RunAsync("--help");
        ToolResult unknown = await this.RunAsync("upgrade");

        Assert.Equal(0, help.ExitCode);
        Assert.Contains("webdriverbidi [command] [options]", help.Output);
        Assert.Contains("install", help.Output);
        Assert.Contains("--path", help.Output);
        Assert.Equal(1, unknown.ExitCode);
    }

    [Fact]
    public async Task PathOptionChoosesTheCache()
    {
        using TemporaryDirectory otherCache = new();

        ToolResult result = await this.RunAsync("install", "--path", otherCache.Path, "geckodriver");

        Assert.Equal([$"geckodriver: {Path.Combine(otherCache.Path, "drivers", "geckodriver", Mirror.GeckoDriverVersion, "geckodriver")}"], result.OutputLines);
        Assert.Empty(BrowserCache.List(this.Options(null)));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.mirror.Dispose();
        this.cache.Dispose();
    }

    private static string Escape(string text) => System.Text.RegularExpressions.Regex.Escape(text);

    private string CachePath(params string[] parts) => Path.Combine([this.cache.Path, .. parts]);

    private BrowserDownloadOptions Options(string? path, IProgress<BrowserDownloadProgress>? progress = null) => new()
    {
        CacheDirectory = path ?? this.cache.Path,
        ManifestUrl = new Uri(this.mirror.ManifestPath),
        Platform = new BrowserPlatform(OperatingSystemFamily.Linux, Architecture.X64),
        Progress = progress,
    };

    private async Task<ToolResult> RunAsync(params string[] args)
    {
        StringWriter output = new();
        StringWriter error = new();
        int exitCode = await WebDriverBiDiTool.RunAsync(args, output, error, this.Options, TestContext.Current.CancellationToken);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }
}
