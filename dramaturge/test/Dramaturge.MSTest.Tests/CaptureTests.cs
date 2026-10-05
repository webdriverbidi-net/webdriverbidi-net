// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

// The tests here create test objects themselves, which share the group of this class, as its tests would, and end
// them as a failed test ends.
[TestClass]
public class CaptureTests
{
    private const string SessionName = "capture";

    private readonly string artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-mstest-{Guid.NewGuid():N}");

    public TestContext TestContext { get; set; } = null!;

    [ClassCleanup]
    public static Task CloseGroupAsync(TestContext context) => BrowserTest.CloseClassGroupAsync(context);

    [TestCleanup]
    public void DeleteArtifactsDirectory()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [TestMethod]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.MSTest.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        // Pages are numbered in the order they opened, so the closed second page has no screenshot.
        CollectionAssert.AreEqual(new[] { Path.Combine(testDirectory, "page-1.png"), Path.Combine(testDirectory, "page-3.png") }, ending.ResultFiles);
        CollectionAssert.AreEqual(new[] { "page-1.png", "page-3.png" }, Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order().ToList());
        CollectionAssert.AreEqual(FakeBrowserServer.Screenshot, File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")));
        Assert.AreEqual(string.Empty, ending.Output);
    }

    [TestMethod]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharactersWhenTheNamesOfTheClassAndTheMethodTogetherAreLongerThanThat()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        CollectionAssert.AreEqual(new[] { "Dramaturge.MSTest.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharactersWhenTheNamesOfTheClassAndTheMethodTo" }, Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName).ToList());
    }

    [TestMethod]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed, "Named (a/b-c_d)");

        CollectionAssert.AreEqual(new[] { "Dramaturge.MSTest.CaptureTests.Named__a_b-c_d_" }, Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName).ToList());
    }

    [TestMethod]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Passed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        await test.Page.CloseAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedCaptureIsWrittenToTheOutput()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(SessionName);
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

            Assert.IsEmpty(ending.ResultFiles);
            Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.MSTest.CaptureTests.FailedCaptureIsWrittenToTheOutput", "page-1.png")}: ", ending.Output);
            Assert.Contains("The page cannot be captured.", ending.Output);
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [TestMethod]
    public async Task UnwritableDirectoryIsWrittenToTheOutput()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = await this.StartAsync(file);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.MSTest.CaptureTests.UnwritableDirectoryIsWrittenToTheOutput")}: ", ending.Output);
    }

    [TestMethod]
    public void DefaultsAreTestResultsBesideTheTestsAndTheEnvironmentsLauncher()
    {
        DefaultsTest test = new();
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge"), test.Directory);
        Assert.IsTrue(test.ScreenshotsOn);
        Assert.AreSame(builder, test.Configure(builder));
        Assert.AreSame(builder, new DefaultsFixture().Configure(builder));
    }

    [TestMethod]
    public async Task FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots()
    {
        FakeSession session = await this.SessionAsync();
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.MSTest.CaptureTests.FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots");
        int startsBefore = session.RemoteEnd.CommandsFor("browsingContext.startScreencast").Count;
        CapturedTest test = await this.StartAsync(this.artifactsDirectory, video: new VideoRecordingOptions() { Width = 640 });
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        string[] files = ["page-1.png", "page-1.webm", "page-2.webm", "page-3.png", "page-3.webm"];
        CollectionAssert.AreEqual(files, ending.ResultFiles.Select(Path.GetFileName).Order().ToList());
        CollectionAssert.AreEqual(files, Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order().ToList());
        CollectionAssert.AreEqual(FakeBrowserServer.Video, File.ReadAllBytes(Path.Combine(testDirectory, "page-2.webm")));
        System.Text.Json.Nodes.JsonNode start = session.RemoteEnd.CommandsFor("browsingContext.startScreencast")[startsBefore]["params"]!;
        Assert.AreEqual("""{"width":640}""", start["video"]!.ToJsonString());
        Assert.IsFalse(Directory.Exists((string)start["destinationFolder"]!));
        Assert.AreEqual(string.Empty, ending.Output);
    }

    [TestMethod]
    public async Task PassedTestsVideosAreDeletedAndWithScreenshotsOffAFailedTestKeepsThem()
    {
        CapturedTest passed = await this.StartAsync(this.artifactsDirectory, videoOnFailure: true);
        EndingContext passedEnding = await EndingContext.EndAsync(passed, this.TestContext, UnitTestOutcome.Passed);
        bool directoryAfterPass = Directory.Exists(this.artifactsDirectory);
        CapturedTest failed = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);
        EndingContext failedEnding = await EndingContext.EndAsync(failed, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(passedEnding.ResultFiles);
        Assert.IsFalse(directoryAfterPass);
        CollectionAssert.AreEqual(new[] { "page-1.webm" }, failedEnding.ResultFiles.Select(Path.GetFileName).ToList());
    }

    [TestMethod]
    public async Task BrowserThatCannotRecordIsReportedOnceAndTheTestRuns()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith("browsingContext.startScreencast", "unsupported operation", "Method browsingContext.startScreencast is not implemented.");
        try
        {
            CapturedTest test = await this.StartAsync(this.artifactsDirectory, videoOnFailure: true);
            await test.Browser.NewPageAsync();

            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Passed);

            string[] lines = ending.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.HasCount(1, lines);
            Assert.StartsWith("Dramaturge could not record video: The browser cannot record video: ", lines[0]);
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [TestMethod]
    [DataRow("startScreencast", true, "Dramaturge could not record a video of page 1: ")]
    [DataRow("startScreencast", false, null)]
    [DataRow("stopScreencast", true, "Dramaturge could not save the video of page 1: ")]
    [DataRow("stopScreencast", false, null)]
    public async Task FailureToRecordOrSaveAVideoIsWrittenForAFailedTest(string command, bool failed, string? expected)
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith($"browsingContext.{command}", "unknown error", "The encoder is busy.");
        try
        {
            CapturedTest test = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);

            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, failed ? UnitTestOutcome.Failed : UnitTestOutcome.Passed);

            Assert.IsEmpty(ending.ResultFiles);
            if (expected is null)
            {
                Assert.AreEqual(string.Empty, ending.Output);
            }
            else
            {
                Assert.StartsWith(expected, ending.Output);
            }
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    [TestMethod]
    public async Task VideoOfABrowserOnAnotherMachineIsReportedWhereItIs()
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new System.Text.Json.Nodes.JsonObject() { ["path"] = "/home/grid/videos/screencast.webm" });
        try
        {
            CapturedTest test = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false, videoOnFailure: true);

            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

            Assert.AreEqual("Dramaturge could not save the video of page 1: the browser wrote it on its own machine, at /home/grid/videos/screencast.webm", ending.Output.TrimEnd());
        }
        finally
        {
            FakeBrowserServer.AnswerScreencasts(session);
        }
    }

    // The class's group, and so its session, is launched by the first test object that needs it.
    private async Task<FakeSession> SessionAsync()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Passed);
        return FakeBrowserSetUp.Server.SessionFor(SessionName);
    }

    [TestMethod]
    public async Task FailedTestKeepsOneTraceOfItsBrowsersAndAPassedTestDeletesIt()
    {
        CapturedTest passed = await this.StartAsync(this.artifactsDirectory, trace: new TraceRecordingOptions());
        EndingContext passedEnding = await EndingContext.EndAsync(passed, this.TestContext, UnitTestOutcome.Passed);
        bool directoryAfterPass = Directory.Exists(this.artifactsDirectory);
        CapturedTest test = await this.StartAsync(this.artifactsDirectory, traceOnFailure: true);
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(passedEnding.ResultFiles);
        Assert.IsFalse(directoryAfterPass);
        CollectionAssert.AreEqual(new[] { "page-1.png", "page-2.png", "trace.zip" }, ending.ResultFiles.Select(Path.GetFileName).Order().ToList());
        using System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.OpenRead(ending.ResultFiles.Single(file => file.EndsWith("trace.zip", StringComparison.Ordinal)));
        string[] entries = [.. zip.Entries.Select(entry => entry.FullName)];
        CollectionAssert.AreEqual(new[] { "0-trace.network", "0-trace.trace", "1-trace.network", "1-trace.trace" }, entries.Where(entry => entry.Contains("trace.", StringComparison.Ordinal)).Order().ToList());
        Assert.IsTrue(entries.Any(entry => entry.StartsWith("src/", StringComparison.Ordinal)));
        CollectionAssert.AllItemsAreUnique(entries);
        Assert.AreEqual(string.Empty, ending.Output);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task TraceThatCannotStartIsWrittenForAFailedTest(bool failed)
    {
        FakeSession session = await this.SessionAsync();
        session.RemoteEnd.FailWith("network.addDataCollector", "unknown error", "No collectors.");
        try
        {
            CapturedTest test = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false, traceOnFailure: true);

            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, failed ? UnitTestOutcome.Failed : UnitTestOutcome.Passed);

            Assert.IsEmpty(ending.ResultFiles);
            if (failed)
            {
                Assert.StartsWith("Dramaturge could not record a trace of browser 1: ", ending.Output);
                Assert.Contains("No collectors.", ending.Output);
            }
            else
            {
                Assert.AreEqual(string.Empty, ending.Output);
            }
        }
        finally
        {
            session.RemoteEnd.AnswerWith("network.addDataCollector", new System.Text.Json.Nodes.JsonObject() { ["collector"] = "collector-restored" });
        }
    }

    private async Task<CapturedTest> StartAsync(string artifactsDirectory, bool screenshotOnFailure = true, bool videoOnFailure = false, VideoRecordingOptions? video = null, bool traceOnFailure = false, TraceRecordingOptions? trace = null)
    {
        CapturedTest test = new(artifactsDirectory, screenshotOnFailure, videoOnFailure, video, traceOnFailure, trace) { TestContext = this.TestContext };
        await test.SetUpBrowsersAsync();
        await test.OpenPageAsync();
        return test;
    }

    private sealed class CapturedTest : PageTest
    {
        public CapturedTest(string artifactsDirectory, bool screenshotOnFailure, bool videoOnFailure, VideoRecordingOptions? video, bool traceOnFailure, TraceRecordingOptions? trace)
        {
            this.ArtifactsDirectory = artifactsDirectory;
            this.ScreenshotOnFailure = screenshotOnFailure;
            this.VideoOnFailure = videoOnFailure || video is not null;
            this.VideoOptions = video;
            this.TraceOnFailure = traceOnFailure || trace is not null;
            this.TraceOptions = trace;
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

    private sealed class DefaultsFixture : DramaturgeAssemblyFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
