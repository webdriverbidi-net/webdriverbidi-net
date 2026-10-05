// <copyright file="CodeRecordingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi;

public class CodeRecordingTests
{
    [Fact]
    public async Task EachActionIsWrittenAsItsStatement()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Click("#go"));
        await SendAsync(driver, session, channel, page.Id, Click("#go", button: "right", modifiers: ["Shift"]));
        await SendAsync(driver, session, channel, page.Id, Click("#go", button: "middle", modifiers: ["Alt", "Control", "Meta"]));
        await SendAsync(driver, session, channel, page.Id, Action("check", "#agree"));
        await SendAsync(driver, session, channel, page.Id, Action("uncheck", "#agree"));
        await SendAsync(driver, session, channel, page.Id, Fill("#notes", "\"quoted\" \\ \n\r\t\0\u0001\u2028\u2029 café 😀"));
        await SendAsync(driver, session, channel, page.Id, Press("#notes", "Enter"));
        await SendAsync(driver, session, channel, page.Id, Press("#notes", "a", ["Control"]));
        await SendAsync(driver, session, channel, page.Id, Press("#notes", "F12", ["Shift"]));
        await SendAsync(driver, session, channel, page.Id, Press("#notes", "😀"));
        await SendAsync(driver, session, channel, page.Id, Press("#notes", "ContextMenu"));
        await SendAsync(driver, session, channel, page.Id, Action("select", "#size", ("values", new JsonArray("s", "l"))));
        await SendAsync(driver, session, channel, page.Id, Action("setInputFiles", "#file", ("files", new JsonArray("a.txt", "b \"c\".txt"))));
        await SendAsync(driver, session, channel, page.Id, Action("hover", "#go"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            """
            using Dramaturge;
            using Dramaturge.Browsers;
            using WebDriverBiDi.BrowsingContext;
            using WebDriverBiDi.Input;

            await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.ConfigureFromEnvironment().WithHeadlessOption(false));
            Page page = await group.DefaultBrowser.NewPageAsync();
            await page.Locate(new CssLocator("#go")).ClickAsync();
            await page.Locate(new CssLocator("#go")).ClickAsync(new() { Button = PointerButton.Right, Modifiers = KeyModifiers.Shift });
            await page.Locate(new CssLocator("#go")).ClickAsync(new() { Button = PointerButton.Middle, Modifiers = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Meta });
            await page.Locate(new CssLocator("#agree")).CheckAsync();
            await page.Locate(new CssLocator("#agree")).UncheckAsync();
            await page.Locate(new CssLocator("#notes")).FillAsync("\"quoted\" \\ \n\r\t\0\u0001\u2028\u2029 café 😀");
            await page.Locate(new CssLocator("#notes")).PressAsync(Keys.Enter);
            await page.Locate(new CssLocator("#notes")).PressAsync("a", new() { Modifiers = KeyModifiers.Control });
            await page.Locate(new CssLocator("#notes")).PressAsync(Keys.F12, new() { Modifiers = KeyModifiers.Shift });
            await page.Locate(new CssLocator("#notes")).PressAsync("😀");
            // The key "ContextMenu" was pressed, which is neither a member of Keys nor a character.
            await page.Locate(new CssLocator("#size")).SelectOptionAsync([SelectOption.ByValue("s"), SelectOption.ByValue("l")]);
            await page.Locate(new CssLocator("#file")).SetInputFilesAsync(["a.txt", "b \"c\".txt"]);

            """.Replace("\r\n", "\n"),
            code);
        Assert.Equal(code, recording.Code);
    }

    [Fact]
    public async Task StatementSettlesAtTheNextActionOrTheStop()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);
        List<string> statements = [];
        TaskCompletionSource firstSettled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnStatement.AddObserver(e =>
        {
            statements.Add(e.Statement);
            firstSettled.TrySetResult();
        });

        await SendAsync(driver, session, channel, page.Id, Fill("#name", "A"));
        await SendAsync(driver, session, channel, page.Id, Fill("#name", "Ad"));
        await SendAsync(driver, session, channel, page.Id, Click("#save"));
        await firstSettled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        string settled = recording.Code;
        await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains("FillAsync(\"Ad\");", settled);
        Assert.DoesNotContain("#save", settled);
        Assert.Equal(["await page.Locate(new CssLocator(\"#name\")).FillAsync(\"Ad\");", "await page.Locate(new CssLocator(\"#save\")).ClickAsync();"], statements);
    }

    [Fact]
    public async Task FillsOfOneFieldAndClicksOfOneSeriesAreOneStatement()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Fill("#first", "A"));
        await SendAsync(driver, session, channel, page.Id, Fill("#last", "B"));
        await SendAsync(driver, session, channel, page.Id, Fill("#last", "Bo"));
        await SendAsync(driver, session, channel, page.Id, Click("#cell"));
        await SendAsync(driver, session, channel, page.Id, Click("#cell", clickCount: 2));
        await SendAsync(driver, session, channel, page.Id, Click("#row"));
        await SendAsync(driver, session, channel, page.Id, Click("#row", clickCount: 2));
        await SendAsync(driver, session, channel, page.Id, Click("#row", clickCount: 3));
        await SendAsync(driver, session, channel, page.Id, Click("#row", clickCount: 2));
        await SendAsync(driver, session, channel, page.Id, Click("#row", clickCount: 2, button: "right"));
        await SendAsync(driver, session, channel, page.Id, Click("#cell", clickCount: 3));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#first\")).FillAsync(\"A\");",
                "await page.Locate(new CssLocator(\"#last\")).FillAsync(\"Bo\");",
                "await page.Locate(new CssLocator(\"#cell\")).DblClickAsync();",
                "await page.Locate(new CssLocator(\"#row\")).ClickAsync(new() { ClickCount = 3 });",
                "await page.Locate(new CssLocator(\"#row\")).DblClickAsync();",
                "await page.Locate(new CssLocator(\"#row\")).DblClickAsync(new() { Button = PointerButton.Right });",
                "await page.Locate(new CssLocator(\"#cell\")).ClickAsync(new() { ClickCount = 3 });",
            ],
            Statements(code));
    }

    [Theory]
    [InlineData(CodeTarget.Xunit, "Dramaturge.Xunit", "Xunit", "", "[Fact]")]
    [InlineData(CodeTarget.NUnit, "Dramaturge.NUnit", "NUnit.Framework", "", "[Test]")]
    [InlineData(CodeTarget.MSTest, "Dramaturge.MSTest", "Microsoft.VisualStudio.TestTools.UnitTesting", "[TestClass]\n", "[TestMethod]")]
    [InlineData(CodeTarget.TUnit, "Dramaturge.TUnit", "TUnit.Core", "", "[Test]")]
    public async Task TestTargetIsAPageTestWithOneTest(CodeTarget target, string package, string framework, string classAttribute, string testAttribute)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        CodeRecording recording = await page.Browser.RecordCodeAsync(new CodeRecordingOptions() { Target = target }, TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Action("check", "#agree"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        string[] usings = [.. new[] { "Dramaturge", package, framework, "WebDriverBiDi.BrowsingContext" }.Order(StringComparer.Ordinal).Select(name => $"using {name};")];
        Assert.Equal(
            $$"""
            {{string.Join("\n", usings)}}

            {{classAttribute}}public class RecordedTests : PageTest
            {
                {{testAttribute}}
                public async Task Recorded()
                {
                    Page page = this.Page;
                    await page.Locate(new CssLocator("#agree")).CheckAsync();
                }
            }

            """.Replace("\r\n", "\n"),
            code);
    }

    [Fact]
    public async Task TestIdAttributeOtherThanTheDefaultIsSetInTheFile()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { TestIdAttribute = "data-test" }, TestContext.Current.CancellationToken);

        CodeRecording program = await group.DefaultBrowser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string programCode = await program.StopAsync(TestContext.Current.CancellationToken);
        CodeRecording test = await group.DefaultBrowser.RecordCodeAsync(new CodeRecordingOptions() { Target = CodeTarget.Xunit }, TestContext.Current.CancellationToken);
        string testCode = await test.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains("await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.ConfigureFromEnvironment().WithHeadlessOption(false), new DramaturgeOptions() { TestIdAttribute = \"data-test\" });\n", programCode);
        Assert.Contains("{\n    protected override DramaturgeOptions? GroupOptions => new() { TestIdAttribute = \"data-test\" };\n\n    [Fact]\n", testCode);
    }

    [Theory]
    [InlineData("firefox", "BrowserLauncher.Configure(BrowserKind.Firefox)")]
    [InlineData("chrome", "BrowserLauncher.Configure(BrowserKind.Chrome)")]
    [InlineData("msedge", "BrowserLauncher.Configure(BrowserKind.Edge)")]
    [InlineData("scripted", "BrowserLauncher.ConfigureFromEnvironment()")]
    public async Task ProgramLaunchesTheRecordedBrowser(string browserName, string launcher)
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        server.BrowserName = browserName;
        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(server.Url), cancellationToken: TestContext.Current.CancellationToken);

        await using CodeRecording recording = await group.DefaultBrowser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains($"await BrowserGroup.LaunchAsync({launcher}.WithHeadlessOption(false));\n", recording.Code);
    }

    [Fact]
    public async Task RecordingStartsInEachDocumentOfTheBrowserAndStopsInEachAtTheEnd()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync(testIdAttribute: "data-qa");
        await using BiDiDriver ownedDriver = driver;
        FakeContext frame = await session.CreateFrameAsync(page.Id);
        await WaitUntilAsync(() => page.Frames.Count == 2);
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        await session.RemoteEnd.WaitForCommandAsync("script.callFunction", 2);
        Page popup = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await RaiseDomContentLoadedAsync(session, otherPage.Id);
        await RaiseDomContentLoadedAsync(session, "unknown-context");
        await RaiseDomContentLoadedAsync(session, popup.Id);
        await session.RemoteEnd.WaitForCommandAsync("script.callFunction", 3);
        await recording.StopAsync(TestContext.Current.CancellationToken);

        JsonObject subscription = session.RemoteEnd.CommandsFor("session.subscribe").Last()["params"]!.AsObject();
        Assert.Equal(["script.message"], subscription["events"]!.AsArray().Select(name => (string?)name));
        Assert.Equal([page.Browser.Id], subscription["userContexts"]!.AsArray().Select(id => (string?)id));
        List<JsonObject> calls = [.. session.RemoteEnd.CommandsFor("script.callFunction").Select(call => call["params"]!.AsObject())];
        string?[] contexts = [.. calls.Select(call => (string?)call["target"]!["context"])];
        Assert.Equal(6, calls.Count);
        Assert.Equal([page.Id, frame.Id], contexts[..2].Order());
        Assert.Equal([popup.Id, page.Id, frame.Id, popup.Id], contexts[2..]);
        Assert.All(calls[..3], call => Assert.Contains("recorder.start(send, testIdAttribute)", (string?)call["functionDeclaration"]));
        Assert.All(calls[3..], call => Assert.Contains("recorder.stop()", (string?)call["functionDeclaration"]));
        JsonObject channel = calls[0]["arguments"]![0]!.AsObject();
        Assert.Equal("channel", (string?)channel["type"]);
        Assert.StartsWith("dramaturge-code-", (string?)channel["value"]!["channel"]);
        Assert.Equal(0, (int?)channel["value"]!["serializationOptions"]!["maxDomDepth"]);
        Assert.Equal("none", (string?)channel["value"]!["ownership"]);
        Assert.Equal("data-qa", (string?)calls[0]["arguments"]![1]!["value"]);
        Assert.Single(session.RemoteEnd.CommandsFor("session.unsubscribe"));
    }

    [Fact]
    public async Task MessagesOnlyFromTheBrowsersPagesAreRecorded()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext frame = await session.CreateFrameAsync(page.Id);
        await WaitUntilAsync(() => page.Frames.Count == 2);
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, "another-channel", page.Id, Click("#another-channel"));
        await SendAsync(driver, session, channel, frame.Id, Click("#child-frame"));
        await SendAsync(driver, session, channel, otherPage.Id, Click("#other-browser"));
        await SendAsync(driver, session, channel, "unknown-context", Click("#unknown-context"));
        await SendAsync(driver, session, channel, null, Click("#no-context"));
        await SendAsync(driver, session, channel, page.Id, Click("#recorded"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);
        await SendAsync(driver, session, channel, page.Id, Click("#after-the-stop"));

        Assert.Equal(["await page.Locate(new CssLocator(\"#recorded\")).ClickAsync();"], Statements(code));
        Assert.Equal(code, recording.Code);
    }

    [Fact]
    public async Task AnotherPageIsOpenedBeforeItsFirstStatement()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Page second = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);

        await SendAsync(driver, session, channel, page.Id, Fill("#search", "a"));
        await SendAsync(driver, session, channel, second.Id, Click("#go"));
        await SendAsync(driver, session, channel, page.Id, Click("#back"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#search\")).FillAsync(\"a\");",
                "Page page1 = await page.Browser.NewPageAsync();",
                "await page1.Locate(new CssLocator(\"#go\")).ClickAsync();",
                "await page.Locate(new CssLocator(\"#back\")).ClickAsync();",
            ],
            Statements(code));
    }

    [Fact]
    public async Task BrowserRecordsCodeOnceAtATime()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        CodeRecording first = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken));
        string code = await first.StopAsync(TestContext.Current.CancellationToken);
        string again = await first.StopAsync(TestContext.Current.CancellationToken);
        await first.DisposeAsync();
        await using CodeRecording second = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("The browser is already recording code.", exception.Message);
        Assert.Equal(code, again);
        Assert.Single(session.RemoteEnd.CommandsFor("session.unsubscribe"));
    }

    [Fact]
    public async Task RecordingThatCannotSubscribeDoesNotStart()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int loadObservers = driver.BrowsingContext.OnDomContentLoaded.CurrentObserverCount;
        session.RemoteEnd.FailWith("session.subscribe", "unknown error", "cannot subscribe");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken));
        int messageObserversAfterFailure = driver.Script.OnMessage.CurrentObserverCount;
        int loadObserversAfterFailure = driver.BrowsingContext.OnDomContentLoaded.CurrentObserverCount;
        session.RemoteEnd.AnswerWith("session.subscribe", new JsonObject() { ["subscription"] = "subscription-2" });
        await using CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        await session.RemoteEnd.WaitForCommandAsync("script.callFunction");

        Assert.Equal(0, messageObserversAfterFailure);
        Assert.Equal(loadObservers, loadObserversAfterFailure);
        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task FailuresInTheDocumentsAreReportedAndTheRecordingContinues()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> messages = [];
        page.Browser.Group.OnLogMessage.AddObserver(e => messages.Add(e.Message));
        session.RemoteEnd.FailWith("script.callFunction", "no such frame", "the document closed");
        session.RemoteEnd.FailWith("session.unsubscribe", "unknown error", "cannot unsubscribe");

        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = (string)(await session.RemoteEnd.WaitForCommandAsync("script.callFunction"))["params"]!["arguments"]![0]!["value"]!["channel"]!;
        await SendAsync(driver, session, channel, page.Id, Click("#go"));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["await page.Locate(new CssLocator(\"#go\")).ClickAsync();"], Statements(code));
        Assert.Equal(3, messages.Count);
        Assert.Contains($"Starting the code recording in browsing context {page.Id} failed", messages[0]);
        Assert.Contains("the document closed", messages[0]);
        Assert.Contains("Removing the code recording's event subscription failed", messages[1]);
        Assert.Contains($"Stopping the code recording in browsing context {page.Id} failed", messages[2]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync(string testIdAttribute = "data-testid")
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromSeconds(5), TestIdAttribute = testIdAttribute }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }));
        return (driver, session, page);
    }

    private static async Task<string> GetChannelAsync(FakeSession session)
    {
        JsonObject install = await session.RemoteEnd.WaitForCommandAsync("script.callFunction");
        return (string)install["params"]!["arguments"]![0]!["value"]!["channel"]!;
    }

    // Sends an action as the page's recorder does, and waits until the driver has delivered it.
    private static async Task SendAsync(BiDiDriver driver, FakeSession session, string channel, string? contextId, JsonObject action)
    {
        JsonObject source = new() { ["realm"] = "realm-1" };
        if (contextId is not null)
        {
            source["context"] = contextId;
        }

        await session.RemoteEnd.RaiseEventAsync("script.message", new JsonObject()
        {
            ["channel"] = channel,
            ["data"] = new JsonObject()
            {
                ["type"] = "array",
                ["value"] = new JsonArray(
                    new JsonObject() { ["type"] = "string", ["value"] = action.ToJsonString() },
                    new JsonObject() { ["type"] = "node", ["sharedId"] = "node-1", ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } }),
            },
            ["source"] = source,
        });
        await NetworkEvents.FlushAsync(driver);
    }

    private static Task RaiseDomContentLoadedAsync(FakeSession session, string contextId)
    {
        return session.RemoteEnd.RaiseEventAsync("browsingContext.domContentLoaded", new JsonObject()
        {
            ["context"] = contextId,
            ["navigation"] = "navigation-1",
            ["timestamp"] = 1790000000000,
            ["url"] = "https://example.com/",
        });
    }

    private static JsonObject Action(string kind, string cssPath, params (string Name, JsonNode Value)[] details)
    {
        JsonObject action = new()
        {
            ["kind"] = kind,
            ["target"] = new JsonObject() { ["cssPath"] = cssPath, ["role"] = null },
            ["ancestors"] = new JsonArray(),
        };
        foreach ((string name, JsonNode value) in details)
        {
            action[name] = value;
        }

        return action;
    }

    private static JsonObject Click(string cssPath, string button = "left", int clickCount = 1, string[]? modifiers = null)
    {
        return Action("click", cssPath, ("button", button), ("clickCount", clickCount), ("modifiers", new JsonArray([.. (modifiers ?? []).Select(modifier => JsonValue.Create(modifier))])));
    }

    private static JsonObject Fill(string cssPath, string value) => Action("fill", cssPath, ("value", value));

    private static JsonObject Press(string cssPath, string key, string[]? modifiers = null)
    {
        return Action("press", cssPath, ("key", key), ("modifiers", new JsonArray([.. (modifiers ?? []).Select(modifier => JsonValue.Create(modifier))])));
    }

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
