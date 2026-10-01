// <copyright file="EnvironmentConfigurationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using Microsoft.Extensions.Time.Testing;

// These tests set environment variables of the test process itself, so they must not run
// alongside other tests.
[Collection("NonParallel")]
public class EnvironmentConfigurationTests
{
    private const string BrowsersPathVariableName = "DRAMATURGE_BROWSERS_PATH";
    private const string SkipDownloadVariableName = "DRAMATURGE_SKIP_DOWNLOAD";
    private const string DownloadManifestVariableName = "DRAMATURGE_DOWNLOAD_MANIFEST";

    [Fact]
    public void BrowsersPathVariableSetsCacheDirectory()
    {
        using VariableOverride variable = new(BrowsersPathVariableName, "/browsers/from/environment");

        Assert.Equal("/browsers/from/environment", new BrowserDownloadOptions().CacheDirectory);
    }

    [Fact]
    public void ExplicitCacheDirectoryOverridesBrowsersPathVariable()
    {
        using VariableOverride variable = new(BrowsersPathVariableName, "/browsers/from/environment");

        Assert.Equal("/explicit", new BrowserDownloadOptions() { CacheDirectory = "/explicit" }.CacheDirectory);
    }

    [Fact]
    public void WhitespaceBrowsersPathVariableIsIgnored()
    {
        using VariableOverride variable = new(BrowsersPathVariableName, "   ");

        Assert.Equal(BrowserDownloadOptions.DefaultCacheDirectory, new BrowserDownloadOptions().CacheDirectory);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData(" TRUE ", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("yes", false)]
    public void SkipDownloadVariableSetsSkipDownload(string value, bool expected)
    {
        using VariableOverride variable = new(SkipDownloadVariableName, value);

        Assert.Equal(expected, new BrowserDownloadOptions().SkipDownload);
    }

    [Fact]
    public void DownloadManifestVariableAcceptsUrl()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, "https://mirror.example/manifest.json");

        Assert.Equal(new Uri("https://mirror.example/manifest.json"), new BrowserDownloadOptions().ManifestUrl);
    }

    [Fact]
    public void DownloadManifestVariableAcceptsRelativeFilePath()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, Path.Combine("mirror", "manifest.json"));

        Uri? manifestUrl = new BrowserDownloadOptions().ManifestUrl;

        Assert.NotNull(manifestUrl);
        Assert.True(manifestUrl.IsFile);
        Assert.Equal(Path.GetFullPath(Path.Combine("mirror", "manifest.json")), manifestUrl.LocalPath);
    }

    [Fact]
    public void DownloadManifestVariableAcceptsFileUrl()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, "file:///mirror/manifest.json");

        Assert.Equal(new Uri("file:///mirror/manifest.json"), new BrowserDownloadOptions().ManifestUrl);
    }

    [Fact]
    public void DownloadManifestVariableWithOtherSchemeIsFilePath()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, "ftp:manifest.json");

        Uri? manifestUrl = new BrowserDownloadOptions().ManifestUrl;

        Assert.NotNull(manifestUrl);
        Assert.True(manifestUrl.IsFile);
    }

    [Fact]
    public void ExplicitManifestUrlOverridesVariable()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, "https://mirror.example/manifest.json");

        Assert.Null(new BrowserDownloadOptions() { ManifestUrl = null }.ManifestUrl);
    }

    [Fact]
    public void ExplicitSkipDownloadOverridesVariable()
    {
        using VariableOverride variable = new(SkipDownloadVariableName, "true");

        Assert.False(new BrowserDownloadOptions() { SkipDownload = false }.SkipDownload);
    }

    [Fact]
    public void DefaultCacheDirectoryFollowsPlatformConvention()
    {
        using VariableOverride variable = new("XDG_CACHE_HOME", null);
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string expectedRoot = OperatingSystem.IsWindows() ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : OperatingSystem.IsMacOS() ? Path.Combine(userProfile, "Library", "Caches")
            : Path.Combine(userProfile, ".cache");

        Assert.Equal(Path.Combine(expectedRoot, "dramaturge"), BrowserDownloadOptions.DefaultCacheDirectory);
    }

    [Theory]
    [InlineData("/xdg/cache", "/xdg/cache/dramaturge")]
    [InlineData("relative/cache", null)]
    public void LinuxDefaultCacheDirectoryHonorsAbsoluteXdgCacheHome(string xdgCacheHome, string? expected)
    {
        Assert.SkipUnless(OperatingSystem.IsLinux(), "XDG_CACHE_HOME applies only on Linux.");
        using VariableOverride variable = new("XDG_CACHE_HOME", xdgCacheHome);

        Assert.Equal(expected ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "dramaturge"), BrowserDownloadOptions.DefaultCacheDirectory);
    }

    [Fact]
    public async Task CacheIsListedAndCleanedWithoutOptions()
    {
        using TestUtilities.TemporaryDirectory cache = new();
        using VariableOverride variable = new(BrowsersPathVariableName, cache.Path);
        TestUtilities.CacheSeeder.SeedInstallation(cache, "drivers/geckodriver", "0.36.0", "geckodriver");

        CachedInstallation installation = Assert.Single(BrowserCache.List());
        await BrowserCache.RemoveAsync(installation, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(BrowserCache.List());
    }

    // The variable holds a URL or a file path, and anything that is not an http, https, or file URL is taken as a path.
    [Fact]
    public void ManifestVariableWithOtherSchemeIsAPath()
    {
        using VariableOverride variable = new(DownloadManifestVariableName, "ftp://mirror.example/manifest.json");

        Uri manifestUrl = new BrowserDownloadOptions().ManifestUrl!;

        Assert.True(manifestUrl.IsFile);
        Assert.EndsWith("manifest.json", manifestUrl.LocalPath);
    }

    // The version is read by running the browser, which inherits the mode from this process.
    [Fact]
    public async Task BrowserThatNeverReportsItsVersionGetsTheLatestDriverOfTheChannel()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Windows reads the file version without running the browser.");
        await using TestUtilities.DownloadServer server = await TestUtilities.DownloadServer.StartAsync();
        TestUtilities.ChromeForTestingService.Serve(server, "Stable", "131.0.6778.204");
        using TestUtilities.TemporaryDirectory cache = new();
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        using VariableOverride variable = new("DRAMATURGE_FAKE_BROWSER_MODE", "hang-version");

        Task<string?> find = DriverLocator.FindDriverAsync(BrowserKind.Chrome, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: TestUtilities.FakeBrowserSetup.ExecutablePath, downloadOptions: TestUtilities.TestDownloadOptions.Create(server, cache, timeProvider: timeProvider), cancellationToken: TestContext.Current.CancellationToken);
        while (!find.IsCompleted)
        {
            timeProvider.Advance(TimeSpan.FromSeconds(10));
            await Task.WhenAny(find, Task.Delay(50, TestContext.Current.CancellationToken));
        }

        Assert.Equal(Path.Combine(cache.Path, "drivers", "chromedriver", "131.0.6778.204", "chromedriver"), await find);
    }

    private sealed class VariableOverride : IDisposable
    {
        private readonly string name;
        private readonly string? originalValue;

        public VariableOverride(string name, string? value)
        {
            this.name = name;
            this.originalValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(this.name, this.originalValue);
        }
    }
}
