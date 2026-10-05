// <copyright file="VideoRecordingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;

public sealed class VideoRecordingTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-video-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Fact]
    public async Task RecordingAsksForTheFolderOfThePathAndTheSettingsGiven()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        this.AnswerWith(session, string.Empty);
        string path = Path.Combine(this.directory, "videos", "checkout.webm");

        await using (VideoRecording recording = await page.RecordVideoAsync(path, new VideoRecordingOptions() { Width = 640, Height = 360, FrameRate = 10 }, TestContext.Current.CancellationToken))
        {
            Assert.Equal(Path.GetFullPath(path), recording.Path);
        }

        await using (await page.RecordVideoAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
        }

        IReadOnlyList<JsonObject> starts = session.RemoteEnd.CommandsFor("browsingContext.startScreencast");
        JsonNode withOptions = starts[0]["params"]!;
        Assert.Equal(page.Id, (string?)withOptions["context"]);
        Assert.Equal(Path.Combine(this.directory, "videos"), (string?)withOptions["destinationFolder"]);
        Assert.Equal("""{"width":640,"height":360,"frameRate":10}""", withOptions["video"]!.ToJsonString());
        Assert.False(starts[1]["params"]!.AsObject().ContainsKey("video"));
        Assert.True(Directory.Exists(Path.Combine(this.directory, "videos")));
        Assert.Equal(["screencast-1", "screencast-1"], session.RemoteEnd.CommandsFor("browsingContext.stopScreencast").Select(command => (string?)command["params"]!["screencast"]));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FileWrittenElsewhereOnThisMachineIsMovedToThePathReplacingAnOlderOne(bool hasOlderFile)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string written = Path.Combine(this.directory, "downloads", "screencast-1.webm");
        Directory.CreateDirectory(Path.GetDirectoryName(written)!);
        File.WriteAllText(written, "new video");
        string path = Path.Combine(this.directory, "checkout.webm");
        if (hasOlderFile)
        {
            File.WriteAllText(path, "older video");
        }

        this.AnswerWith(session, written);

        VideoRecording recording = await page.RecordVideoAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        string stopped = await recording.StopAsync(TestContext.Current.CancellationToken);
        string again = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(path, stopped);
        Assert.Equal(path, again);
        Assert.Equal("new video", File.ReadAllText(path));
        Assert.False(File.Exists(written));
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.stopScreencast"));
    }

    [Theory]
    [InlineData("at-the-path")]
    [InlineData("remote")]
    [InlineData("unreported")]
    public async Task FileAtThePathOrOnAnotherMachineIsLeftWhereItIs(string where)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string path = Path.Combine(this.directory, "checkout.webm");
        string written = where switch
        {
            "at-the-path" => path,
            "remote" => "/home/grid/videos/screencast-1.webm",
            _ => string.Empty,
        };
        if (where == "at-the-path")
        {
            Directory.CreateDirectory(this.directory);
            File.WriteAllText(path, "video");
        }

        this.AnswerWith(session, written);

        VideoRecording recording = await page.RecordVideoAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        string stopped = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(where == "remote" ? written : path, stopped);
        Assert.Equal(stopped, recording.Path);
    }

    [Fact]
    public async Task ErrorWritingTheVideoIsReported()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.startScreencast", new JsonObject() { ["screencast"] = "screencast-1", ["path"] = string.Empty });
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new JsonObject() { ["path"] = string.Empty, ["error"] = "encoder failed" });
        VideoRecording recording = await page.RecordVideoAsync(Path.Combine(this.directory, "failed.webm"), cancellationToken: TestContext.Current.CancellationToken);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => recording.StopAsync(TestContext.Current.CancellationToken));
        await recording.DisposeAsync();

        Assert.Equal("The browser could not write the video of the page: encoder failed", exception.Message);
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.stopScreencast"));
    }

    [Fact]
    public async Task BrowserThatCannotRecordIsReportedAsUnsupported()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.FailWith("browsingContext.startScreencast", "unsupported operation", "Method browsingContext.startScreencast is not implemented.");

        NotSupportedException unsupported = await Assert.ThrowsAsync<NotSupportedException>(() => page.RecordVideoAsync(Path.Combine(this.directory, "video.webm"), cancellationToken: TestContext.Current.CancellationToken));
        session.RemoteEnd.FailWith("browsingContext.startScreencast", "no such frame", "The page has gone.");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.RecordVideoAsync(Path.Combine(this.directory, "video.webm"), cancellationToken: TestContext.Current.CancellationToken));
        Assert.StartsWith("The browser cannot record video: ", unsupported.Message);
        Assert.Contains("Method browsingContext.startScreencast is not implemented.", unsupported.Message);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private void AnswerWith(FakeSession session, string written)
    {
        session.RemoteEnd.AnswerWith("browsingContext.startScreencast", new JsonObject() { ["screencast"] = "screencast-1", ["path"] = written });
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", new JsonObject() { ["path"] = written });
    }
}
