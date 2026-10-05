// <copyright file="CodeRecordingLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;

// The element acted on is node-1; the fake finds it alone with one kind of locator, and among others with the rest.
public class CodeRecordingLocatorTests
{
    private const string Target = "node-1";

    [Theory]
    [InlineData("testId", true, "page.GetByTestId(\"save\")")]
    [InlineData("role+name", true, "page.GetByRole(\"button\", \"Save\")")]
    [InlineData("label", false, "page.GetByLabel(\"Email\")")]
    [InlineData("label", true, "page.GetByLabel(\"Email\", exact: true)")]
    [InlineData("placeholder", false, "page.GetByPlaceholder(\"you@example.com\")")]
    [InlineData("placeholder", true, "page.GetByPlaceholder(\"you@example.com\", exact: true)")]
    [InlineData("alt", false, "page.GetByAltText(\"Logo\")")]
    [InlineData("alt", true, "page.GetByAltText(\"Logo\", exact: true)")]
    [InlineData("title", false, "page.GetByTitle(\"Tip\")")]
    [InlineData("title", true, "page.GetByTitle(\"Tip\", exact: true)")]
    [InlineData("text", false, "page.GetByText(\"Save now\")")]
    [InlineData("text", true, "page.GetByText(\"Save now\", exact: true)")]
    [InlineData("role", true, "page.GetByRole(\"button\")")]
    public async Task FirstCandidateFindingTheElementAloneIsChosen(string kind, bool exact, string expected)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, request => request.Kind == kind && request.Exact == exact ? [Target] : [Target, "node-9"]);

        string statement = await RecordClickAsync(driver, session, page, Facts());

        Assert.Equal($"await {expected}.ClickAsync();", statement);
    }

    [Fact]
    public async Task ElementFoundOnlyAmongOthersIsTheFirstCandidatesMatchAtItsIndex()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, request => request.Kind == "testId" ? ["node-9", Target] : request.Kind == "role" ? ["node-8", "node-9", Target] : []);

        string statement = await RecordClickAsync(driver, session, page, Facts());

        Assert.Equal("await page.GetByTestId(\"save\").Nth(1).ClickAsync();", statement);
    }

    [Fact]
    public async Task ElementIsFoundWithinTheNearestAncestorFoundAlone()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject form = new() { ["role"] = "form", ["name"] = "Sign in", ["cssPath"] = "#login", ["labels"] = new JsonArray() };
        JsonObject panel = new() { ["testId"] = "panel", ["role"] = "generic", ["cssPath"] = "#main > div", ["labels"] = new JsonArray() };
        Answer(session, request => request.Scoped
            ? (request.Kind == "role" ? [Target] : [Target, "node-9"])
            : request.Value.Contains("panel") ? ["node-3"] : request.Value.Contains("Sign in") || request.Value == "#login" ? [] : [Target, "node-9"]);

        string statement = await RecordClickAsync(driver, session, page, Facts(), [form, panel], [Target, "node-2", "node-3"]);

        Assert.Equal("await page.GetByTestId(\"panel\").GetByRole(\"button\").ClickAsync();", statement);
    }

    [Fact]
    public async Task AncestorsThatAreNotFoundAloneOrDoNotNarrowTheSearchAreSkipped()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject plain = new() { ["testId"] = "row", ["cssPath"] = "section > div", ["labels"] = new JsonArray() };
        JsonObject unnamed = new() { ["role"] = "generic", ["cssPath"] = "#main > div", ["labels"] = new JsonArray() };
        JsonObject side = new() { ["role"] = "region", ["name"] = string.Empty, ["cssPath"] = "#side", ["labels"] = new JsonArray() };
        Answer(session, request => request.Value == "#side" ? ["node-4"] : [Target, "node-9"]);

        string statement = await RecordClickAsync(driver, session, page, Facts(), [plain, unnamed, side], [Target, "node-2", "node-3", "node-4"]);

        Assert.Equal("await page.GetByTestId(\"save\").Nth(0).ClickAsync();", statement);
    }

    [Fact]
    public async Task ElementNoCandidateFindsIsLocatedByItsCssPath()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, _ => [], connected: true);

        string statement = await RecordClickAsync(driver, session, page, Facts(), [new JsonObject() { ["role"] = "generic", ["cssPath"] = "#main > div", ["labels"] = new JsonArray() }], [Target, "node-2"]);

        Assert.Equal("await page.Locate(new CssLocator(\"#save\")).ClickAsync();", statement);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task ElementGoneFromItsDocumentIsLocatedByItsFirstCandidate(bool? connected)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, _ => [], connected);

        string statement = await RecordClickAsync(driver, session, page, Facts());

        Assert.Equal("await page.GetByTestId(\"save\").ClickAsync();", statement);
    }

    [Theory]
    [InlineData("Save the changes you have made to this document before closing it, or they will be lost forever", "Save the changes you have made to this document before closing it, or they will")]
    [InlineData("Unbroken-text-that-runs-on-and-on-without-a-single-space-to-cut-it-at-anywhere-at-all-in-it", "Unbroken-text-that-runs-on-and-on-without-a-single-space-to-cut-it-at-anywhere-a")]
    public async Task LongTextIsCutAndMatchedInPart(string text, string shortened)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, request => request.Kind == "text" ? [Target] : []);

        string statement = await RecordClickAsync(driver, session, page, new JsonObject() { ["text"] = text, ["cssPath"] = "p", ["labels"] = new JsonArray() });

        Assert.Equal($"await page.GetByText(\"{shortened}\").ClickAsync();", statement);
        Assert.DoesNotContain(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"), command => (string?)command["params"]!["locator"]!["matchType"] == "full");
    }

    [Fact]
    public async Task CandidateTheBrowserCannotResolveIsSkipped()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => (string?)parameters["locator"]!["type"] == "innerText"
            ? FakeResponse.Failure("unsupported operation", "innerText locators are not supported")
            : new FakeResponse(ProtocolJson.Nodes(Target)));

        string statement = await RecordClickAsync(driver, session, page, new JsonObject() { ["text"] = "Go", ["role"] = "button", ["name"] = string.Empty, ["cssPath"] = "#go", ["labels"] = new JsonArray() });

        Assert.Equal("await page.GetByRole(\"button\").ClickAsync();", statement);
    }

    [Fact]
    public async Task ConsecutiveActionsOnOneElementShareItsLocator()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Page second = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        Answer(session, request => request.Kind == "testId" ? [Target] : []);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);
        TaskCompletionSource settled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnStatement.AddObserver(_ => settled.TrySetResult());

        await SendAsync(driver, session, channel, page.Id, Action("fill", Facts(), ("value", "A")));
        await SendAsync(driver, session, channel, page.Id, Action("fill", Facts(), ("value", "Ad")));
        await SendAsync(driver, session, channel, page.Id, new JsonObject() { ["kind"] = "mode", ["mode"] = "record" });
        await settled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        int lookups = session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count;
        await SendAsync(driver, session, channel, second.Id, Action("fill", Facts(), ("value", "B")));
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, lookups);
        Assert.Equal(2, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
        Assert.Contains("await page.GetByTestId(\"save\").FillAsync(\"Ad\");\n", code);
        Assert.Contains("await page1.GetByTestId(\"save\").FillAsync(\"B\");\n", code);
        Assert.DoesNotContain("using WebDriverBiDi.BrowsingContext;", code);
    }

    [Fact]
    public async Task PickedLocatorIsTheGeneratedOne()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Answer(session, request => request.Kind == "role+name" ? [Target] : [Target, "node-9"]);
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);
        TaskCompletionSource<LocatorPickedEventArgs> picked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnLocatorPicked.AddObserver(e => picked.TrySetResult(e));

        await SendAsync(driver, session, channel, page.Id, Action("pick", Facts()));
        LocatorPickedEventArgs pick = await picked.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal("page.GetByRole(\"button\", \"Save\")", pick.Code);
        Assert.Equal(page.GetByRole("button", "Save").ToString(), pick.Locator.ToString());
        Assert.DoesNotContain("using WebDriverBiDi.BrowsingContext;", code);
    }

    private static JsonObject Facts()
    {
        return new JsonObject()
        {
            ["testId"] = "save",
            ["role"] = "button",
            ["name"] = "Save",
            ["labels"] = new JsonArray("Email", string.Empty),
            ["placeholder"] = "you@example.com",
            ["alt"] = "Logo",
            ["title"] = "Tip",
            ["text"] = "Save now",
            ["cssPath"] = "#save",
            ["id"] = null,
        };
    }

    // Answers each lookup with the shared IDs of the elements it finds, and the connection check, which fails when not given.
    private static void Answer(FakeSession session, Func<Request, string[]> find, bool? connected = true)
    {
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => ProtocolJson.Nodes(find(Classify(parameters))));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            string function = (string)parameters["functionDeclaration"]!;
            JsonArray arguments = parameters["arguments"]!.AsArray();
            if (function.Contains("findElementsByLabel"))
            {
                Request request = new("label", (bool)arguments[1]!["value"]!, arguments.Count > 2, (string)arguments[0]!["value"]!);
                return new FakeResponse(ProtocolJson.Success(new JsonObject() { ["type"] = "array", ["value"] = ProtocolJson.Nodes(find(request))["nodes"]!.DeepClone() }));
            }

            if (function.Contains("element.isConnected"))
            {
                return connected is bool value ? new FakeResponse(ProtocolJson.Boolean(value)) : FakeResponse.Failure("no such node", "The document has gone.");
            }

            return new FakeResponse(ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }));
        });
    }

    private static Request Classify(JsonObject parameters)
    {
        JsonObject locator = parameters["locator"]!.AsObject();
        bool scoped = parameters["startNodes"] is JsonArray { Count: > 0 };
        switch ((string)locator["type"]!)
        {
            case "accessibility":
                return new(locator["value"]!["name"] is null ? "role" : "role+name", true, scoped, locator["value"]!.ToJsonString());
            case "innerText":
                return new("text", (string?)locator["matchType"] == "full", scoped, (string)locator["value"]!);
            default:
                string selector = (string)locator["value"]!;
                string kind = selector.StartsWith("[data-testid", StringComparison.Ordinal) ? "testId"
                    : selector.StartsWith("[placeholder", StringComparison.Ordinal) ? "placeholder"
                    : selector.StartsWith("[alt", StringComparison.Ordinal) ? "alt"
                    : selector.StartsWith("[title", StringComparison.Ordinal) ? "title"
                    : "css";
                return new(kind, !selector.EndsWith(" i]", StringComparison.Ordinal), scoped, selector);
        }
    }

    private static async Task<string> RecordClickAsync(BiDiDriver driver, FakeSession session, Page page, JsonObject facts, JsonObject[]? ancestors = null, string[]? elementIds = null)
    {
        CodeRecording recording = await page.Browser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        string channel = await GetChannelAsync(session);
        JsonObject click = Action("click", facts, ("button", "left"), ("clickCount", 1), ("modifiers", new JsonArray()));
        click["ancestors"] = new JsonArray([.. (ancestors ?? []).Select(ancestor => (JsonNode)ancestor)]);
        await SendAsync(driver, session, channel, page.Id, click, elementIds ?? [Target]);
        string code = await recording.StopAsync(TestContext.Current.CancellationToken);
        return code.Split('\n').Single(line => line.Contains("ClickAsync"));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromSeconds(5) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static async Task<string> GetChannelAsync(FakeSession session)
    {
        JsonObject install = await session.RemoteEnd.WaitForCommandAsync("script.callFunction");
        return (string)install["params"]!["arguments"]![0]!["value"]!["channel"]!;
    }

    private static JsonObject Action(string kind, JsonObject facts, params (string Name, JsonNode Value)[] details)
    {
        JsonObject action = new() { ["kind"] = kind, ["target"] = facts, ["ancestors"] = new JsonArray() };
        foreach ((string name, JsonNode value) in details)
        {
            action[name] = value;
        }

        return action;
    }

    private static async Task SendAsync(BiDiDriver driver, FakeSession session, string channel, string contextId, JsonObject action, params string[] elementIds)
    {
        JsonArray data = [new JsonObject() { ["type"] = "string", ["value"] = action.ToJsonString() }];
        foreach (string elementId in elementIds.Length == 0 ? [Target] : elementIds)
        {
            data.Add(new JsonObject() { ["type"] = "node", ["sharedId"] = elementId, ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } });
        }

        await session.RemoteEnd.RaiseEventAsync("script.message", new JsonObject()
        {
            ["channel"] = channel,
            ["data"] = new JsonObject() { ["type"] = "array", ["value"] = data },
            ["source"] = new JsonObject() { ["realm"] = "realm-1", ["context"] = contextId },
        });
        await NetworkEvents.FlushAsync(driver);
    }

    // A lookup the recording made: the kind of locator, whether it matches exactly, whether it searches within other elements, and its value.
    private sealed record Request(string Kind, bool Exact, bool Scoped, string Value);
}
