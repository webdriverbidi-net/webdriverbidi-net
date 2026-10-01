// <copyright file="FileExtractorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics;
using Dramaturge.Browsers.TestUtilities;
using WebDriverBiDi;

public class FileExtractorTests
{
    // Started without remote debugging arguments, the fake browser prints nothing and never exits.
    private static readonly string FakeBrowserPath = Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "Dramaturge.FakeBrowser.exe" : "Dramaturge.FakeBrowser");

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

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(
            () => extractor.RunAsync("tar", "--not-a-real-option", TimeSpan.FromSeconds(30)));

        Assert.Contains("exited with code", exception.Message);
        Assert.Contains("stderr:", exception.Message);
    }

    // The fake browser acts as the installer, writing core/firefox.exe.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelfExtractingInstallerIsExtractedIntoItsDestination(bool leftoversExist)
    {
        using TemporaryDirectory installerDirectory = new();
        using TemporaryDirectory extractDirectory = new();
        string installerPath = CopyFakeBrowser(installerDirectory);
        if (leftoversExist)
        {
            Directory.CreateDirectory(Path.Combine(extractDirectory.Path, "extract", "leftover"));
            Directory.CreateDirectory(Path.Combine(extractDirectory.Path, "firefox", "leftover"));
        }

        await new SelfExtractingExecutableFileExtractor("core", "firefox").ExtractFileContentsAsync(installerPath, extractDirectory.Path, TestContext.Current.CancellationToken);

        Assert.True(File.Exists(Path.Combine(extractDirectory.Path, "firefox", "firefox.exe")));
        Assert.False(Directory.Exists(Path.Combine(extractDirectory.Path, "firefox", "leftover")));
        Assert.False(Directory.Exists(Path.Combine(extractDirectory.Path, "extract")));
        Assert.False(File.Exists(installerPath));
    }

    [Fact]
    public async Task SelfExtractingInstallerWithoutExpectedDirectoryFailsAndIsCleanedUp()
    {
        using TemporaryDirectory installerDirectory = new();
        using TemporaryDirectory extractDirectory = new();
        string installerPath = CopyFakeBrowser(installerDirectory);

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => new SelfExtractingExecutableFileExtractor("missing", "firefox").ExtractFileContentsAsync(installerPath, extractDirectory.Path, TestContext.Current.CancellationToken));

        Assert.False(Directory.Exists(Path.Combine(extractDirectory.Path, "extract")));
        Assert.False(File.Exists(installerPath));
    }

    [Fact]
    public async Task DiskImageAppBundleIsCopiedOutReplacingAnEarlierCopy()
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "Disk images are mounted with hdiutil, which only macOS has.");
        using TemporaryDirectory workDirectory = new();
        using TemporaryDirectory extractDirectory = new();
        string sourceDirectory = Directory.CreateDirectory(Path.Combine(workDirectory.Path, "source", "Test.app", "Contents")).FullName;
        File.WriteAllText(Path.Combine(sourceDirectory, "Info.plist"), "plist");
        Directory.CreateDirectory(Path.Combine(extractDirectory.Path, "Test.app", "leftover"));
        string diskImagePath = await CreateDiskImageAsync(workDirectory, Path.Combine(workDirectory.Path, "source"));

        await new DiskImageFileExtractor().ExtractFileContentsAsync(diskImagePath, extractDirectory.Path, TestContext.Current.CancellationToken);

        Assert.True(File.Exists(Path.Combine(extractDirectory.Path, "Test.app", "Contents", "Info.plist")));
        Assert.False(Directory.Exists(Path.Combine(extractDirectory.Path, "Test.app", "leftover")));
        Assert.False(Directory.Exists(Path.Combine(extractDirectory.Path, "dmg-mount")));
        Assert.False(File.Exists(diskImagePath));
    }

    [Fact]
    public async Task DiskImageWithoutAppBundleFails()
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "Disk images are mounted with hdiutil, which only macOS has.");
        using TemporaryDirectory workDirectory = new();
        using TemporaryDirectory extractDirectory = new();
        string sourceDirectory = Directory.CreateDirectory(Path.Combine(workDirectory.Path, "source")).FullName;
        File.WriteAllText(Path.Combine(sourceDirectory, "README"), "no application here");
        string diskImagePath = await CreateDiskImageAsync(workDirectory, sourceDirectory);

        BrowserDownloadException exception = await Assert.ThrowsAsync<BrowserDownloadException>(() => new DiskImageFileExtractor().ExtractFileContentsAsync(diskImagePath, extractDirectory.Path, TestContext.Current.CancellationToken));

        Assert.Contains("No .app bundle found", exception.Message);
        Assert.False(File.Exists(diskImagePath));
    }

    // The extractor deletes the installer, so it runs a copy of the fake browser.
    private static string CopyFakeBrowser(TemporaryDirectory directory)
    {
        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(FakeBrowserPath)!, "Dramaturge.FakeBrowser*"))
        {
            File.Copy(file, Path.Combine(directory.Path, Path.GetFileName(file)));
        }

        return Path.Combine(directory.Path, Path.GetFileName(FakeBrowserPath));
    }

    private static async Task<string> CreateDiskImageAsync(TemporaryDirectory workDirectory, string sourceDirectory)
    {
        string diskImagePath = Path.Combine(workDirectory.Path, "test.dmg");
        ProcessStartInfo startInfo = new("hdiutil") { UseShellExecute = false };
        foreach (string argument in new[] { "create", "-srcfolder", sourceDirectory, "-volname", "Test", "-quiet", diskImagePath })
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)!;
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0, process.ExitCode);
        return diskImagePath;
    }

    private sealed class ProcessRunningExtractor : FileExtractor
    {
        public override Task ExtractFileContentsAsync(string installerPath, string extractDir, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task RunAsync(string fileName, string arguments, TimeSpan timeout) => this.RunProcessAsync(fileName, arguments, timeout);
    }
}
