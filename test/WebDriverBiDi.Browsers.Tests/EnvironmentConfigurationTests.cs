// <copyright file="EnvironmentConfigurationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

// These tests set environment variables of the test process itself, so they must not run
// alongside other tests.
[Collection("NonParallel")]
public class EnvironmentConfigurationTests
{
    private const string BrowsersPathVariableName = "WEBDRIVERBIDI_BROWSERS_PATH";
    private const string SkipDownloadVariableName = "WEBDRIVERBIDI_SKIP_DOWNLOAD";

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

        Assert.Equal(Path.Combine(expectedRoot, "webdriverbidi-net"), BrowserDownloadOptions.DefaultCacheDirectory);
    }

    [Theory]
    [InlineData("/xdg/cache", "/xdg/cache/webdriverbidi-net")]
    [InlineData("relative/cache", null)]
    public void LinuxDefaultCacheDirectoryHonorsAbsoluteXdgCacheHome(string xdgCacheHome, string? expected)
    {
        Assert.SkipUnless(OperatingSystem.IsLinux(), "XDG_CACHE_HOME applies only on Linux.");
        using VariableOverride variable = new("XDG_CACHE_HOME", xdgCacheHome);

        Assert.Equal(expected ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "webdriverbidi-net"), BrowserDownloadOptions.DefaultCacheDirectory);
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
