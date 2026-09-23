// <copyright file="ModuleExtensionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Nodes;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.TestUtilities;

public class ModuleExtensionTests
{
    private static readonly SharedReference Element = new("element-1");

    // Every extension, with the command it sends.
    public static TheoryData<string, Func<BiDiDriver, TimeSpan?, CancellationToken, Task>> Extensions => new()
    {
        { "session.subscribe", (d, t, c) => d.Session.SubscribeAsync(["log.entryAdded"], timeoutOverride: t, cancellationToken: c) },
        { "session.unsubscribe", (d, t, c) => d.Session.UnsubscribeAsync("subscription-1", t, c) },
        { "browsingContext.close", (d, t, c) => d.BrowsingContext.CloseAsync("context-1", t, c) },
        { "browsingContext.getTree", (d, t, c) => d.BrowsingContext.GetTopLevelBrowsingContextsAsync(t, c) },
        { "browsingContext.navigate", (d, t, c) => d.BrowsingContext.NavigateAsync("context-1", "https://example.com/", timeoutOverride: t, cancellationToken: c) },
        { "browsingContext.captureScreenshot", (d, t, c) => d.BrowsingContext.CaptureScreenshotAsync("context-1", timeoutOverride: t, cancellationToken: c) },
        { "browsingContext.locateNodes", (d, t, c) => d.BrowsingContext.LocateNodesByCssSelectorAsync("context-1", "p", timeoutOverride: t, cancellationToken: c) },
        { "script.addPreloadScript", (d, t, c) => d.Script.AddPreloadScriptAsync("() => {}", timeoutOverride: t, cancellationToken: c) },
        { "script.removePreloadScript", (d, t, c) => d.Script.RemovePreloadScriptAsync("preload-script-1", t, c) },
        { "script.callFunction", (d, t, c) => d.Script.CallFunctionAsync("context-1", "() => 1", timeoutOverride: t, cancellationToken: c) },
        { "input.performActions", (d, t, c) => d.Input.ClickElementAsync("context-1", Element, t, c) },
        { "input.performActions", (d, t, c) => d.Input.SendKeysAsync("context-1", "a", t, c) },
        { "input.performActions", (d, t, c) => d.Input.PerformActionsAsync("context-1", new InputBuilder().AddScrollAction(0, 10), t, c) },
        { "input.releaseActions", (d, t, c) => d.Input.ReleaseActionsAsync("context-1", t, c) },
    };

