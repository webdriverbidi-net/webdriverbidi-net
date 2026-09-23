// <copyright file="LaunchOptionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.Protocol;

public class LaunchOptionsTests
{
    // Each needs quoting or escaping in a Windows command line, or in the shell used for Unix pipe launches.
    private static readonly string[] ArgumentsNeedingQuotes = ["--plain", "--with-space=a b", "--quoted=say \"hi\"", @"--trailing-backslash=C:\dir\", "--single='quoted'", string.Empty];

    public static TheoryData<BrowserKind, ConnectionKind> DirectLaunches => new()
    {
        { BrowserKind.Chrome, ConnectionKind.WebSocket },
        { BrowserKind.Chrome, ConnectionKind.Pipes },
        { BrowserKind.Firefox, ConnectionKind.WebSocket },
    };

    [Theory]
    [MemberData(nameof(DirectLaunches))]
    public async Task ArgumentsReachBrowserUnchangedAfterLauncherArguments(BrowserKind browser, ConnectionKind connectionKind)
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithConnection(connectionKind)
            .WithArguments(ArgumentsNeedingQuotes)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string[] arguments = (await fakeBrowser.WaitForLaunchAsync()).Arguments;
        int firstAddedArgument = Array.IndexOf(arguments, ArgumentsNeedingQuotes[0]);
        Assert.Equal(ArgumentsNeedingQuotes, arguments.Skip(firstAddedArgument).Take(ArgumentsNeedingQuotes.Length));
        Assert.True(firstAddedArgument > Array.FindIndex(arguments, argument => argument.StartsWith("--user-data-dir=", StringComparison.Ordinal) || argument == "--profile"));
    }

