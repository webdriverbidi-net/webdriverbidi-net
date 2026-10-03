// <copyright file="AriaSnapshotTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class AriaSnapshotTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void OptionsIncludeRefsAndFramesByDefault()
    {
        AriaSnapshotOptions options = new();

        Assert.True(options.IncludeRefs);
        Assert.True(options.IncludeFrames);
    }

    [Fact]
    public async Task PageSnapshotIsTakenByTheLibraryInTheSandbox()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", Snapshot(Node("heading", "Title", children: [], level: 1, reference: "e1"), "- heading \"Title\" [level=1] [ref=e1]", ("e1", "h1-1")));

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("- heading \"Title\" [level=1] [ref=e1]", snapshot.ToString());
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Contains("snapshots.snapshot(root, includeRefs, refPrefix)", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Browser.Group.Options.SandboxName, (string?)call["target"]!["sandbox"]);
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        Assert.Equal("null", (string?)call["arguments"]![0]!["type"]);
        Assert.True((bool?)call["arguments"]![1]!["value"]);
        Assert.Equal(string.Empty, (string?)call["arguments"]![2]!["value"]);
    }

    [Fact]
    public async Task TreeHoldsEveryNodeWithItsStates()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject tree = Node(
            "fragment",
            string.Empty,
            [
                Node("treeitem", "Item", ["Some text"], reference: "e1", @checked: true, disabled: false, expanded: true, level: 2, pressed: "mixed", selected: true),
                Node("checkbox", "Off", [], @checked: false, pressed: false),
                Node("link", "Home", [], url: "/home"),
                Node("textbox", "Email", [], placeholder: "you@example.com"),
            ]);
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(tree, "text"));

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeRefs = false }, TestContext.Current.CancellationToken);

        Assert.Equal(AriaNode.FragmentRole, snapshot.Root.Role);
        AriaNode item = snapshot.Root.Children[0];
        Assert.Equal(("treeitem", "Item", "e1"), (item.Role, item.Name, item.Ref));
        Assert.Equal((ToggleState.On, false, true, 2, ToggleState.Mixed, true), (item.Checked, item.Disabled, item.Expanded, item.Level, item.Pressed, item.Selected));
        AriaNode text = Assert.Single(item.Children);
        Assert.Equal((AriaNode.TextRole, string.Empty, "Some text", null), (text.Role, text.Name, text.Text, text.Ref));
        AriaNode checkbox = snapshot.Root.Children[1];
        Assert.Equal((ToggleState.Off, ToggleState.Off, null, null, null, null), (checkbox.Checked, checkbox.Pressed, checkbox.Disabled, checkbox.Level, checkbox.Text, checkbox.Url));
        Assert.Equal("/home", snapshot.Root.Children[2].Url);
        Assert.Equal("you@example.com", snapshot.Root.Children[3].Placeholder);
        Assert.False((bool?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["arguments"]![1]!["value"]);
    }

    [Fact]
    public async Task FramesArePutBeneathTheirIframeNodes()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        Frame child = page.Frames[1];
        JsonObject mainTree = Node("fragment", string.Empty, [Node("main", string.Empty, [Node("iframe", string.Empty, [], reference: "e2"), Node("iframe", string.Empty, [], reference: "e3")], reference: "e1")]);
        string mainText = "- main [ref=e1]:\n  - iframe [ref=e2]\n  - iframe [ref=e3]";
        JsonObject childTree = Node("fragment", string.Empty, [Node("button", "OK", [], reference: "f1e1")]);
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => (string?)parameters["target"]!["context"] == child.Id
            ? SnapshotOf(childTree, "- button \"OK\" [ref=f1e1]", ("f1e1", "button-1"))
            : SnapshotOf(mainTree, mainText, [Window(child.Id), Null()], ("e1", "main-1"), ("e2", "iframe-1"), ("e3", "iframe-2")));

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("- main [ref=e1]:\n  - iframe [ref=e2]:\n    - button \"OK\" [ref=f1e1]\n  - iframe [ref=e3]", snapshot.ToString());
        AriaNode main = Assert.Single(snapshot.Root.Children);
        Assert.Equal("button", Assert.Single(main.Children[0].Children).Role);
        Assert.Empty(main.Children[1].Children);
        Assert.Equal("f1", (string?)session.RemoteEnd.CommandsFor("script.callFunction")[1]["params"]!["arguments"]![2]!["value"]);
        Assert.Same(child, snapshot.Locator("f1e1").Frame);
        Assert.Same(page.MainFrame, snapshot.Locator("e3").Frame);
    }

    [Fact]
    public async Task FramesAreLeftEmptyWhenNotIncluded()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, [Node("iframe", string.Empty, [])]), "- iframe", [Window(page.Frames[1].Id)]));

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeFrames = false }, TestContext.Current.CancellationToken);

        Assert.Equal("- iframe", snapshot.ToString());
        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task FrameNotYetTrackedIsWaitedForUntilTheBudgetIsSpent()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync(TimeSpan.FromSeconds(1));
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, [Node("iframe", string.Empty, [])]), "- iframe", [Window("untracked")]));

        Task<AriaSnapshot> snapshot = page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, snapshot));

        Assert.Equal("Timed out after 1 seconds waiting for the frame untracked to be tracked.", exception.Message);
    }

    [Fact]
    public async Task FrameKeepsItsRefPrefixFromOneSnapshotToTheNext()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext second = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("body-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, []), string.Empty));
        Frame secondFrame = page.Frames.Single(frame => frame.Id == second.Id);

        await secondFrame.Locate(new CssLocator("body")).AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Frames[1].Locate(new CssLocator("body")).AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        await secondFrame.Locate(new CssLocator("body")).AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["f1", "f2", "f1"], session.RemoteEnd.CommandsFor("script.callFunction").Select(call => (string?)call["params"]!["arguments"]![2]!["value"]));
    }

    [Fact]
    public async Task LocatorSnapshotWaitsForOneElementAndStartsThere()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] locates = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => Interlocked.Increment(ref locates[0]) == 1 ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("main-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, [Node("main", string.Empty, [])]), "- main"));

        AriaSnapshot snapshot = await DriveAsync(time, page.Locate(new CssLocator("main")).AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeRefs = false }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("- main", snapshot.ToString());
        Assert.Equal("main-1", (string?)Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!["arguments"]![0]!["sharedId"]);
    }

    [Fact]
    public async Task LocatorSnapshotOfSeveralElementsIsAmbiguous()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("p")).AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RefLocatorFindsItsElementWhileItIsInItsDocument()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        bool[] connected = [true];
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ((string?)parameters["functionDeclaration"])!.Contains("isConnected")
            ? ProtocolJson.Success(connected[0] ? Element("button-1") : new JsonObject() { ["type"] = "null" })
            : SnapshotOf(Node("fragment", string.Empty, [Node("button", "Go", [], reference: "e1")]), "- button \"Go\" [ref=e1]", ("e1", "button-1")));
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator button = snapshot.Locator("e1");

        int whileConnected = await button.CountAsync(TestContext.Current.CancellationToken);
        connected[0] = false;
        int afterRemoval = await button.CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal((1, 0), (whileConnected, afterRemoval));
        Assert.Equal("ref e1", button.ToString());
        JsonObject check = session.RemoteEnd.CommandsFor("script.callFunction")[1]["params"]!.AsObject();
        Assert.Equal("(element) => element.isConnected ? element : null", (string?)check["functionDeclaration"]);
        Assert.Equal("button-1", (string?)check["arguments"]![0]!["sharedId"]);
        Assert.Equal(page.Browser.Group.Options.SandboxName, (string?)check["target"]!["sandbox"]);
    }

    [Fact]
    public async Task RefLocatorFailsAtOnceWhenItsDocumentIsGone()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, [Node("button", "Go", [], reference: "e1")]), "- button \"Go\" [ref=e1]", ("e1", "button-1")));
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("script.callFunction", "no such node", "belongs to a different document");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => snapshot.Locator("e1").ClickAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("The element of ref e1 is no longer in its document, which has been replaced or discarded.", exception.Message);
        Assert.IsType<WebDriverBiDiCommandException>(exception.InnerException);
    }

    [Fact]
    public async Task UnknownRefIsRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", SnapshotOf(Node("fragment", string.Empty, []), string.Empty));
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => snapshot.Locator("e7"));

        Assert.Equal("reference", exception.ParamName);
        Assert.StartsWith("The snapshot has no ref \"e7\".", exception.Message);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync(TimeSpan? actionTimeout = null)
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        DramaturgeOptions options = new() { PollInterval = PollInterval, TimeProvider = time, ActionTimeout = actionTimeout ?? TimeSpan.FromSeconds(30) };
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, options, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageWithFrameAsync()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // A node of the tree the library returns as JSON; a string child is a run of text.
    private static JsonObject Node(string role, string name, JsonNode[] children, string? reference = null, bool? @checked = null, bool? disabled = null, bool? expanded = null, int? level = null, object? pressed = null, bool? selected = null, string? url = null, string? placeholder = null)
    {
        JsonObject node = new() { ["role"] = role, ["name"] = name, ["children"] = new JsonArray([.. children.Select(child => child.DeepClone())]) };
        AddIfSet(node, "ref", reference);
        AddIfSet(node, "checked", @checked);
        AddIfSet(node, "disabled", disabled);
        AddIfSet(node, "expanded", expanded);
        AddIfSet(node, "level", level);
        AddIfSet(node, "pressed", pressed is string text ? JsonValue.Create(text) : pressed is bool value ? JsonValue.Create(value) : null);
        AddIfSet(node, "selected", selected);
        AddIfSet(node, "url", url);
        AddIfSet(node, "placeholder", placeholder);
        return node;
    }

    private static void AddIfSet<T>(JsonObject node, string name, T? value)
    {
        if (value is not null)
        {
            node[name] = value is JsonNode json ? json : JsonValue.Create(value);
        }
    }

    private static JsonObject Snapshot(JsonObject node, string text, params (string Ref, string SharedId)[] references)
    {
        return SnapshotOf(Node("fragment", string.Empty, [node]), text, references);
    }

    private static JsonObject SnapshotOf(JsonObject tree, string text, params (string Ref, string SharedId)[] references)
    {
        return SnapshotOf(tree, text, [], references);
    }

    // The value the library's snapshot function returns, as the protocol serializes it.
    private static JsonObject SnapshotOf(JsonObject tree, string text, JsonObject[] frames, params (string Ref, string SharedId)[] references)
    {
        JsonArray properties =
        [
            Property("tree", new JsonObject() { ["type"] = "string", ["value"] = tree.ToJsonString() }),
            Property("text", new JsonObject() { ["type"] = "string", ["value"] = text }),
            Property("refs", Array(references.Select(reference => new JsonObject() { ["type"] = "string", ["value"] = reference.Ref }))),
            Property("elements", Array(references.Select(reference => Element(reference.SharedId)))),
            Property("frames", Array(frames)),
        ];
        return ProtocolJson.Success(new JsonObject() { ["type"] = "object", ["value"] = properties });
    }

    private static JsonArray Property(string name, JsonObject value)
    {
        return [name, value];
    }

    private static JsonObject Array(IEnumerable<JsonObject> items)
    {
        return new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. items.Select(item => (JsonNode)item.DeepClone())]) };
    }

    private static JsonObject Element(string sharedId)
    {
        return new JsonObject() { ["type"] = "node", ["sharedId"] = sharedId, ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } };
    }

    private static JsonObject Window(string contextId)
    {
        return new JsonObject() { ["type"] = "window", ["value"] = new JsonObject() { ["context"] = contextId } };
    }

    private static JsonObject Null()
    {
        return new JsonObject() { ["type"] = "null" };
    }

    // Moves fake time on by one poll interval at a time until the operation completes.
    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        return await operation;
    }
}