    [Theory]
    [MemberData(nameof(Extensions))]
    public async Task ExtensionForwardsTimeout(string method, Func<BiDiDriver, TimeSpan?, CancellationToken, Task> call)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.NeverAnswer(method);

        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => call(driver, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(Extensions))]
    public async Task ExtensionForwardsCancellation(string method, Func<BiDiDriver, TimeSpan?, CancellationToken, Task> call)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.NeverAnswer(method);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call(driver, null, cancellationSource.Token));
    }

    [Fact]
    public async Task SubscribeSendsEventsAndContextsAndReturnsSubscription()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        string subscriptionId = await driver.Session.SubscribeAsync(["log.entryAdded", "network.beforeRequestSent"], ["context-1"], cancellationToken: TestContext.Current.CancellationToken);
        await driver.Session.UnsubscribeAsync(subscriptionId, cancellationToken: TestContext.Current.CancellationToken);

        JsonNode subscribe = Assert.Single(remoteEnd.CommandsFor("session.subscribe"))["params"]!;
        Assert.Equal("""["log.entryAdded","network.beforeRequestSent"]""", subscribe["events"]!.ToJsonString());
        Assert.Equal("""["context-1"]""", subscribe["contexts"]!.ToJsonString());
        JsonNode unsubscribe = Assert.Single(remoteEnd.CommandsFor("session.unsubscribe"))["params"]!;
        Assert.Equal($"""["{subscriptionId}"]""", unsubscribe["subscriptions"]!.ToJsonString());
    }

    [Fact]
    public async Task TopLevelBrowsingContextsAreFetchedWithoutChildren()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject context = new() { ["context"] = "context-1", ["url"] = "about:blank", ["userContext"] = "default", ["clientWindow"] = "window-1", ["originalOpener"] = null, ["children"] = null };
        remoteEnd.AnswerWith("browsingContext.getTree", new JsonObject() { ["contexts"] = new JsonArray(context) });

        IReadOnlyList<BrowsingContextInfo> contexts = await driver.BrowsingContext.GetTopLevelBrowsingContextsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("context-1", Assert.Single(contexts).BrowsingContextId);
        Assert.Equal(0, (int?)Assert.Single(remoteEnd.CommandsFor("browsingContext.getTree"))["params"]!["maxDepth"]);
    }

    [Fact]
    public async Task NavigateWaitsAsRequestedAndReturnsUrl()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("browsingContext.navigate", new JsonObject() { ["navigation"] = "navigation-1", ["url"] = "https://example.com/final" });

        string url = await driver.BrowsingContext.NavigateAsync("context-1", "https://example.com/", ReadinessState.Complete, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://example.com/final", url);
        JsonNode parameters = Assert.Single(remoteEnd.CommandsFor("browsingContext.navigate"))["params"]!;
        Assert.Equal("complete", (string?)parameters["wait"]);
        Assert.Equal("context-1", (string?)parameters["context"]);
    }

    [Fact]
    public async Task ScreenshotIsReturnedAsImageBytes()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        byte[] image = [0x89, 0x50, 0x4E, 0x47];
        remoteEnd.AnswerWith("browsingContext.captureScreenshot", new JsonObject() { ["data"] = Convert.ToBase64String(image) });

        byte[] screenshot = await driver.BrowsingContext.CaptureScreenshotAsync("context-1", ScreenshotOrigin.Document, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(image, screenshot);
        Assert.Equal("document", (string?)Assert.Single(remoteEnd.CommandsFor("browsingContext.captureScreenshot"))["params"]!["origin"]);
    }

    public static TheoryData<Func<BiDiDriver, Task<IReadOnlyList<SharedReference>>>, string> Locators => new()
    {
        { d => d.BrowsingContext.LocateNodesByCssSelectorAsync("context-1", "p.note", [Element]), """{"type":"css","value":"p.note"}""" },
        { d => d.BrowsingContext.LocateNodesByXPathAsync("context-1", "//p"), """{"type":"xpath","value":"//p"}""" },
        { d => d.BrowsingContext.LocateNodesByAccessibleRoleAsync("context-1", "button", "Submit"), """{"type":"accessibility","value":{"role":"button","name":"Submit"}}""" },
        { d => d.BrowsingContext.LocateNodesByAccessibleRoleAsync("context-1", "button"), """{"type":"accessibility","value":{"role":"button"}}""" },
        { d => d.BrowsingContext.LocateNodesByVisibleTextAsync("context-1", "Hello"), """{"type":"innerText","value":"Hello","ignoreCase":true,"matchType":"partial"}""" },
        { d => d.BrowsingContext.LocateNodesByVisibleTextAsync("context-1", "Hello", isPartialMatch: false, matchCase: true), """{"type":"innerText","value":"Hello","ignoreCase":false,"matchType":"full"}""" },
    };

    [Theory]
    [MemberData(nameof(Locators))]
    public async Task LocatorsAreSentAndNodesReturnedAsReferences(Func<BiDiDriver, Task<IReadOnlyList<SharedReference>>> locate, string expectedLocator)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("browsingContext.locateNodes", new JsonObject() { ["nodes"] = new JsonArray(new JsonObject() { ["type"] = "node", ["sharedId"] = "node-1" }) });

        IReadOnlyList<SharedReference> nodes = await locate(driver);

        Assert.Equal("node-1", Assert.Single(nodes).SharedId);
        JsonNode parameters = Assert.Single(remoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!;
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expectedLocator), parameters["locator"]), parameters["locator"]!.ToJsonString());
    }

    [Fact]
    public async Task LocatorSearchesWithinParentNodes()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("browsingContext.locateNodes", new JsonObject() { ["nodes"] = new JsonArray() });

        await driver.BrowsingContext.LocateNodesByXPathAsync("context-1", ".//p", [Element], cancellationToken: TestContext.Current.CancellationToken);

        JsonNode parameters = Assert.Single(remoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!;
        Assert.Equal("element-1", (string?)parameters["startNodes"]![0]!["sharedId"]);
    }

    [Fact]
    public async Task PreloadScriptIsAddedAndRemoved()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        string preloadScriptId = await driver.Script.AddPreloadScriptAsync("(channel) => {}", [new ChannelValue(new ChannelProperties("channel-1"))], "isolated", cancellationToken: TestContext.Current.CancellationToken);
        await driver.Script.RemovePreloadScriptAsync(preloadScriptId, cancellationToken: TestContext.Current.CancellationToken);

        JsonNode added = Assert.Single(remoteEnd.CommandsFor("script.addPreloadScript"))["params"]!;
        Assert.Equal("isolated", (string?)added["sandbox"]);
        Assert.Equal("channel-1", (string?)added["arguments"]![0]!["value"]!["channel"]);
        Assert.Equal(preloadScriptId, (string?)Assert.Single(remoteEnd.CommandsFor("script.removePreloadScript"))["params"]!["script"]);
    }

    [Fact]
    public async Task FunctionIsCalledInSandboxAndItsResultReturned()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("script.callFunction", Success(new JsonObject() { ["type"] = "string", ["value"] = "Example" }));

        StringRemoteValue title = await driver.Script.CallFunctionAsync<StringRemoteValue>("context-1", "(x) => document.title", [LocalValue.Number(1)], "isolated", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Example", title.Value);
        JsonNode parameters = Assert.Single(remoteEnd.CommandsFor("script.callFunction"))["params"]!;
        Assert.Equal("context-1", (string?)parameters["target"]!["context"]);
        Assert.Equal("isolated", (string?)parameters["target"]!["sandbox"]);
        Assert.True((bool?)parameters["awaitPromise"]);
        Assert.Equal(1, (int?)parameters["arguments"]![0]!["value"]);
    }

    [Fact]
    public async Task ResultOfAnotherKindIsRejected()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("script.callFunction", Success(new JsonObject() { ["type"] = "number", ["value"] = 42 }));

        await Assert.ThrowsAsync<WebDriverBiDiException>(() => driver.Script.CallFunctionAsync<StringRemoteValue>("context-1", "() => 42", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowingFunctionReportsTheJavaScriptException()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject exception = new()
        {
            ["type"] = "exception",
            ["realm"] = "realm-1",
            ["exceptionDetails"] = new JsonObject()
            {
                ["text"] = "Error: boom",
                ["lineNumber"] = 2,
                ["columnNumber"] = 7,
                ["stackTrace"] = new JsonObject() { ["callFrames"] = new JsonArray() },
                ["exception"] = new JsonObject() { ["type"] = "error" },
            },
        };
        remoteEnd.AnswerWith("script.callFunction", exception);

        ScriptException thrown = await Assert.ThrowsAsync<ScriptException>(() => driver.Script.CallFunctionAsync("context-1", "() => { throw new Error('boom'); }", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("Error: boom (line 2, column 7)", thrown.Message);
        Assert.Equal(RemoteValueType.Error, thrown.Details!.Exception.Type);
    }

    [Fact]
    public async Task ClickMovesToElementThenPressesAndReleases()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await driver.Input.ClickElementAsync("context-1", Element, cancellationToken: TestContext.Current.CancellationToken);

        JsonNode source = Assert.Single(Assert.Single(remoteEnd.CommandsFor("input.performActions"))["params"]!["actions"]!.AsArray())!;
        Assert.Equal("pointer", (string?)source["type"]);
        Assert.Equal(["pointerMove", "pointerDown", "pointerUp"], source["actions"]!.AsArray().Select(action => (string)action!["type"]!));
        Assert.Equal("element-1", (string?)source["actions"]![0]!["origin"]!["element"]!["sharedId"]);
    }

    [Fact]
    public async Task SendKeysTypesIntoFocusedElementWithoutClicking()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await driver.Input.SendKeysAsync("context-1", "ab", cancellationToken: TestContext.Current.CancellationToken);

        JsonNode source = Assert.Single(Assert.Single(remoteEnd.CommandsFor("input.performActions"))["params"]!["actions"]!.AsArray())!;
        Assert.Equal("key", (string?)source["type"]);
        Assert.Equal(["keyDown:a", "keyUp:a", "keyDown:b", "keyUp:b"], source["actions"]!.AsArray().Select(action => $"{action!["type"]}:{action["value"]}"));
    }

    private static JsonObject Success(JsonObject result) => new() { ["type"] = "success", ["realm"] = "realm-1", ["result"] = result };
}
