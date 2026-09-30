// <copyright file="ElementLocatorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class ElementLocatorTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task CountLooksUpEveryMatch()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(3));

        int count = await page.Locate(new CssLocator("li")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, count);
        JsonObject parameters = Parameters(session, "browsingContext.locateNodes");
        Assert.Equal(page.Id, (string?)parameters["context"]);
        Assert.Equal("css", (string?)parameters["locator"]!["type"]);
        Assert.Equal("li", (string?)parameters["locator"]!["value"]);
        Assert.False(parameters.ContainsKey("maxNodeCount"));
        Assert.Equal(0, (int?)parameters["serializationOptions"]!["maxDomDepth"]);
    }

    [Fact]
    public async Task VisibilityIsReadInTheSandbox()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(1));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Boolean(true));

        bool visible = await page.Locate(new CssLocator("#status")).IsVisibleAsync(TestContext.Current.CancellationToken);

        Assert.True(visible);
        Assert.Equal(2, (int?)Parameters(session, "browsingContext.locateNodes")["maxNodeCount"]);
        JsonObject call = Parameters(session, "script.callFunction");
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        Assert.Equal(group.Options.SandboxName, (string?)call["target"]!["sandbox"]);
        Assert.Contains("isElementVisible", (string?)call["functionDeclaration"]);
        Assert.Equal("node-1", (string?)call["arguments"]![0]!["sharedId"]);
    }

    [Fact]
    public async Task NoMatchIsNotVisible()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        bool visible = await page.Locate(new CssLocator("#missing")).IsVisibleAsync(TestContext.Current.CancellationToken);

        Assert.False(visible);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task SeveralMatchesAreAmbiguous()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator locator = page.Locate(new CssLocator("button"));

        AmbiguousElementException visibility = await Assert.ThrowsAsync<AmbiguousElementException>(() => locator.IsVisibleAsync(TestContext.Current.CancellationToken));
        AmbiguousElementException wait = await Assert.ThrowsAsync<AmbiguousElementException>(() => locator.WaitForAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("css \"button\" matched more than one element", visibility.Message);
        Assert.Equal(visibility.Message, wait.Message);
    }

    [Fact]
    public async Task WaitLooksUpTheElementAgainUntilItIsVisible()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync(time);
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        AnswerInTurn(session, "browsingContext.locateNodes", ProtocolJson.Nodes(0), ProtocolJson.Nodes(0), ProtocolJson.Nodes(1));
        AnswerInTurn(session, "script.callFunction", ProtocolJson.Boolean(false), ProtocolJson.Boolean(true));

        await DriveAsync(time, page.Locate(new CssLocator("#late")).WaitForAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(4, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
        Assert.Equal(2, session.RemoteEnd.CommandsFor("script.callFunction").Count);
    }

    [Theory]
    [InlineData(ElementState.Attached, 1, null)]
    [InlineData(ElementState.Detached, 0, null)]
    [InlineData(ElementState.Hidden, 0, null)]
    [InlineData(ElementState.Hidden, 1, false)]
    [InlineData(ElementState.Visible, 1, true)]
    public async Task WaitEndsAsSoonAsTheStateHolds(ElementState state, int matches, bool? visible)
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        if (visible is not null)
        {
            session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Boolean(visible.Value));
        }

        await page.Locate(new CssLocator("#target")).WaitForAsync(state, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
        Assert.Equal(visible is null ? 0 : 1, session.RemoteEnd.CommandsFor("script.callFunction").Count);
    }

    [Theory]
    [InlineData(ElementState.Visible, 0, null, "no element matched")]
    [InlineData(ElementState.Visible, 1, false, "the element was hidden")]
    [InlineData(ElementState.Hidden, 1, true, "the element was visible")]
    [InlineData(ElementState.Detached, 1, null, "an element still matched")]
    public async Task WaitThatTimesOutReportsWhatItLastSaw(ElementState state, int matches, bool? visible, string observed)
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync(time);
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        if (visible is not null)
        {
            session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Boolean(visible.Value));
        }

        Task wait = page.Locate(new CssLocator("#target")).WaitForAsync(state, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, wait));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"#target\" to be {state.ToString().ToLowerInvariant()}; {observed}.", exception.Message);
    }

    [Fact]
    public async Task ElementRemovedWhileCheckedIsLookedUpAgain()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync(time);
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(1));
        session.RemoteEnd.FailWith("script.callFunction", "no such node", "The node was removed");

        Task wait = page.Locate(new CssLocator("#flaky")).WaitForAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, wait));

        Assert.EndsWith("the element was removed while it was checked.", exception.Message);
        Assert.True(session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count > 1);
    }

    [Fact]
    public async Task OtherCommandErrorsEndTheWait()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("browsingContext.locateNodes", "invalid selector", "Bad selector");

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Locate(new CssLocator("[")).WaitForAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Bad selector", exception.Message);
    }

    [Fact]
    public async Task CommandStillRunningWhenTimeRunsOutEndsTheWait()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.NeverAnswer("browsingContext.locateNodes");

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.Locate(new CssLocator("#slow")).WaitForAsync(timeout: TimeSpan.FromMilliseconds(200), cancellationToken: TestContext.Current.CancellationToken));

        Assert.EndsWith("a command was still running.", exception.Message);
    }

    [Fact]
    public async Task CancelledWaitStops()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync(time);
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));
        using CancellationTokenSource cancellation = new();

        Task wait = page.Locate(new CssLocator("#never")).WaitForAsync(cancellationToken: cancellation.Token);
        await session.RemoteEnd.WaitForCommandAsync("browsingContext.locateNodes");
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
    }

    [Fact]
    public async Task LibraryMissingFromADocumentIsInstalledThenUsed()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(1));
        AnswerInTurn(session, "script.callFunction", ProtocolJson.Exception("Error: webdriverbidi-automation: scripts not installed"), ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }), ProtocolJson.Boolean(true));

        bool visible = await page.Locate(new CssLocator("#status")).IsVisibleAsync(TestContext.Current.CancellationToken);

        Assert.True(visible);
        IReadOnlyList<JsonObject> calls = session.RemoteEnd.CommandsFor("script.callFunction");
        Assert.Equal(3, calls.Count);
        Assert.Contains("Acquiescence", (string?)calls[1]["params"]!["functionDeclaration"]);
        Assert.Contains("selectOptions(element, options)", (string?)calls[1]["params"]!["functionDeclaration"]);
        Assert.Equal(group.Options.SandboxName, (string?)calls[1]["params"]!["target"]!["sandbox"]);
        Assert.Equal((string?)calls[0]["params"]!["functionDeclaration"], (string?)calls[2]["params"]!["functionDeclaration"]);
    }

    [Fact]
    public async Task OtherScriptErrorsAreNotTakenForAMissingLibrary()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(1));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Exception("TypeError: broken"));

        await Assert.ThrowsAsync<ScriptException>(() => page.Locate(new CssLocator("#status")).IsVisibleAsync(TestContext.Current.CancellationToken));

        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task PreloadScriptInstallsTheLibraryAndIsRemovedWithTheGroup()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await group.DisposeAsync();

        JsonObject added = Parameters(session, "script.addPreloadScript");
        Assert.Contains("Acquiescence.ElementStateInspector", (string?)added["functionDeclaration"]);
        Assert.Equal(group.Options.SandboxName, (string?)added["sandbox"]);
        Assert.Equal((string?)session.RemoteEnd.ResultsFor("script.addPreloadScript")[0]["script"], (string?)Parameters(session, "script.removePreloadScript")["script"]);
    }

    [Fact]
    public async Task LocatorsFindElementsInTheirFrame()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        FakeContext frameContext = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(1));
        Frame frame = page.Frames[1];

        ElementLocator pageLocator = page.Locate(new CssLocator("p"));
        ElementLocator frameLocator = frame.Locate(new CssLocator("p"));
        await pageLocator.CountAsync(TestContext.Current.CancellationToken);
        await frameLocator.CountAsync(TestContext.Current.CancellationToken);

        Assert.Same(page.MainFrame, pageLocator.Frame);
        Assert.Same(frame, frameLocator.Frame);
        Assert.Equal([page.Id, frameContext.Id], session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Select(command => (string?)command["params"]!["context"]));
    }

    [Fact]
    public async Task LocatorDescribesHowItFindsElements()
    {
        (BiDiDriver driver, FakeSession _, BrowserGroup group) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup ownedGroup = group;
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("css \"a.b\"", page.Locate(new CssLocator("a.b")).ToString());
        Assert.Equal("xpath \"//a\"", page.Locate(new XPathLocator("//a")).ToString());
        Assert.Equal("text \"Sign in\"", page.Locate(new InnerTextLocator("Sign in")).ToString());
        Assert.Equal("accessibility role \"button\" name \"OK\"", page.Locate(new AccessibilityLocator() { Role = "button", Name = "OK" }).ToString());
        Assert.Equal("context \"frame-1\"", page.Locate(new ContextLocator("frame-1")).ToString());
        Assert.Equal("custom", page.Locate(new CustomLocator()).ToString());
    }

    [Fact]
    public void ElementStateLibraryIsEmbedded()
    {
        using Stream? resource = typeof(BrowserGroup).Assembly.GetManifestResourceStream("acquiescence-library");

        Assert.NotNull(resource);
        Assert.True(resource.Length > 0);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, BrowserGroup Group)> ConnectAsync(TimeProvider? time = null)
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        AutomationOptions options = new() { PollInterval = PollInterval, TimeProvider = time ?? TimeProvider.System };
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, options, TestContext.Current.CancellationToken);
        return (driver, session, group);
    }

    // Answers each sending of a command with the next result, repeating the last.
    private static void AnswerInTurn(FakeSession session, string method, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith(method, _ => results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone());
    }

    // Moves fake time on by one poll interval at a time until the operation completes.
    private static async Task DriveAsync(FakeTimeProvider time, Task operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        await operation;
    }

    private static JsonObject Parameters(FakeSession session, string method)
    {
        return Assert.Single(session.RemoteEnd.CommandsFor(method))["params"]!.AsObject();
    }

    private sealed class CustomLocator : Locator
    {
        public override string Type => "custom";
    }
}
