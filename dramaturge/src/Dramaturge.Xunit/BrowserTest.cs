// <copyright file="BrowserTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;
using Dramaturge.Testing;
using global::Xunit;

/// <summary>
/// A base class for tests that open browsers. Its tests share the assembly's browser group, unless the class overrides
/// <see cref="ConfigureLauncher"/> or <see cref="GroupOptions"/>, when they share a group of the class's own. Each
/// browser a test opens is isolated from others, and closed when the test ends; if the test fails, its pages are
/// first captured to <see cref="ArtifactsDirectory"/>, with their
/// videos if <see cref="VideoOnFailure"/> is on, and attached to the test's result.
/// </summary>
public abstract class BrowserTest : IAsyncLifetime, IClassFixture<ClassBrowserGroup>
{
    private readonly TestBrowsers browsers = new();
    private BrowserGroup? group;

    /// <summary>
    /// Gets the browser group the test's browsers are opened in.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown before the test starts.</exception>
    public BrowserGroup Group => this.group ?? throw new InvalidOperationException("The browser group is available once the test has started.");

    /// <summary>
    /// Gets or sets a value indicating whether a failed test's pages are captured. The default is <see langword="true"/>.
    /// </summary>
    protected bool ScreenshotOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the directory under which a failed test's screenshots are written, in a directory named for the
    /// test. The default is TestResults/Dramaturge in the test assembly's directory.
    /// </summary>
    protected string ArtifactsDirectory { get; set; } = TestBrowsers.DefaultArtifactsDirectory;

    /// <summary>
    /// Gets or sets a value indicating whether each page a test opens is recorded from when it opens, a failed test's
    /// videos being kept beside its screenshots, and a passed test's deleted. The default is <see langword="false"/>.
    /// Set it before the test opens a page, as in the class's constructor. A browser that cannot record video is
    /// reported on the test, which runs as usual.
    /// </summary>
    protected bool VideoOnFailure { get; set; }

    /// <summary>
    /// Gets or sets the settings of the videos <see cref="VideoOnFailure"/> records, or <see langword="null"/> for the
    /// browser's own.
    /// </summary>
    protected VideoRecordingOptions? VideoOptions { get; set; }

    /// <summary>
    /// Gets the settings of the browsers the test opens without settings of their own.
    /// </summary>
    protected virtual BrowserOptions? BrowserOptions => null;

    /// <summary>
    /// Gets the options of the class's own group. Overriding this gives the class a group of its own.
    /// </summary>
    protected virtual DramaturgeOptions? GroupOptions => null;

    /// <summary>
    /// Opens an isolated browser, closed when the test ends.
    /// </summary>
    /// <param name="options">The browser's settings, or <see langword="null"/> for <see cref="BrowserOptions"/>.</param>
    /// <returns>The browser.</returns>
    public Task<Browser> NewBrowserAsync(BrowserOptions? options = null)
    {
        return this.browsers.CreateAsync(this.Group, options ?? this.BrowserOptions, this.VideoOnFailure ? this.VideoOptions ?? new VideoRecordingOptions() : null, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Gets the test's browser group, launching it if this is the first test to need it. A derived class that
    /// overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the group is ready.</returns>
    public virtual async ValueTask InitializeAsync()
    {
        if (GroupChoice.UsesOwnGroup(this.GetType(), typeof(BrowserTest)))
        {
            ClassBrowserGroup classGroup = (await TestContext.Current.GetFixture<ClassBrowserGroup>().ConfigureAwait(false))!;
            this.group = await classGroup.GetAsync(this.ConfigureLauncher, this.GroupOptions).ConfigureAwait(false);
        }
        else
        {
            this.group = await DramaturgeAssemblyFixture.GetSharedGroupAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Captures the pages of a failed test, then closes the test's browsers. A failure to do either is reported as a
    /// warning on the test, and does not change its result. A derived class that overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the browsers are closed.</returns>
    public virtual async ValueTask DisposeAsync()
    {
        ITestContext context = TestContext.Current;
        bool failed = context.TestState?.Result == TestResult.Failed;
        foreach (PageCapture capture in await this.browsers.FinishAsync(failed, this.ScreenshotOnFailure, this.ArtifactsDirectory, context.Test!.TestDisplayName).ConfigureAwait(false))
        {
            if (capture.Failure is not null)
            {
                context.AddWarning(capture.Failure);
            }
            else
            {
                context.AddAttachment(Path.GetFileName(capture.Path), capture.Contents!, capture.MediaType);
            }
        }

        foreach (Exception failure in await this.browsers.CloseAsync().ConfigureAwait(false))
        {
            context.AddWarning($"Dramaturge could not close a browser: {failure.Message}");
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Configures the launcher of the class's own group. Overriding this gives the class a group of its own.
    /// </summary>
    /// <param name="builder">The launcher the environment chooses, from <see cref="BrowserLauncher.ConfigureFromEnvironment"/>.</param>
    /// <returns>The builder to launch with: <paramref name="builder"/>, changed or not, or another.</returns>
    protected virtual BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return builder;
    }
}
