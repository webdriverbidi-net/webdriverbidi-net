// <copyright file="VideoCaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::TUnit.Core;

// Most tests here fail on purpose, so that their videos are kept; ExpectedEnding checks them once each ends. The tests
// open their own pages, so that the class's session can be made to fail first, and they run one at a time.
[ExpectedEnding]
[NotInParallel(nameof(VideoCaptureTests))]
public class VideoCaptureTests : BrowserTest
{
    private const string SessionName = "video-capture";

    public VideoCaptureTests()
    {
        this.ArtifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-tunit-{Guid.NewGuid():N}");
        this.VideoOnFailure = true;
    }

    private static FakeSession Session => FakeBrowserSetUp.Server.SessionFor(SessionName);

    [Test]
    public async Task FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots()
    {
        string testDirectory = Path.Combine(this.ArtifactsDirectory, "Dramaturge.TUnit.VideoCaptureTests.FailedTestKeepsAVideoOfEveryPageBesideItsScreenshots");
        int startsBefore = Session.RemoteEnd.CommandsFor("browsingContext.startScreencast").Count;
        this.VideoOptions = new VideoRecordingOptions() { Width = 640 };
        Browser first = await this.NewBrowserAsync();
        await first.NewPageAsync();
        Page closed = await first.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await this.NewBrowserAsync();
        await second.NewPageAsync();

        this.ExpectEnding(context =>
        {
            string[] expected = ["page-1.png", "page-1.webm", "page-2.webm", "page-3.png", "page-3.webm"];
            string[] artifacts = [.. context.Output.Artifacts.Select(artifact => artifact.File.Name).Order()];
            string[] files = [.. Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order()!];
            JsonNode start = Session.RemoteEnd.CommandsFor("browsingContext.startScreencast")[startsBefore]["params"]!;
            string[] descriptions = [.. context.Output.Artifacts.Where(artifact => artifact.File.Extension == ".webm").Select(artifact => artifact.Description!).Distinct()];
            return !artifacts.SequenceEqual(expected) ? $"Attached {string.Join(", ", artifacts)}"
                : !files.SequenceEqual(expected) ? $"Saved {string.Join(", ", files)}"
                : !descriptions.SequenceEqual(["A video of a page of the failed test"]) ? $"Described {string.Join(", ", descriptions)}"
                : !File.ReadAllBytes(Path.Combine(testDirectory, "page-2.webm")).SequenceEqual(FakeBrowserServer.Video) ? "Saved another video"
                : start["video"]!.ToJsonString() != """{"width":640}""" ? $"Recorded with {start["video"]!.ToJsonString()}"
                : Directory.Exists((string)start["destinationFolder"]!) ? "Left the temporary folder"
                : context.Output.GetStandardOutput().Length != 0 ? $"Wrote '{context.Output.GetStandardOutput()}'"
                : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public async Task PassedTestsVideosAreDeleted()
    {
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();
        int stopsBefore = Session.RemoteEnd.CommandsFor("browsingContext.stopScreencast").Count;

        string root = this.ArtifactsDirectory;
        this.ExpectEnding(context =>
        {
            int stops = Session.RemoteEnd.CommandsFor("browsingContext.stopScreencast").Count - stopsBefore;
            return stops != 1 ? $"Stopped {stops} recordings"
                : context.Output.Artifacts.Count != 0 ? "Attached a video"
                : Directory.Exists(root) ? "Made a directory"
                : null;
        });
    }

    [Test]
    public async Task FailedTestKeepsItsVideosWhenScreenshotsAreOff()
    {
        this.ScreenshotOnFailure = false;
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();

        this.ExpectEnding(context =>
        {
            string[] artifacts = [.. context.Output.Artifacts.Select(artifact => artifact.File.Name)];
            return !artifacts.SequenceEqual(["page-1.webm"]) ? $"Attached {string.Join(", ", artifacts)}" : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public async Task BrowserThatCannotRecordIsReportedOnceAndTheTestRuns()
    {
        Session.RemoteEnd.FailWith("browsingContext.startScreencast", "unsupported operation", "Method browsingContext.startScreencast is not implemented.");
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();
        await browser.NewPageAsync();

        this.ExpectEnding(context =>
        {
            FakeBrowserServer.AnswerScreencasts(Session);
            string[] lines = context.Output.GetStandardOutput().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length != 1 || !lines[0].StartsWith("Dramaturge could not record video: The browser cannot record video: ", StringComparison.Ordinal) ? $"Wrote '{string.Join("|", lines)}'" : null;
        });
    }

    [Test]
    [Arguments("startScreencast", true, "Dramaturge could not record a video of page 1: ")]
    [Arguments("startScreencast", false, null)]
    [Arguments("stopScreencast", true, "Dramaturge could not save the video of page 1: ")]
    [Arguments("stopScreencast", false, null)]
    public async Task FailureToRecordOrSaveAVideoIsWrittenForAFailedTest(string command, bool failed, string? expected)
    {
        this.ScreenshotOnFailure = false;
        Session.RemoteEnd.FailWith($"browsingContext.{command}", "unknown error", "The encoder is busy.");
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();

        this.ExpectEnding(context =>
        {
            FakeBrowserServer.AnswerScreencasts(Session);
            string output = context.Output.GetStandardOutput();
            return context.Output.Artifacts.Count != 0 ? "Attached a video"
                : expected is null ? (output.Length != 0 ? $"Wrote '{output}'" : null)
                : !output.StartsWith(expected, StringComparison.Ordinal) ? $"Wrote '{output}'"
                : null;
        });
        if (failed)
        {
            throw new InvalidOperationException("The test failed.");
        }
    }

    [Test]
    public async Task VideoOfABrowserOnAnotherMachineIsReportedWhereItIs()
    {
        this.ScreenshotOnFailure = false;
        Session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new JsonObject() { ["path"] = "/home/grid/videos/screencast.webm" });
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();

        this.ExpectEnding(context =>
        {
            FakeBrowserServer.AnswerScreencasts(Session);
            string output = context.Output.GetStandardOutput().TrimEnd();
            return output != "Dramaturge could not save the video of page 1: the browser wrote it on its own machine, at /home/grid/videos/screencast.webm" ? $"Wrote '{output}'" : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }

    // Deletes the test's directory once checked.
    private void ExpectEnding(Func<TestContext, string?> check)
    {
        string directory = this.ArtifactsDirectory;
        ExpectedEndingAttribute.Expect(context =>
        {
            try
            {
                return check(context);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        });
    }
}
