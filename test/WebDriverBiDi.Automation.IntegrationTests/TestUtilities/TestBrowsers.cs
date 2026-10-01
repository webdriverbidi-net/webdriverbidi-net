// <copyright file="TestBrowsers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using Dramaturge.Browsers;

/// <summary>
/// The browsers the tests run against: the executable named by CHROME_EXECUTABLE or FIREFOX_EXECUTABLE, or else,
/// outside CI, the downloaded Alpha channel. In CI, a browser whose variable is not set is skipped.
/// </summary>
public static class TestBrowsers
{
    /// <summary>
    /// The browsers every test runs against.
    /// </summary>
    public static readonly TheoryData<BrowserKind> All = [BrowserKind.Chrome, BrowserKind.Firefox];

    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Launches a group for a browser, skipping the test if the browser is not configured in CI.
    /// </summary>
    /// <param name="browser">The browser.</param>
    /// <param name="options">The group's options, or <see langword="null"/> for the defaults.</param>
    /// <param name="configure">Further configuration of the launcher, such as session capabilities, or <see langword="null"/> for none.</param>
    /// <returns>The launched group.</returns>
    public static Task<BrowserGroup> LaunchAsync(BrowserKind browser, AutomationOptions? options = null, Action<BrowserLauncherBuilder>? configure = null)
    {
        string variableName = browser == BrowserKind.Chrome ? "CHROME_EXECUTABLE" : "FIREFOX_EXECUTABLE";
        string? executablePath = Environment.GetEnvironmentVariable(variableName);
        bool isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        Assert.SkipWhen(isCI && string.IsNullOrEmpty(executablePath), $"{variableName} is not set.");
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(browser).WithReleaseChannel(BrowserReleaseChannel.Alpha);
        if (string.IsNullOrEmpty(executablePath))
        {
            builder.AtAutomaticallyDownloadedLocation();
        }
        else
        {
            builder.AtLocation(executablePath);
        }

        configure?.Invoke(builder);
        return BrowserGroup.LaunchAsync(builder, options, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Waits for a condition the browser reports through events, failing after ten seconds.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="description">What is awaited, for the failure message.</param>
    /// <param name="describeState">Describes what was observed instead, for the failure message.</param>
    /// <returns>A task that completes when the condition holds.</returns>
    public static async Task WaitUntilAsync(Func<bool> condition, string description, Func<string>? describeState = null)
    {
        DateTime giveUpAt = DateTime.UtcNow + WaitTimeout;
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < giveUpAt, $"Timed out waiting for {description}. {describeState?.Invoke()}");
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }
}
