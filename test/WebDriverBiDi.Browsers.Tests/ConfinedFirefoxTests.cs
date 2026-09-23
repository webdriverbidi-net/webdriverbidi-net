// <copyright file="ConfinedFirefoxTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.Versioning;
using WebDriverBiDi.Browsers.TestUtilities;

// A Snap or Flatpak Firefox cannot read a profile in the temporary directory, so launching one
// without a user data directory fails before anything is started.
public class ConfinedFirefoxTests
{
    [Theory]
    [InlineData("#!/bin/sh\nexec /snap/bin/firefox \"$@\"\n")]
    [InlineData("#!/bin/sh\nexec snap run firefox \"$@\"\n")]
    [InlineData("#!/bin/sh\nexec flatpak run org.mozilla.firefox \"$@\"\n")]
    public async Task LaunchRejectsWrapperScriptForConfinedFirefox(string script)
    {
        using TemporaryDirectory directory = new();
        string executablePath = Path.Combine(directory.Path, "firefox");
        File.WriteAllText(executablePath, script);

        BrowserLaunchException exception = await LaunchAsync(executablePath);

        Assert.Contains("Snap or Flatpak", exception.Message);
        Assert.Contains("WithUserDataDirectory", exception.Message);
    }

    [Theory]
    [InlineData("snap/firefox/current/usr/lib/firefox/firefox")]
    [InlineData("var/lib/flatpak/exports/bin/org.mozilla.firefox")]
    public async Task LaunchRejectsConfinedInstallation(string relativePath)
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Snap and Flatpak installations have Unix paths.");
        using TemporaryDirectory directory = new();
        string executablePath = Path.Combine(directory.Path, relativePath);

        await LaunchAsync(executablePath);
    }

    [Fact]
    public async Task LaunchRejectsLinkToConfinedInstallation()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Snap installations have Unix paths.");
        using TemporaryDirectory directory = new();
        string executablePath = Path.Combine(directory.Path, "firefox");
        File.CreateSymbolicLink(executablePath, "/snap/bin/firefox");

        await LaunchAsync(executablePath);
    }

    [Fact]
    [UnsupportedOSPlatform("windows")]
    public async Task LaunchWithUserDataDirectoryStartsConfinedFirefox()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Wrapper scripts are Unix shell scripts.");
        using FakeBrowserSetup fakeBrowser = new();
        using TemporaryDirectory directory = new();
        using TemporaryDirectory profile = new();
        string executablePath = Path.Combine(directory.Path, "firefox");
        File.WriteAllText(executablePath, $"#!/bin/sh\n# Stands in for: snap run firefox\nexec '{FakeBrowserSetup.ExecutablePath}' \"$@\"\n");
        File.SetUnixFileMode(executablePath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox)).AtLocation(executablePath).WithUserDataDirectory(profile.Path).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        Assert.Single(fakeBrowser.Launches);
    }

    private static async Task<BrowserLaunchException> LaunchAsync(string executablePath)
    {
        await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).AtLocation(executablePath).Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        return await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));
    }
}
