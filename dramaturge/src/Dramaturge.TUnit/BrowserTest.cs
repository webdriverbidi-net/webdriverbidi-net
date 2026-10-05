// <copyright file="BrowserTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using System.Collections.Concurrent;
using Dramaturge.Browsers;
using Dramaturge.Testing;
using global::TUnit.Core;
using global::TUnit.Core.Interfaces;

/// <summary>
/// A base class for test classes that open browsers. Its tests share the session's browser group, unless the class
/// overrides <see cref="ConfigureLauncher"/> or <see cref="GroupOptions"/>, when they share a group of the class's
/// own, closed when the class finishes; the class is the one running the test, so a test object created by a test
/// shares its class's group. Each browser a test opens is isolated from others, and closed when the test ends; if the
/// test fails, its pages are first captured to <see cref="ArtifactsDirectory"/>, with their
/// videos if <see cref="VideoOnFailure"/> is on, and attached to the test's result.
/// </summary>
/// <remarks>
/// TUnit runs hooks only from the assembly that declares them, so the class receives TUnit's test events instead.
/// </remarks>
public abstract class BrowserTest : ITestStartEventReceiver, ITestEndEventReceiver, ILastTestInClassEventReceiver
{
    private static readonly ConcurrentDictionary<Type, SharedBrowserGroup> ClassGroups = new();

    private readonly TestBrowsers browsers = new();
    private BrowserGroup? group;

    /// <summary>
    /// Gets the browser group the test's browsers are opened in.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown before the test starts.</exception>
    public BrowserGroup Group => this.group ?? throw new InvalidOperationException("The browser group is available once the test has started.");

    /// <inheritdoc/>
    int IEventReceiver.Order => int.MinValue;

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
    /// Closes a class's own group, if it has one.
    /// </summary>
    /// <param name="context">The context of the finishing class.</param>
    /// <returns>A task that completes when the group is closed.</returns>
    public static async Task CloseClassGroupAsync(ClassHookContext context)
    {
        if (ClassGroups.TryRemove(context.ClassType, out SharedBrowserGroup? classGroup))
        {
            await classGroup.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Opens an isolated browser, closed when the test ends.
    /// </summary>
    /// <param name="options">The browser's settings, or <see langword="null"/> for <see cref="BrowserOptions"/>.</param>
    /// <returns>The browser.</returns>
    public Task<Browser> NewBrowserAsync(BrowserOptions? options = null)
    {
        return this.browsers.CreateAsync(this.Group, options ?? this.BrowserOptions, this.VideoOnFailure ? this.VideoOptions ?? new VideoRecordingOptions() : null, TestContext.Current!.Execution.CancellationToken);
    }

    /// <summary>
    /// Gets the test's browser group, launching it if this is the first test to need it. A derived class that
    /// overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the group is ready.</returns>
    public virtual async Task SetUpBrowsersAsync()
    {
        Task<BrowserGroup> getGroup = GroupChoice.UsesOwnGroup(this.GetType(), typeof(BrowserTest))
            ? ClassGroups.GetOrAdd(TestContext.Current!.ClassContext.ClassType, _ => new SharedBrowserGroup()).GetAsync(this.ConfigureLauncher, this.GroupOptions)
            : DramaturgeAssemblyFixture.GetSharedGroupAsync();
        this.group = await getGroup.ConfigureAwait(false);
    }

    /// <summary>
    /// Captures the pages of a failed test, then closes the test's browsers. A failure to do either is written to the
    /// test's output, and does not change its result.
    /// </summary>
    /// <returns>A task that completes when the browsers are closed.</returns>
    public async Task TearDownBrowsersAsync()
    {
        TestContext context = TestContext.Current!;
        bool failed = context.Execution.Result?.State == TestState.Failed;
        foreach (PageCapture capture in await this.browsers.FinishAsync(failed, this.ScreenshotOnFailure, this.ArtifactsDirectory, $"{context.ClassContext.ClassType.FullName}.{context.Metadata.DisplayName}").ConfigureAwait(false))
        {
            if (capture.Failure is not null)
            {
                context.Output.WriteLine(capture.Failure);
            }
            else
            {
                context.Output.AttachArtifact(capture.Path, Path.GetFileName(capture.Path), capture.MediaType == PageCapture.Video ? "A video of a page of the failed test" : "A page open when the test failed");
            }
        }

        foreach (Exception failure in await this.browsers.CloseAsync().ConfigureAwait(false))
        {
            context.Output.WriteLine($"Dramaturge could not close a browser: {failure.Message}");
        }
    }

    /// <inheritdoc/>
    async ValueTask ITestStartEventReceiver.OnTestStart(TestContext context)
    {
        await this.SetUpBrowsersAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async ValueTask ITestEndEventReceiver.OnTestEnd(TestContext context)
    {
        await this.TearDownBrowsersAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async ValueTask ILastTestInClassEventReceiver.OnLastTestInClass(ClassHookContext context, TestContext testContext)
    {
        await CloseClassGroupAsync(context).ConfigureAwait(false);
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
