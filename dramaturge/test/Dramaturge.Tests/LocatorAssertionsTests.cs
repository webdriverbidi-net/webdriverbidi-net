// <copyright file="LocatorAssertionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using static Dramaturge.Assertions;

public class LocatorAssertionsTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan ExpectTimeout = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task ExpectationIsCheckedAgainUntilItIsMet()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        AnswerScripts(session, Bool(false), Bool(true));

        await DriveAsync(time, Expect(page.Locate(new CssLocator("div"))).ToBeVisibleAsync(cancellationToken: TestContext.Current.CancellationToken));

        IReadOnlyList<JsonObject> calls = session.RemoteEnd.CommandsFor("script.callFunction");
        Assert.Equal(2, calls.Count);
        Assert.All(calls, call => Assert.Contains("inspector.isElementVisible(element)", (string?)call["params"]!["functionDeclaration"]));
        Assert.Equal(2, (int?)session.RemoteEnd.CommandsFor("browsingContext.locateNodes")[0]["params"]!["maxNodeCount"]);
    }

    [Fact]
    public async Task UnmetExpectationReportsWhatItLastSawAfterTheExpectTimeout()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        AnswerScripts(session, Bool(false));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page.Locate(new CssLocator("div"))).ToBeVisibleAsync(cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"div\" to be visible; received hidden after 2 seconds.", exception.Message);
        Assert.Equal("to be visible", exception.Expected);
        Assert.Equal("hidden", exception.Actual);
        Assert.Equal(ExpectTimeout, exception.Timeout);
    }

    [Fact]
    public async Task TimeoutCanBeGivenForOneExpectation()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page.Locate(new CssLocator("div"))).ToBeVisibleAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"div\" to be visible; no element matched after 1 seconds.", exception.Message);
        Assert.Null(exception.Actual);
        Assert.Equal(TimeSpan.FromSeconds(1), exception.Timeout);
    }

    [Theory]
    [InlineData("visible", false)]
    [InlineData("hidden", true)]
    [InlineData("attached", false)]
    [InlineData("enabled", false)]
    [InlineData("checked", false)]
    [InlineData("empty", false)]
    [InlineData("focused", false)]
    [InlineData("inviewport", false)]
    public async Task NoMatchingElementMeetsOnlyTheExpectationsItSatisfies(string matcher, bool holds)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("div")));

        await Run(holds ? expectations : expectations.Not, matcher, time);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => Run(holds ? expectations.Not : expectations, matcher, time));

        Assert.EndsWith("; no element matched after 1 seconds.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task NegatedExpectationWaitsForTheConditionToStopHolding()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        AnswerScripts(session, Bool(true), Bool(true), Bool(false));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("div")));

        await expectations.Not.Not.ToBeVisibleAsync(cancellationToken: TestContext.Current.CancellationToken);
        await DriveAsync(time, expectations.Not.ToBeVisibleAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(3, session.RemoteEnd.CommandsFor("script.callFunction").Count);
    }

    [Fact]
    public async Task NegatedExpectationReportsTheNegation()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page.Locate(new CssLocator("div"))).Not.ToBeAttachedAsync(cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"div\" not to be attached; received attached after 2 seconds.", exception.Message);
        Assert.Equal("not to be attached", exception.Expected);
    }

    [Fact]
    public async Task MoreThanOneMatchingElementFailsAtOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1", "div-2"));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("div")));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => expectations.ToBeHiddenAsync(cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<AmbiguousElementException>(() => expectations.Not.ToBeVisibleAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
    }

    [Fact]
    public async Task CountIsCheckedAgainstEveryMatch()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] lookups = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => ProtocolJson.Nodes(Interlocked.Increment(ref lookups[0]) == 1 ? 2 : 3));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("li")));

        await DriveAsync(time, expectations.ToHaveCountAsync(3, cancellationToken: TestContext.Current.CancellationToken));
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.Not.ToHaveCountAsync(3, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"li\" not to have count 3; received 3 after 1 seconds.", exception.Message);
        Assert.All(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"), lookup => Assert.False(lookup["params"]!.AsObject().ContainsKey("maxNodeCount")));
    }

    [Theory]
    [InlineData("enabled", "enabled", true, "enabled")]
    [InlineData("enabled", "disabled", false, "disabled")]
    [InlineData("disabled", "disabled", true, "disabled")]
    [InlineData("disabled", "enabled", false, "enabled")]
    [InlineData("editable", "editable", true, "editable")]
    [InlineData("editable", "readOnly", false, "read-only")]
    [InlineData("checked", "checked", true, "checked")]
    [InlineData("checked", "indeterminate", false, "indeterminate")]
    public async Task StateExpectationsReadTheStateTheLibraryReports(string matcher, string received, bool holds, string actual)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, Received(received));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("input")));

        await Run(holds ? expectations : expectations.Not, matcher, time);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => Run(holds ? expectations.Not : expectations, matcher, time));

        Assert.Equal(actual, exception.Actual);
        string state = matcher == "disabled" ? "enabled" : matcher;
        Assert.All(session.RemoteEnd.CommandsFor("script.callFunction"), call => Assert.Contains($"inspector.queryElementState(element, '{state}')", (string?)call["params"]!["functionDeclaration"]));
    }

    [Theory]
    [InlineData("editable", "error:noteditable", "css \"input\" is not an editable element.")]
    [InlineData("checked", "error:notcheckable", "css \"input\" is not a checkbox or radio button.")]
    public async Task StateExpectationsRejectElementsWithoutTheState(string matcher, string received, string message)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, Received(received));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Run(Expect(page.Locate(new CssLocator("input"))).Not, matcher, time));

        Assert.Equal(message, exception.Message);
        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task ElementRemovedWhileItIsCheckedIsCheckedAgain()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Received("error:notconnected"), Received("enabled"));

        await DriveAsync(time, Expect(page.Locate(new CssLocator("button"))).ToBeEnabledAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
    }

    [Fact]
    public async Task ElementRemovedEveryTimeItIsCheckedIsReported()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Received("error:notconnected"));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page.Locate(new CssLocator("button"))).ToBeEnabledAsync(cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"button\" to be enabled; the element was removed from the document after 2 seconds.", exception.Message);
        Assert.Null(exception.Actual);
    }

    [Theory]
    [InlineData("empty", "state.isEmpty(element)", "empty", "not empty")]
    [InlineData("focused", "state.isFocused(element)", "focused", "not focused")]
    [InlineData("inviewport", "inspector.isElementInViewPort(element)", "in the viewport", "outside the viewport")]
    public async Task ElementReadsAreCheckedInTheSandbox(string matcher, string function, string whenTrue, string whenFalse)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("div")));

        AnswerScripts(session, Bool(true));
        ExpectationFailedException holding = await Assert.ThrowsAsync<ExpectationFailedException>(() => Run(expectations.Not, matcher, time));
        AnswerScripts(session, Bool(false));
        ExpectationFailedException notHolding = await Assert.ThrowsAsync<ExpectationFailedException>(() => Run(expectations, matcher, time));

        Assert.Equal(whenTrue, holding.Actual);
        Assert.Equal(whenFalse, notHolding.Actual);
        Assert.All(session.RemoteEnd.CommandsFor("script.callFunction"), call =>
        {
            Assert.Contains(function, (string?)call["params"]!["functionDeclaration"]);
            Assert.Equal(page.Group().Options.SandboxName, (string?)call["params"]!["target"]!["sandbox"]);
            Assert.Equal("div-1", (string?)call["params"]!["arguments"]![0]!["sharedId"]);
        });
    }

    private static Task Run(LocatorAssertions expectations, string matcher, FakeTimeProvider time)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        CancellationToken token = TestContext.Current.CancellationToken;
        return DriveAsync(time, matcher switch
        {
            "visible" => expectations.ToBeVisibleAsync(timeout, token),
            "hidden" => expectations.ToBeHiddenAsync(timeout, token),
            "attached" => expectations.ToBeAttachedAsync(timeout, token),
            "enabled" => expectations.ToBeEnabledAsync(timeout, token),
            "disabled" => expectations.ToBeDisabledAsync(timeout, token),
            "editable" => expectations.ToBeEditableAsync(timeout, token),
            "checked" => expectations.ToBeCheckedAsync(timeout, token),
            "empty" => expectations.ToBeEmptyAsync(timeout, token),
            "focused" => expectations.ToBeFocusedAsync(timeout, token),
            _ => expectations.ToBeInViewportAsync(timeout, token),
        });
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { PollInterval = PollInterval, ExpectTimeout = ExpectTimeout, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Answers each call in turn, repeating the last answer.
    private static void AnswerScripts(FakeSession session, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => ProtocolJson.Success((JsonObject)results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone()));
    }

    private static JsonObject Bool(bool value)
    {
        return new JsonObject() { ["type"] = "boolean", ["value"] = value };
    }

    private static JsonObject Received(string received)
    {
        JsonObject value = new() { ["type"] = "string", ["value"] = received };
        return new JsonObject() { ["type"] = "object", ["value"] = new JsonArray(new JsonArray("received", value), new JsonArray("isRadio", Bool(false))) };
    }

    private static async Task DriveAsync(FakeTimeProvider time, Task operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        await operation;
    }
}
