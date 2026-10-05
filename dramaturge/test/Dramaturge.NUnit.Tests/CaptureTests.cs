// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::NUnit.Framework.Interfaces;

// The tests here create test objects themselves, which share the group of this fixture, as its tests would, and end
// them as a failed test ends.
public class CaptureTests
{
    private const string SessionName = "capture";

    private string artifactsDirectory = string.Empty;

    [OneTimeTearDown]
    public Task CloseGroupAsync() => BrowserTest.CloseFixtureGroupAsync();

    [SetUp]
    public void CreateArtifactsDirectoryName()
    {
        this.artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-nunit-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void DeleteArtifactsDirectory()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [Test]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.NUnit.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        // Pages are numbered in the order they opened, so the closed second page has no screenshot.
        Assert.That(ending.Attachments, Is.EqualTo(new[] { Path.Combine(testDirectory, "page-1.png"), Path.Combine(testDirectory, "page-3.png") }));
        Assert.That(Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order(), Is.EqualTo(new[] { "page-1.png", "page-3.png" }));
        Assert.That(File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")), Is.EqualTo(FakeBrowserServer.Screenshot));
        Assert.That(ending.Output, Is.Empty);
    }

    [TestCase("a/b-c_d")]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters(string value)
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName), Is.EqualTo(new[] { "Dramaturge.NUnit.CaptureTests.DirectoryIsNamedForTheTestInPortableCharacters__a_b-c_d__" }));
    }

    [TestCase("012345678901234567890123456789012345678901234567890123456789")]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharacters(string value)
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName), Is.EqualTo(new[] { "Dramaturge.NUnit.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharacters__01234567890123456789012345678901234" }));
    }

    [Test]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Success);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = new(this.artifactsDirectory, screenshotOnFailure: false);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        await test.Page.CloseAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedCaptureIsWrittenToTheOutput()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(SessionName);
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

            Assert.That(ending.Attachments, Is.Empty);
            Assert.That(ending.Output, Does.StartWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.NUnit.CaptureTests.FailedCaptureIsWrittenToTheOutput", "page-1.png")}: "));
            Assert.That(ending.Output, Does.Contain("The page cannot be captured."));
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [Test]
    public async Task UnwritableDirectoryIsWrittenToTheOutput()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = new(file);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(ending.Output, Does.StartWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.NUnit.CaptureTests.UnwritableDirectoryIsWrittenToTheOutput")}: "));
    }

    [Test]
    public void DefaultsAreTestResultsBesideTheTestsAndTheEnvironmentsLauncher()
    {
        DefaultsTest test = new();
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.That(test.Directory, Is.EqualTo(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge")));
        Assert.That(test.ScreenshotsOn, Is.True);
        Assert.That(test.Configure(builder), Is.SameAs(builder));
        Assert.That(new DefaultsFixture().Configure(builder), Is.SameAs(builder));
    }

    [Test]
    public async Task FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots()
    {
        FakeSession session = await this.SessionAsync();
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.NUnit.CaptureTests.FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots");
        int startsBefore = session.RemoteEnd.CommandsFor("browsingContext.startScreencast").Count;
        CapturedTest test = new(this.artifactsDirectory, video: new VideoRecordingOptions() { Width = 640 });
        await test.SetUpBrowsersAsync();
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        string[] files = ["page-1.png", "page-1.webm", "page-2.webm", "page-3.png", "page-3.webm"];
        Assert.That(ending.Attachments.Select(Path.GetFileName).Order(), Is.EqualTo(files));
        Assert.That(Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order(), Is.EqualTo(files));
        Assert.That(File.ReadAllBytes(Path.Combine(testDirectory, "page-2.webm")), Is.EqualTo(FakeBrowserServer.Video));
        System.Text.Json.Nodes.JsonNode start = session.RemoteEnd.CommandsFor("browsingContext.startScreencast")[startsBefore]["params"]!;
        Assert.That(start["video"]!.ToJsonString(), Is.EqualTo("""{"width":640}"""));
        Assert.That(Directory.Exists((string)start["destinationFolder"]!), Is.False);
        Assert.That(ending.Output, Is.Empty);
    }

    [Test]
    public async Task PassedTestsVideosAreDeletedAndWithScreenshotsOffAFailedTestKeepsThem()
    {
        CapturedTest passed = new(this.artifactsDirectory, videoOnFailure: true);
        await passed.SetUpBrowsersAsync();
        TestEnding passedEnding = await TestEnding.EndAsync(passed, ResultState.Success);
        bool directoryAfterPass = Directory.Exists(this.artifactsDirectory);
        CapturedTest failed = new(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);
        await failed.SetUpBrowsersAsync();
        TestEnding failedEnding = await TestEnding.EndAsync(failed, ResultState.Failure);

        Assert.That(passedEnding.Attachments, Is.Empty);
        Assert.That(directoryAfterPass, Is.False);
        Assert.That(failedEnding.Attachments.Select(Path.GetFileName), Is.EqualTo(new[] { "page-1.webm" }));
    }

    [Test]
    public async Task BrowserThatCannotRecordIsReportedOnceAndTheTestRuns()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith("browsingContext.startScreencast", "unsupported operation", "Method browsingContext.startScreencast is not implemented.");
        try
        {
            CapturedTest test = new(this.artifactsDirectory, videoOnFailure: true);
            await test.SetUpBrowsersAsync();
            await test.Browser.NewPageAsync();

            TestEnding ending = await TestEnding.EndAsync(test, ResultState.Success);

            string[] lines = ending.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines, Has.Length.EqualTo(1));
            Assert.That(lines[0], Does.StartWith("Dramaturge could not record video: The browser cannot record video: "));
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [TestCase("startScreencast", true, "Dramaturge could not record a video of page 1: ")]
    [TestCase("startScreencast", false, null)]
    [TestCase("stopScreencast", true, "Dramaturge could not save the video of page 1: ")]
    [TestCase("stopScreencast", false, null)]
    public async Task FailureToRecordOrSaveAVideoIsWrittenForAFailedTest(string command, bool failed, string? expected)
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith($"browsingContext.{command}", "unknown error", "The encoder is busy.");
        try
        {
            CapturedTest test = new(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);
            await test.SetUpBrowsersAsync();

            TestEnding ending = await TestEnding.EndAsync(test, failed ? ResultState.Failure : ResultState.Success);

            Assert.That(ending.Attachments, Is.Empty);
            Assert.That(ending.Output, expected is null ? Is.Empty : Does.StartWith(expected));
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [Test]
    public async Task VideoOfABrowserOnAnotherMachineIsReportedWhereItIs()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new System.Text.Json.Nodes.JsonObject() { ["path"] = "/home/grid/videos/screencast.webm" });
        try
        {
            CapturedTest test = new(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);
            await test.SetUpBrowsersAsync();

            TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

            Assert.That(ending.Output.TrimEnd(), Is.EqualTo("Dramaturge could not save the video of page 1: the browser wrote it on its own machine, at /home/grid/videos/screencast.webm"));
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    // The fixture's group, and so its session, is launched by the first test object that needs it.
    private async Task<FakeSession> SessionAsync()
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        await TestEnding.EndAsync(test, ResultState.Success);
        return FakeBrowserSetUp.Server.SessionFor(SessionName);
    }

    private sealed class CapturedTest : PageTest
    {
        public CapturedTest(string artifactsDirectory, bool screenshotOnFailure = true, bool videoOnFailure = false, VideoRecordingOptions? video = null)
        {
            this.ArtifactsDirectory = artifactsDirectory;
            this.ScreenshotOnFailure = screenshotOnFailure;
            this.VideoOnFailure = videoOnFailure || video is not null;
            this.VideoOptions = video;
        }

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }

    private sealed class DefaultsTest : BrowserTest
    {
        public string Directory => this.ArtifactsDirectory;

        public bool ScreenshotsOn => this.ScreenshotOnFailure;

        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }

    private sealed class DefaultsFixture : DramaturgeSetUpFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
