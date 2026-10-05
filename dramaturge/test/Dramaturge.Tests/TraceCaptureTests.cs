// <copyright file="TraceCaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public sealed class TraceCaptureTests : IDisposable
{
    // A baseline JPEG, 100 by 50, with an APP0 segment before its frame segment.
    private static readonly byte[] BaselineJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, .. new byte[14], 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x32, 0x00, 0x64, .. new byte[12], 0xFF, 0xD9];

    // A progressive JPEG, 20 by 10.
    private static readonly byte[] ProgressiveJpeg = [0xFF, 0xD8, 0xFF, 0xC2, 0x00, 0x11, 0x08, 0x00, 0x0A, 0x00, 0x14, .. new byte[12], 0xFF, 0xD9];

    // A JPEG whose segments end before any frame segment.
    private static readonly byte[] FramelessJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, .. new byte[14]];

    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-capture-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Fact]
    public async Task SnapshotsOfEveryFrameAreTakenBeforeAfterAndAsAnActionActsWithItsPoint()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id, "https://example.com/child");
        await NetworkEvents.FlushAsync(driver);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, page.Id, child.Id, failChild: false);
        ElementLocator button = page.Locate(new CssLocator("button"));
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Snapshots = true }, TestContext.Current.CancellationToken))
        {
            await button.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
            await button.DragToAsync(page.Locate(new CssLocator("#drop")), cancellationToken: TestContext.Current.CancellationToken);
            await button.SetInputFilesAsync(["/tmp/a.txt"], cancellationToken: TestContext.Current.CancellationToken);
            await button.DispatchEventAsync("click", cancellationToken: TestContext.Current.CancellationToken);
        }

        List<JsonObject> events = ReadLines(path);
        string click = (string)events.First(e => (string?)e["type"] == "before")["callId"]!;
        List<JsonNode> snapshots = [.. events.Where(e => (string?)e["type"] == "frame-snapshot" && (string?)e["snapshot"]!["callId"] == click).Select(e => e["snapshot"]!)];
        Assert.Equal(["before:main", "before:child", "action:main", "action:child", "after:main", "after:child"], snapshots.Select(s => $"{s["phase"]}:{((bool)s["isMainFrame"]! ? "main" : "child")}"));
        JsonNode main = snapshots[2];
        Assert.Equal(page.Id, (string?)main["frameId"]);
        Assert.Equal(page.Id, (string?)main["pageId"]);
        Assert.Equal("html", (string?)main["doctype"]);
        Assert.Equal($$"""["HTML",{},["BODY",{},["IFRAME",{"src":"/snapshot/{{child.Id}}"}],["IFRAME",{"src":""}]]]""", main["html"]!.ToJsonString());
        Assert.Equal("""{"width":800,"height":600}""", main["viewport"]!.ToJsonString());
        Assert.Equal("[]", main["resourceOverrides"]!.ToJsonString());
        Assert.Equal(1790000000000, (double?)main["wallTime"]);
        Assert.Equal(1.5, (double?)main["collectionTime"]);
        JsonNode childSnapshot = snapshots[3];
        Assert.Equal(child.Id, (string?)childSnapshot["frameId"]);
        Assert.Equal("https://example.com/child", (string?)childSnapshot["frameUrl"]);
        Assert.Null(childSnapshot["doctype"]);
        Assert.Equal("""{"x":11,"y":21}""", events.Single(e => (string?)e["type"] == "input" && (string?)e["callId"] == click)["point"]!.ToJsonString());
        List<string> actionPhases = [.. events.Where(e => (string?)e["type"] == "frame-snapshot" && (string?)e["snapshot"]!["phase"] == "action" && (bool)e["snapshot"]!["isMainFrame"]!).Select(e => (string)e["snapshot"]!["callId"]!)];
        Assert.Equal(4, actionPhases.Count);
        Assert.Equal(2, events.Count(e => (string?)e["type"] == "input"));
        IReadOnlyList<JsonObject> targeted = [.. ScriptCalls(session, "snapshots.snapshot").Where(call => call["params"]!["arguments"]![0]!["sharedId"] is not null)];
        Assert.Equal(4, targeted.Count);
        Assert.All(targeted, call => Assert.Equal(page.Id, (string?)call["params"]!["target"]!["context"]));
        Assert.Equal("""{"type":"object","value":[["x",{"type":"number","value":1}],["y",{"type":"number","value":1}]]}""", targeted[0]["params"]!["arguments"]![1]!.ToJsonString());
        Assert.Equal("null", (string?)targeted[2]["params"]!["arguments"]![1]!["type"]);
    }

    [Fact]
    public async Task FrameThatCannotBeCapturedIsLeftOutAndTheActionRuns()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        await NetworkEvents.FlushAsync(driver);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, page.Id, child.Id, failChild: true);
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Snapshots = true }, TestContext.Current.CancellationToken))
        {
            await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(["before", "after"], ReadLines(path).Where(e => (string?)e["type"] == "frame-snapshot").Select(e => (string?)e["snapshot"]!["phase"]));
    }

    [Fact]
    public async Task ScreenshotsAfterActionsAndLoadsMakeTheFilmstrip()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        Queue<byte[]> images = new([BaselineJpeg, ProgressiveJpeg, FramelessJpeg, [0x89, 0x50, 0x4E, 0x47]]);
        session.RemoteEnd.AnswerWith("browsingContext.captureScreenshot", _ => new JsonObject() { ["data"] = Convert.ToBase64String(images.Dequeue()) });
        ElementLocator button = page.Locate(new CssLocator("button"));
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Screenshots = true }, TestContext.Current.CancellationToken))
        {
            await button.CountAsync(TestContext.Current.CancellationToken);
            await session.RaiseNavigationEventAsync("browsingContext.load", page.Id, "https://example.com/next");
            await session.RaiseNavigationEventAsync("browsingContext.load", child.Id, "https://example.com/frame");
            await session.RaiseNavigationEventAsync("browsingContext.load", otherPage.Id, "https://example.com/other");
            await session.RaiseNavigationEventAsync("browsingContext.load", "unknown-context", "https://example.com/unknown");
            await NetworkEvents.FlushAsync(driver);
            await button.CountAsync(TestContext.Current.CancellationToken);
            await button.CountAsync(TestContext.Current.CancellationToken);
            session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "no such frame", "The page is gone.");
            await button.CountAsync(TestContext.Current.CancellationToken);
        }

        List<JsonObject> frames = [.. ReadLines(path).Where(e => (string?)e["type"] == "screencast-frame")];
        Assert.Equal(["100x50", "20x10", "0x0", "0x0"], frames.Select(e => $"{e["width"]}x{e["height"]}"));
        Assert.All(frames, frame => Assert.Equal(page.Id, (string?)frame["pageId"]));
        Assert.Equal(BaselineJpeg, ReadEntry(path, (string)frames[0]["file"]!));
        Assert.StartsWith($"screencast/{page.Id}-", (string?)frames[0]["file"]);
        Assert.EndsWith(".jpeg", (string?)frames[0]["file"]);
        Assert.Equal("image/jpeg", (string?)session.RemoteEnd.CommandsFor("browsingContext.captureScreenshot")[0]["params"]!["format"]!["type"]);
    }

    [Fact]
    public async Task SourcesOfTheCallersOnThisMachineAreIncluded()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        ElementLocator button = page.Locate(new CssLocator("button"));
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Sources = true }, TestContext.Current.CancellationToken))
        {
            await button.CountAsync(TestContext.Current.CancellationToken);
            await CountElsewhere(button);
        }

        string thisFile = ThisFile();
        using ZipArchive zip = ZipFile.OpenRead(path);
        string[] sources = [.. zip.Entries.Select(entry => entry.FullName).Where(name => name.StartsWith("src/", StringComparison.Ordinal))];
        string expected = $"src/{Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(thisFile))).ToLowerInvariant()}.cs";
        Assert.Equal([expected], sources);
        Assert.Equal(File.ReadAllBytes(thisFile), ReadEntry(path, expected));
    }

    [Fact]
    public async Task SourcesAreLeftOutUnlessAsked()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);
        }

        using ZipArchive zip = ZipFile.OpenRead(path);
        Assert.DoesNotContain(zip.Entries, entry => entry.FullName.StartsWith("src/", StringComparison.Ordinal));
    }

    private static string ThisFile([CallerFilePath] string file = "") => file;

    // Its frame names a source file this machine does not have.
    private static Task<int> CountElsewhere(ElementLocator locator)
    {
#line 1 "/nonexistent/Elsewhere.cs"
        return locator.CountAsync(TestContext.Current.CancellationToken);
#line default
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromSeconds(10) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    // Answers readiness checks as ready, snapshots as a page with two frames, one of them gone, and every other script
    // with undefined.
    private static void AnswerScripts(FakeSession session, string pageId, string childId, bool failChild)
    {
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            string function = (string)parameters["functionDeclaration"]!;
            string context = (string)parameters["target"]!["context"]!;
            if (function.Contains("isInteractionReady"))
            {
                return ProtocolJson.Success(Object(("status", String("ready")), ("interactionOffset", Object(("x", Number(1)), ("y", Number(1))))));
            }

            if (function.Contains("dispatchEvent"))
            {
                return ProtocolJson.Boolean(true);
            }

            if (!function.Contains("snapshots.snapshot"))
            {
                return ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" });
            }

            if (context == childId)
            {
                return failChild
                    ? ProtocolJson.Exception("TypeError: Acquiescence.DomSnapshotGenerator is not a constructor")
                    : ProtocolJson.Success(Object(("json", String("""{"html":["HTML"],"viewport":{"width":300,"height":150},"url":"https://example.com/child","wallTime":1790000000001,"collectionTime":0.5}""")), ("frames", new JsonObject() { ["type"] = "array", ["value"] = new JsonArray() }), ("point", new JsonObject() { ["type"] = "null" })));
            }

            bool targeted = parameters["arguments"]![0]!["sharedId"] is not null;
            return ProtocolJson.Success(Object(
                ("json", String("""{"doctype":"html","html":["HTML",{},["BODY",{},["IFRAME",{"src":"/snapshot/@0"}],["IFRAME",{"src":"/snapshot/@1"}]]],"viewport":{"width":800,"height":600},"url":"https://example.com/","wallTime":1790000000000,"collectionTime":1.5}""")),
                ("frames", new JsonObject() { ["type"] = "array", ["value"] = new JsonArray(new JsonObject() { ["type"] = "window", ["value"] = new JsonObject() { ["context"] = childId } }, new JsonObject() { ["type"] = "null" }) }),
                ("point", targeted && parameters["arguments"]![1]!["type"]?.GetValue<string>() == "object" ? Object(("x", Number(11)), ("y", Number(21))) : new JsonObject() { ["type"] = "null" })));
        });
    }

    private static JsonObject Object(params (string Name, JsonObject Value)[] properties)
    {
        return new JsonObject() { ["type"] = "object", ["value"] = new JsonArray([.. properties.Select(property => (JsonNode?)new JsonArray(property.Name, property.Value))]) };
    }

    private static JsonObject String(string value) => new() { ["type"] = "string", ["value"] = value };

    private static JsonObject Number(double value) => new() { ["type"] = "number", ["value"] = value };

    private static IReadOnlyList<JsonObject> ScriptCalls(FakeSession session, string functionFragment)
    {
        return [.. session.RemoteEnd.CommandsFor("script.callFunction").Where(command => ((string)command["params"]!["functionDeclaration"]!).Contains(functionFragment))];
    }

    private static List<JsonObject> ReadLines(string path)
    {
        return [.. Encoding.UTF8.GetString(ReadEntry(path, "trace.trace")).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!.AsObject())];
    }

    private static byte[] ReadEntry(string path, string entry)
    {
        using ZipArchive zip = ZipFile.OpenRead(path);
        using MemoryStream content = new();
        using (Stream stream = zip.GetEntry(entry)!.Open())
        {
            stream.CopyTo(content);
        }

        return content.ToArray();
    }
}
