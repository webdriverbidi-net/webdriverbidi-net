// <copyright file="CodeRecordingEffectsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using static Dramaturge.TestUtilities.RecorderMessages;

// No locator candidate resolves on the fake, so each element is located by its CSS path.
public class CodeRecordingEffectsTests
{
    private const string NavigationStarted = "browsingContext.navigationStarted";

    [Fact]
    public async Task NavigationTheActionCausedIsWaitedFor()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Click("#next"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/next"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/again"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["await page.RunAndWaitForNavigationAsync(() => page.Locate(new CssLocator(\"#next\")).ClickAsync());"], Statements(code));
    }

    [Fact]
    public async Task NavigationNoActionCausedIsANavigation()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext frame = await session.CreateFrameAsync(page.Id);
        await WaitUntilAsync(() => page.Frames.Count == 2);
        Page second = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "about:blank"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, frame.Id, "https://example.com/frame"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, otherPage.Id, "https://example.com/other"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, "unknown-context", "https://example.com/unknown"));
        await SendAsync(driver, session, channel, page.Id, Fill("#search", "a"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, second.Id, "https://example.com/second"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.NavigateAsync(\"https://example.com/\");",
                "await page.Locate(new CssLocator(\"#search\")).FillAsync(\"a\");",
                "Page page1 = await page.Browser.NewPageAsync();",
                "await page1.NavigateAsync(\"https://example.com/second\");",
            ],
            Statements(code));
    }

    [Fact]
    public async Task PopupTheActionOpenedIsItsResult()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Click("#open"));
        Page popup = await OpenPopupAsync(session, page);
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, popup.Id, "https://example.com/popup"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/opener"));
        await SendAsync(driver, session, channel, popup.Id, Click("#inside"));
        await SendAsync(driver, session, channel, popup.Id, new JsonObject() { ["kind"] = "mode", ["mode"] = "record" });
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, popup.Id, "https://example.com/typed"));
        Page unopened = await OpenPopupAsync(session, page);
        await SendAsync(driver, session, channel, unopened.Id, Click("#elsewhere"));
        await popup.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "Page page1 = await page.RunAndWaitForPopupAsync(() => page.Locate(new CssLocator(\"#open\")).ClickAsync());",
                "await page1.Locate(new CssLocator(\"#inside\")).ClickAsync();",
                "await page1.NavigateAsync(\"https://example.com/typed\");",
                "Page page2 = await page.Browser.NewPageAsync();",
                "await page2.Locate(new CssLocator(\"#elsewhere\")).ClickAsync();",
                "await page1.CloseAsync();",
            ],
            Statements(code));
    }

    [Fact]
    public async Task DownloadTheActionStartedIsItsResult()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id)));
        await SendAsync(driver, session, channel, page.Id, Click("#report"));
        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id)));
        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id)));
        await SendAsync(driver, session, channel, page.Id, Click("#export"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/export"));
        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id)));
        await SendAsync(driver, session, channel, page.Id, Click("#archive"));
        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id)));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "Download download = await page.RunAndWaitForDownloadAsync(() => page.Locate(new CssLocator(\"#report\")).ClickAsync());",
                "await page.RunAndWaitForNavigationAsync(() => page.Locate(new CssLocator(\"#export\")).ClickAsync());",
                "Download download1 = await page.RunAndWaitForDownloadAsync(() => page.Locate(new CssLocator(\"#archive\")).ClickAsync());",
            ],
            Statements(code));
    }

    [Fact]
    public async Task DialogIsNotedBeforeTheActionThatOpenedIt()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt(page.Id, "alert", "Hi")));
        await SendAsync(driver, session, channel, page.Id, Fill("#name", "A"));
        await RaiseAsync(driver, session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt(page.Id, "confirm", "Sure?")));
        await SendAsync(driver, session, channel, page.Id, Fill("#name", "Ad"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "// A dialog opened (alert): \"Hi\"",
                "// Opens a dialog (confirm): \"Sure?\"",
                "await page.Locate(new CssLocator(\"#name\")).FillAsync(\"A\");",
                "await page.Locate(new CssLocator(\"#name\")).FillAsync(\"Ad\");",
            ],
            Statements(code));
    }

    [Fact]
    public async Task FramesAreDeclaredByTheirElementsBeforeTheirFirstStatement()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext outer = await session.CreateFrameAsync(page.Id);
        FakeContext inner = await session.CreateFrameAsync(outer.Id);
        FakeContext hidden = await session.CreateFrameAsync(page.Id);
        await WaitUntilAsync(() => page.Frames.Count == 4);
        AnswerFrames(session, new()
        {
            [page.Id] = [(outer.Id, "#outer"), ("unrelated-context", "#unrelated")],
            [outer.Id] = [(inner.Id, "#inner")],
        });
        List<string> messages = [];
        page.Browser.Group.OnLogMessage.AddObserver(e => messages.Add(e.Message));
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Fill("#search", "a"));
        await SendAsync(driver, session, channel, inner.Id, Click("#go"));
        await RaiseAsync(driver, session.RaiseNavigationEventAsync(NavigationStarted, page.Id, "https://example.com/next"));
        await SendAsync(driver, session, channel, outer.Id, Action("assertVisible", "#title"));
        await SendAsync(driver, session, channel, hidden.Id, Click("#hidden"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#search\")).FillAsync(\"a\");",
                "Frame frame = await page.GetByTitle(\"Payment\").ContentFrameAsync();",
                "Frame frame1 = await frame.Locate(new CssLocator(\"#inner\")).ContentFrameAsync();",
                "await page.RunAndWaitForNavigationAsync(() => frame1.Locate(new CssLocator(\"#go\")).ClickAsync());",
                "await Expect(frame.Locate(new CssLocator(\"#title\"))).ToBeVisibleAsync();",
            ],
            Statements(code));
        Assert.Contains(messages, message => message.Contains($"An action in browsing context {hidden.Id} was not recorded: the element of frame {hidden.Id} was not found in its parent."));
    }

    [Fact]
    public async Task FrameWhoseParentCannotDescribeItsFramesIsNotRecorded()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext frame = await session.CreateFrameAsync(page.Id);
        await WaitUntilAsync(() => page.Frames.Count == 2);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);
        session.RemoteEnd.FailWith("script.callFunction", "no such frame", "The document has gone.");

        await SendAsync(driver, session, channel, frame.Id, Click("#go"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Empty(Statements(code));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromSeconds(5) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        AnswerFrames(session, []);
        return (driver, session, page);
    }

    // Answers the recorder's installs, and each document's description of its frame elements: each frame's context
    // and CSS path; #outer also has a title, which no lookup finds, as when the frame has gone, and #inner a nameable ancestor.
    private static void AnswerFrames(FakeSession session, Dictionary<string, (string ContextId, string CssPath)[]> frameElements)
    {
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            if (!((string)parameters["functionDeclaration"]!).Contains("describeFrames"))
            {
                return ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" });
            }

            JsonArray described = [];
            foreach ((string contextId, string cssPath) in frameElements.GetValueOrDefault((string)parameters["target"]!["context"]!, []))
            {
                JsonObject target = new() { ["cssPath"] = cssPath, ["labels"] = new JsonArray() };
                if (cssPath == "#outer")
                {
                    target["title"] = "Payment";
                }

                JsonArray ancestors = cssPath == "#inner" ? [new JsonObject() { ["cssPath"] = "#wrap", ["labels"] = new JsonArray() }] : [];
                JsonObject facts = new() { ["target"] = target, ["ancestors"] = ancestors };
                described.Add(new JsonObject()
                {
                    ["type"] = "array",
                    ["value"] = new JsonArray(
                        new JsonObject() { ["type"] = "window", ["value"] = new JsonObject() { ["context"] = contextId } },
                        new JsonObject() { ["type"] = "string", ["value"] = facts.ToJsonString() },
                        Node($"frame-element-{contextId}")),
                });
            }

            return ProtocolJson.Success(new JsonObject() { ["type"] = "array", ["value"] = described });
        });
    }

    // Opens a page as one the page's script opened, once the recording has seen it.
    private static async Task<Page> OpenPopupAsync(FakeSession session, Page opener)
    {
        TaskCompletionSource<Page> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable observer = opener.Browser.OnPageCreated.AddObserver(e => created.TrySetResult(e.Page));
        FakeContext context = session.AddContext();
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", new JsonObject()
        {
            ["context"] = context.Id,
            ["clientWindow"] = $"window-for-{context.Id}",
            ["originalOpener"] = opener.Id,
            ["url"] = "about:blank",
            ["userContext"] = "default",
            ["children"] = null,
        });
        return await created.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    }

    private static async Task RaiseAsync(BiDiDriver driver, Task raised)
    {
        await raised;
        await NetworkEvents.FlushAsync(driver);
    }

    private static JsonObject WillBegin(string contextId)
    {
        return new JsonObject() { ["context"] = contextId, ["navigation"] = null, ["timestamp"] = 1790000000000, ["url"] = "https://example.com/report.txt", ["suggestedFilename"] = "report.txt", ["download"] = $"download-{Guid.NewGuid():N}" };
    }

    private static JsonObject Prompt(string contextId, string type, string message)
    {
        return new JsonObject() { ["context"] = contextId, ["type"] = type, ["handler"] = "ignore", ["message"] = message, ["userContext"] = "default" };
    }

    private static JsonObject Action(string kind, string cssPath, params (string Name, JsonNode Value)[] details)
    {
        JsonObject action = new()
        {
            ["kind"] = kind,
            ["target"] = new JsonObject() { ["cssPath"] = cssPath, ["labels"] = new JsonArray() },
            ["ancestors"] = new JsonArray(),
        };
        foreach ((string name, JsonNode value) in details)
        {
            action[name] = value;
        }

        return action;
    }

    private static JsonObject Click(string cssPath) => Action("click", cssPath, ("button", "left"), ("clickCount", 1), ("modifiers", new JsonArray()));

    private static JsonObject Fill(string cssPath, string value) => Action("fill", cssPath, ("value", value));

    private static string[] Statements(string programCode)
    {
        return [.. programCode.Split('\n').SkipWhile(line => !line.StartsWith("Page page = ", StringComparison.Ordinal)).Skip(1).Where(line => line.Length > 0)];
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}