    [Fact]
    public async Task WithoutDefaultArgumentsOmitsEveryChromeDefault()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithoutDefaultArguments()
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string[] arguments = fakeBrowser.Launches.Single().Arguments;
        Assert.Equal(2, arguments.Length);
        Assert.StartsWith("--user-data-dir=", arguments[0]);
        Assert.Equal("--remote-debugging-port=0", arguments[1]);
    }

    [Fact]
    public async Task WithoutDefaultArgumentsOmitsEveryFirefoxDefault()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithoutDefaultArguments()
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["--profile", fakeBrowser.GetLastLaunchArgument("--profile"), "--remote-debugging-port", "0"], fakeBrowser.Launches.Single().Arguments);
    }

    [Fact]
    public async Task WithoutDefaultArgumentsOmitsNamedDefaults()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithoutDefaultArguments("--disable-features", "--no-first-run", "about:blank")
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string[] arguments = fakeBrowser.Launches.Single().Arguments;
        Assert.DoesNotContain(arguments, argument => argument.StartsWith("--disable-features=", StringComparison.Ordinal));
        Assert.DoesNotContain("--no-first-run", arguments);
        Assert.DoesNotContain("about:blank", arguments);
        Assert.Contains("--disable-sync", arguments);
        Assert.Contains(arguments, argument => argument.StartsWith("--enable-features=", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(BrowserKind.Chrome)]
    [InlineData(BrowserKind.Firefox)]
    public async Task EnvironmentVariableReachesBrowser(BrowserKind browser)
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(browser))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithEnvironmentVariable(FakeBrowserSetup.EchoVariableName, "a value with spaces")
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        Assert.Equal("a value with spaces", fakeBrowser.Launches.Single().Echo);
    }

    [Fact]
    public async Task ChromeUserDataDirectoryIsUsedAndKept()
    {
        using FakeBrowserSetup fakeBrowser = new();
        using TemporaryDirectory profile = new();
        string profileDirectory = profile.Path;
        File.WriteAllText(Path.Combine(profileDirectory, "user-file"), "kept");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithUserDataDirectory(profileDirectory)
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        Assert.Equal(profileDirectory, fakeBrowser.GetLastLaunchArgument("--user-data-dir"));
        Assert.Equal("kept", File.ReadAllText(Path.Combine(profileDirectory, "user-file")));
        Assert.False(File.Exists(Path.Combine(profileDirectory, ".webdriverbidi-owner")));
    }

    [Fact]
    public async Task FirefoxUserDataDirectoryGetsPreferencesAndKeepsItsOwnSettings()
    {
        using FakeBrowserSetup fakeBrowser = new();
        using TemporaryDirectory profile = new();
        File.WriteAllText(Path.Combine(profile.Path, "prefs.js"), "user_pref(\"user.setting\", 1);");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithUserDataDirectory(profile.Path)
            .WithBrowserOptions(new FirefoxLaunchOptions() { Preferences = { ["custom.preference"] = "value" } })
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);
        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        await launcher.QuitBrowserAsync(TestContext.Current.CancellationToken);

        string userJs = File.ReadAllText(Path.Combine(profile.Path, "user.js"));
        Assert.Equal(profile.Path, fakeBrowser.GetLastLaunchArgument("--profile"));
        Assert.Contains("user_pref(\"remote.enabled\", true);", userJs);
        Assert.Contains("user_pref(\"custom.preference\", \"value\");", userJs);
        Assert.Equal("user_pref(\"user.setting\", 1);", File.ReadAllText(Path.Combine(profile.Path, "prefs.js")));
        Assert.False(File.Exists(Path.Combine(profile.Path, ".webdriverbidi-owner")));
    }

    [Fact]
    public async Task FirefoxPreferencesOverrideDefaults()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Firefox))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithBrowserOptions(new FirefoxLaunchOptions() { Preferences = { ["remote.enabled"] = false, ["apz.content_response_timeout"] = 5 } })
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string userJs = File.ReadAllText(Path.Combine(fakeBrowser.GetLastLaunchArgument("--profile"), "user.js"));
        Assert.Contains("user_pref(\"remote.enabled\", false);", userJs);
        Assert.DoesNotContain("user_pref(\"remote.enabled\", true);", userJs);
        Assert.Contains("user_pref(\"apz.content_response_timeout\", 5);", userJs);
    }

    [Fact]
    public async Task LaunchTimeoutLimitsWaitForReadiness()
    {
        using FakeBrowserSetup fakeBrowser = new(mode: "silent");
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithLaunchTimeout(TimeSpan.FromSeconds(1))
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        BrowserLaunchException exception = await Assert.ThrowsAsync<BrowserLaunchException>(() => launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken));

        Assert.Contains("within 1 seconds", exception.Message);
    }

    [Fact]
    public async Task HeadlessChromeIsSandboxedUnlessRunningAsRootOnLinux()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithHeadlessOption()
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string[] arguments = fakeBrowser.Launches.Single().Arguments;
        Assert.Contains("--headless=new", arguments);
        Assert.Equal(OperatingSystem.IsLinux() && Environment.UserName == "root", arguments.Contains("--no-sandbox"));
    }

    [Fact]
    public async Task HeadlessShellIsLaunchedWithoutHeadlessArgument()
    {
        using FakeBrowserSetup fakeBrowser = new();
        await using BrowserLauncher launcher = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithHeadlessOption()
            .WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true })
            .Build();
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        Assert.DoesNotContain(fakeBrowser.Launches.Single().Arguments, argument => argument.StartsWith("--headless", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SettingsAddedAfterBuildDoNotReachBuiltLauncher()
    {
        using FakeBrowserSetup fakeBrowser = new();
        BrowserLauncherBuilder builder = fakeBrowser.Apply(BrowserLauncher.Configure(BrowserKind.Chrome))
            .AtLocation(FakeBrowserSetup.ExecutablePath)
            .WithArguments("--before-build");
        await using BrowserLauncher launcher = builder.Build();
        builder.WithArguments("--after-build");
        await launcher.StartAsync(TestContext.Current.CancellationToken);

        await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);

        string[] arguments = fakeBrowser.Launches.Single().Arguments;
        Assert.Contains("--before-build", arguments);
        Assert.DoesNotContain("--after-build", arguments);
    }
}
