// <copyright file="LocatorChainTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class LocatorChainTests
{
    [Fact]
    public async Task EachStepSearchesWithinThePreviousStepsMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Nodes("form-1", "form-2"), ProtocolJson.Nodes("input-1", "input-2", "input-3"));

        int count = await page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, count);
        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browsingContext.locateNodes");
        Assert.False(commands[0]["params"]!.AsObject().ContainsKey("startNodes"));
        Assert.Equal("input", (string?)commands[1]["params"]!["locator"]!["value"]);
        Assert.Equal(["form-1", "form-2"], commands[1]["params"]!["startNodes"]!.AsArray().Select(node => (string?)node!["sharedId"]));
    }

    [Fact]
    public async Task ElementFoundFromNestedStartNodesIsCountedOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Nodes("outer", "inner"), ProtocolJson.Nodes("span-1", "span-2", "span-1"));

        int count = await page.Locate(new CssLocator("div")).Locate(new CssLocator("span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
    }

    // As the protocol serializes a value, an object met again is a bare reference to its first entry.
    [Fact]
    public async Task RepeatWithoutASharedIdIsDropped()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject leaves = ProtocolJson.Nodes("span-1", "span-2");
        leaves["nodes"]![0]!["internalId"] = "internal-1";
        leaves["nodes"]!.AsArray().Add(new JsonObject() { ["type"] = "node", ["internalId"] = "internal-1" });
        AnswerInTurn(session, ProtocolJson.Nodes("outer", "inner"), leaves);

        int count = await page.Locate(new CssLocator("div")).Locate(new CssLocator("span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task StepWithNoMatchesEndsTheSearch()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));

        int count = await page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public async Task MatchLimitIsSentOnlyWhereADuplicateCannotUseItUp(int parents, bool limited)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Nodes(parents), ProtocolJson.Nodes(0));

        await page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).IsVisibleAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browsingContext.locateNodes");
        Assert.False(commands[0]["params"]!.AsObject().ContainsKey("maxNodeCount"));
        Assert.Equal(limited, commands[1]["params"]!.AsObject().ContainsKey("maxNodeCount"));
    }

    [Theory]
    [InlineData("first", "item-1")]
    [InlineData("last", "item-3")]
    [InlineData("nth=1", "item-2")]
    public async Task PositionPicksOneMatch(string position, string expected)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Nodes("item-1", "item-2", "item-3"), ProtocolJson.Nodes("child"));
        ElementLocator items = page.Locate(new CssLocator("li"));
        ElementLocator picked = position switch
        {
            "first" => items.First(),
            "last" => items.Last(),
            _ => items.Nth(1),
        };

        int count = await picked.Locate(new CssLocator("span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browsingContext.locateNodes");
        Assert.False(commands[0]["params"]!.AsObject().ContainsKey("maxNodeCount"));
        Assert.Equal([expected], commands[1]["params"]!["startNodes"]!.AsArray().Select(node => (string?)node!["sharedId"]));
    }

    [Fact]
    public async Task PositionPastTheMatchesMatchesNothing()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));
        ElementLocator items = page.Locate(new CssLocator("li"));

        Assert.Equal(0, await items.Nth(2).CountAsync(TestContext.Current.CancellationToken));
        Assert.False(await items.Nth(5).IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await items.Last().CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LastOfNoMatchesMatchesNothing()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));

        Assert.Equal(0, await page.Locate(new CssLocator("li")).Last().CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NegativePositionIsRejected()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => page.Locate(new CssLocator("li")).Nth(-1)).ParamName);
    }

    [Fact]
    public async Task ChainingLeavesTheOriginalUnchangedAndDescribesEveryStep()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        ElementLocator form = page.Locate(new CssLocator("form"));

        ElementLocator input = form.Locate(new XPathLocator(".//input")).Nth(1);

        Assert.Equal("css \"form\"", form.ToString());
        Assert.Equal("css \"form\" >> xpath \".//input\" >> nth=1", input.ToString());
        Assert.Equal("css \"form\" >> first >> last", form.First().Last().ToString());
        Assert.Same(form.Frame, input.Frame);
    }

    [Fact]
    public async Task AmbiguityAndTimeoutsDescribeTheWholeChain()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Nodes("form-1"), ProtocolJson.Nodes(2));

        AmbiguousElementException exception = await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).IsVisibleAsync(TestContext.Current.CancellationToken));

        Assert.StartsWith("css \"form\" >> css \"input\" matched more than one element", exception.Message);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    // Answers each "browsingContext.locateNodes" with the next result, repeating the last.
    private static void AnswerInTurn(FakeSession session, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone());
    }
}
