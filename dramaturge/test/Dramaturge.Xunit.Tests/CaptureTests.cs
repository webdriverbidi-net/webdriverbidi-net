// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::Xunit.Sdk;

// The tests here create test objects themselves, which get the class group of this class, as tests of this class
// would, and end them as a failed test ends.
public sealed class CaptureTests(FakeBrowserFixture fixture) : IClassFixture<ClassBrowserGroup>, IDisposable
{
    private const string SessionName = "capture";

    private readonly string artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-xunit-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [Fact]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.Xunit.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        Page closed = await test.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await closed.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        // Pages are numbered in the order they opened, so the closed second page has no screenshot.
        Assert.Equal(["page-1.png", "page-3.png"], ending.Attachments.Keys.Order());
        Assert.All(ending.Attachments.Values, attachment => Assert.Equal(FakeBrowserServer.Screenshot, attachment.AsByteArray().ByteArray));
        Assert.All(ending.Attachments.Values, attachment => Assert.Equal("image/png", attachment.AsByteArray().MediaType));
        Assert.Equal(["page-1.png", "page-3.png"], Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order());
        Assert.Equal(FakeBrowserServer.Screenshot, File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")));
        Assert.Empty(ending.Warnings);
    }

    [Theory]
    [InlineData("a/b-c_d")]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters(string value)
    {
        Assert.NotNull(value);
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["Dramaturge.Xunit.CaptureTests.DirectoryIsNamedForTheTestInPortableCharacters_value___a_b-c_d__"], Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName));
    }

    [Theory]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharacters(string value)
    {
        Assert.NotNull(value);
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["Dramaturge.Xunit.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharacters_value___0123456789012345678901234567"], Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName));
    }

    [Fact]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.ForPassed(0));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory, screenshotOnFailure: false);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        await test.Page.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedCaptureIsAWarning()
    {
        FakeSession session = await this.SessionAsync();
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

            Assert.Empty(ending.Attachments);
            string warning = Assert.Single(ending.Warnings);
            Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.Xunit.CaptureTests.FailedCaptureIsAWarning", "page-1.png")}: ", warning);
            Assert.Contains("The page cannot be captured.", warning);
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [Fact]
    public async Task UnwritableDirectoryIsAWarning()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = new(fixture, file);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.Xunit.CaptureTests.UnwritableDirectoryIsAWarning")}: ", Assert.Single(ending.Warnings));
    }

    [Fact]
    public void ArtifactsDirectoryDefaultsToTestResultsBesideTheTests()
    {
        DefaultsTest test = new();

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge"), test.Directory);
        Assert.True(test.ScreenshotsOn);
    }

    [Fact]
    public void DefaultLaunchersAreTheEnvironmentsUnchanged()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.Same(builder, new DefaultsTest().Configure(builder));
        Assert.Same(builder, new DefaultsFixture().Configure(builder));
    }

    [Fact]
    public async Task FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots()
    {
        FakeSession session = await this.SessionAsync();
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.Xunit.CaptureTests.FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots");
        int startsBefore = session.RemoteEnd.CommandsFor("browsingContext.startScreencast").Count;
        CapturedTest test = new(fixture, this.artifactsDirectory, video: new VideoRecordingOptions() { Width = 640 });
        await test.InitializeAsync();
        Page closed = await test.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await closed.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["page-1.png", "page-1.webm", "page-2.webm", "page-3.png", "page-3.webm"], ending.Attachments.Keys.Order());
        Assert.All(ending.Attachments.Where(attachment => attachment.Key.EndsWith(".webm", StringComparison.Ordinal)).Select(attachment => attachment.Value.AsByteArray()), video =>
        {
            Assert.Equal(FakeBrowserServer.Video, video.ByteArray);
            Assert.Equal("video/webm", video.MediaType);
        });
        Assert.Equal(["page-1.png", "page-1.webm", "page-2.webm", "page-3.png", "page-3.webm"], Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order());
        JsonNode start = session.RemoteEnd.CommandsFor("browsingContext.startScreencast")[startsBefore]["params"]!;
        Assert.Equal("""{"width":640}""", start["video"]!.ToJsonString());
        Assert.False(Directory.Exists((string)start["destinationFolder"]!));
        Assert.Empty(ending.Warnings);
    }

    [Fact]
    public async Task PassedTestsVideosAreDeleted()
    {
        FakeSession session = await this.SessionAsync();
        int startsBefore = session.RemoteEnd.CommandsFor("browsingContext.startScreencast").Count;
        CapturedTest test = new(fixture, this.artifactsDirectory, video: new VideoRecordingOptions());
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.ForPassed(0));

        Assert.Empty(ending.Attachments);
        Assert.Empty(ending.Warnings);
        Assert.False(Directory.Exists(this.artifactsDirectory));
        Assert.False(Directory.Exists((string)session.RemoteEnd.CommandsFor("browsingContext.startScreencast")[startsBefore]["params"]!["destinationFolder"]!));
    }

    [Fact]
    public async Task FailedTestKeepsVideosWithScreenshotsOff()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["page-1.webm"], ending.Attachments.Keys);
    }

    [Fact]
    public async Task BrowserThatCannotRecordIsReportedOnceAndTheTestRuns()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith("browsingContext.startScreencast", "unsupported operation", "Method browsingContext.startScreencast is not implemented.");
        try
        {
            CapturedTest test = new(fixture, this.artifactsDirectory, video: new VideoRecordingOptions());
            await test.InitializeAsync();
            await test.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

            Ending ending = await EndAsync(test, TestResultState.ForPassed(0));

            string warning = Assert.Single(ending.Warnings);
            Assert.StartsWith("Dramaturge could not record video: The browser cannot record video: ", warning);
            Assert.Empty(ending.Attachments);
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [Theory]
    [InlineData("startScreencast", true, "Dramaturge could not record a video of page 1: ")]
    [InlineData("startScreencast", false, null)]
    [InlineData("stopScreencast", true, "Dramaturge could not save the video of page 1: ")]
    [InlineData("stopScreencast", false, null)]
    public async Task FailureToRecordOrSaveAVideoIsAWarningOnAFailedTest(string command, bool failed, string? expectedWarning)
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith($"browsingContext.{command}", "unknown error", "The encoder is busy.");
        try
        {
            CapturedTest test = new(fixture, this.artifactsDirectory, screenshotOnFailure: false, video: new VideoRecordingOptions());
            await test.InitializeAsync();

            Ending ending = await EndAsync(test, failed ? TestResultState.FromException(0, new InvalidOperationException("The test failed.")) : TestResultState.ForPassed(0));

            Assert.Empty(ending.Attachments);
            if (expectedWarning is null)
            {
                Assert.Empty(ending.Warnings);
            }
            else
            {
                Assert.StartsWith(expectedWarning, Assert.Single(ending.Warnings));
            }
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [Fact]
    public async Task VideoOfABrowserOnAnotherMachineIsReportedWhereItIs()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new JsonObject() { ["path"] = "/home/grid/videos/screencast.webm" });
        try
        {
            CapturedTest test = new(fixture, this.artifactsDirectory, screenshotOnFailure: false, video: new VideoRecordingOptions());
            await test.InitializeAsync();

            Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

            Assert.Equal("Dramaturge could not save the video of page 1: the browser wrote it on its own machine, at /home/grid/videos/screencast.webm", Assert.Single(ending.Warnings));
            Assert.Empty(ending.Attachments);
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    // The class's group, and so its session, is launched by the first test object that needs it.
    private async Task<FakeSession> SessionAsync()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        await EndAsync(test, TestResultState.ForPassed(0));
        return fixture.Server.SessionFor(SessionName);
    }

    // Changes to the test context made in an async method are not seen by its caller, so the current test's own
    // context is unchanged once this returns.
    private static async Task<Ending> EndAsync(BrowserTest test, TestResultState state)
    {
        ITestContext current = TestContext.Current;
        TestContext.SetForTest(current.Test!, TestEngineStatus.CleaningUp, current.CancellationToken, state, current.TestOutputHelper);
        await test.DisposeAsync();
        return new Ending(TestContext.Current.Attachments ?? new Dictionary<string, TestAttachment>(), TestContext.Current.Warnings ?? []);
    }

    private sealed record Ending(IReadOnlyDictionary<string, TestAttachment> Attachments, IReadOnlyList<string> Warnings);

    private sealed class CapturedTest : PageTest
    {
        private readonly FakeBrowserFixture fixture;

        public CapturedTest(FakeBrowserFixture fixture, string artifactsDirectory, bool screenshotOnFailure = true, bool videoOnFailure = false, VideoRecordingOptions? video = null)
        {
            this.fixture = fixture;
            this.ArtifactsDirectory = artifactsDirectory;
            this.ScreenshotOnFailure = screenshotOnFailure;
            this.VideoOnFailure = videoOnFailure || video is not null;
            this.VideoOptions = video;
        }

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return this.fixture.ConnectTo(SessionName);
        }
    }

    private sealed class DefaultsTest : BrowserTest
    {
        public string Directory => this.ArtifactsDirectory;

        public bool ScreenshotsOn => this.ScreenshotOnFailure;

        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }

    private sealed class DefaultsFixture : DramaturgeAssemblyFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
