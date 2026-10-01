// <copyright file="LocatorFilterTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class LocatorFilterTests
{
    [Fact]
    public async Task AndKeepsTheElementsBothLocatorsFind()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["button"] = ["a", "b", "c"], [".primary"] = ["d", "c", "b"] });

        ElementLocator locator = page.Locate(new CssLocator("button")).And(page.Locate(new CssLocator(".primary")));
        int count = await locator.CountAsync(TestContext.Current.CancellationToken);
        int firstCount = await locator.First().Locate(new CssLocator("span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
        Assert.Equal(["b"], StartNodesOf(session, "span"));
        Assert.All(session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Where(command => (string?)command["params"]!["locator"]!["value"] == ".primary"), command => Assert.False(command["params"]!.AsObject().ContainsKey("startNodes")));
        Assert.Equal(0, firstCount);
    }

    [Fact]
    public async Task AndOfNoMatchesSkipsTheOtherLookup()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["button"] = [] });

        int count = await page.Locate(new CssLocator("button")).And(page.Locate(new CssLocator(".primary"))).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
    }

    [Fact]
    public async Task OrAddsTheOtherLocatorsNewMatchesAfterThisOnes()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["button"] = ["a", "b"], ["a.link"] = ["c", "b"] });

        ElementLocator locator = page.Locate(new CssLocator("button")).Or(page.Locate(new CssLocator("a.link")));
        int count = await locator.CountAsync(TestContext.Current.CancellationToken);
        await locator.Last().Locate(new CssLocator("span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, count);
        Assert.Equal(["c"], StartNodesOf(session, "span"));
    }

    [Fact]
    public async Task OrFindsTheOtherLocatorsMatchesWhenThisFindsNone()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["form"] = [], ["input"] = [], ["a.link"] = ["c"] });

        int count = await page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).Or(page.Locate(new CssLocator("a.link"))).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TextFiltersCheckEveryCandidateInOneCall()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["li"] = ["a", "b", "c"] });
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Booleans(true, false, true));

        int containing = await page.Locate(new CssLocator("li")).Filter(hasText: "Sign in").CountAsync(TestContext.Current.CancellationToken);
        int notContaining = await page.Locate(new CssLocator("li")).Filter(hasNotText: "Sign in").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, containing);
        Assert.Equal(1, notContaining);
        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Contains("elementsContainText", (string?)call["functionDeclaration"]);
        JsonArray arguments = call["arguments"]!.AsArray();
        Assert.Equal("Sign in", (string?)arguments[0]!["value"]);
        Assert.Equal(["a", "b", "c"], arguments.Skip(1).Select(argument => (string?)argument!["sharedId"]));
    }

    [Fact]
    public async Task TextFilterOfNoCandidatesSendsNoScript()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerByLocator(session, new() { ["li"] = [] });

        int count = await page.Locate(new CssLocator("li")).Filter(hasText: "x").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task ElementFiltersSearchWithinEachCandidate()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters =>
        {
            string locator = (string)parameters["locator"]!["value"]!;
            string? start = (string?)parameters["startNodes"]?[0]?["sharedId"];
            return locator == "li" ? ProtocolJson.Nodes("a", "b") : start == "a" ? ProtocolJson.Nodes("span-in-a") : ProtocolJson.Nodes(0);
        });
        ElementLocator span = page.Locate(new CssLocator("span"));

        int having = await page.Locate(new CssLocator("li")).Filter(has: span).CountAsync(TestContext.Current.CancellationToken);
        int notHaving = await page.Locate(new CssLocator("li")).Filter(hasNot: span).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, having);
        Assert.Equal(1, notHaving);
        Assert.Equal([["a"], ["b"], ["a"], ["b"]], session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Where(command => (string?)command["params"]!["locator"]!["value"] == "span").Select(command => command["params"]!["startNodes"]!.AsArray().Select(node => (string?)node!["sharedId"]).ToArray()));
    }

    [Fact]
    public async Task EveryConditionMustHold()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => (string)parameters["locator"]!["value"]! == "li" ? ProtocolJson.Nodes("a", "b", "c") : ProtocolJson.Nodes(1));
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => Interlocked.Increment(ref calls[0]) == 1 ? ProtocolJson.Booleans(true, true, false) : ProtocolJson.Booleans(false, true));

        int count = await page.Locate(new CssLocator("li")).Filter(hasText: "Item", hasNotText: "Old", has: page.Locate(new CssLocator("span"))).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        Assert.Equal(2, session.RemoteEnd.CommandsFor("script.callFunction").Count);
    }

    [Fact]
    public async Task FilterNeedsACondition()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Throws<ArgumentException>(() => page.Locate(new CssLocator("li")).Filter());
    }

    [Fact]
    public async Task LocatorsFromAnotherFrameAreRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator here = page.Locate(new CssLocator("li"));
        ElementLocator elsewhere = page.Frames[1].Locate(new CssLocator("span"));

        Assert.Equal("other", Assert.Throws<ArgumentException>(() => here.And(elsewhere)).ParamName);
        Assert.Equal("other", Assert.Throws<ArgumentException>(() => here.Or(elsewhere)).ParamName);
        Assert.Equal("has", Assert.Throws<ArgumentException>(() => here.Filter(has: elsewhere)).ParamName);
        Assert.Equal("hasNot", Assert.Throws<ArgumentException>(() => here.Filter(hasNot: elsewhere)).ParamName);
    }

    [Fact]
    public async Task CombinationsAndFiltersAreDescribed()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        ElementLocator button = page.Locate(new CssLocator("button"));
        ElementLocator span = page.Locate(new CssLocator("span"));

        Assert.Equal("css \"button\" >> and(css \"span\")", button.And(span).ToString());
        Assert.Equal("css \"button\" >> or(css \"span\" >> first)", button.Or(span.First()).ToString());
        Assert.Equal("css \"button\" >> filter(hasText \"Go\", hasNotText \"Stop\", has(css \"span\"), hasNot(css \"span\"))", button.Filter("Go", "Stop", span, span).ToString());
        Assert.Equal("css \"button\" >> filter(hasText \"Go\")", button.Filter(hasText: "Go").ToString());
        Assert.Equal("css \"button\" >> filter(hasNot(css \"span\"))", button.Filter(hasNot: span).ToString());
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    // Answers "browsingContext.locateNodes" with the nodes each locator value finds, wherever it starts.
    private static void AnswerByLocator(FakeSession session, Dictionary<string, string[]> matches)
    {
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => matches.TryGetValue((string)parameters["locator"]!["value"]!, out string[]? found) ? ProtocolJson.Nodes(found) : ProtocolJson.Nodes(0));
    }

    private static IEnumerable<string?> StartNodesOf(FakeSession session, string locatorValue)
    {
        return session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Last(command => (string?)command["params"]!["locator"]!["value"] == locatorValue)["params"]!["startNodes"]!.AsArray().Select(node => (string?)node!["sharedId"]);
    }
}
