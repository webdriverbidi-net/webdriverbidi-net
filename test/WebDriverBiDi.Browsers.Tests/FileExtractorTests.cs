// <copyright file="FileExtractorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

public class FileExtractorTests
{
    // Started without remote debugging arguments, the fake browser prints nothing and never exits.
    private static readonly string FakeBrowserPath = Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "WebDriverBiDi.FakeBrowser.exe" : "WebDriverBiDi.FakeBrowser");

    [Fact]
    public async Task RunProcessKillsProcessThatExceedsTimeout()
    {
        ProcessRunningExtractor extractor = new();

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(
            () => extractor.RunAsync(FakeBrowserPath, string.Empty, TimeSpan.FromMilliseconds(500)));

        Assert.Contains("did not complete within 0.5 seconds", exception.Message);
    }

    [Fact]
    public async Task RunProcessReportsNonZeroExitCodeWithOutput()
    {
        ProcessRunningExtractor extractor = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => extractor.RunAsync("tar", "--not-a-real-option", TimeSpan.FromSeconds(30)));

        Assert.Contains("exited with code", exception.Message);
        Assert.Contains("stderr:", exception.Message);
    }

    private sealed class ProcessRunningExtractor : FileExtractor
    {
        public override Task ExtractFileContentsAsync(string installerPath, string extractDir) => throw new NotSupportedException();

        public Task RunAsync(string fileName, string arguments, TimeSpan timeout) => this.RunProcessAsync(fileName, arguments, timeout);
    }
}
