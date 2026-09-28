// <copyright file="RoleStatesAndLabelTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class RoleStatesAndLabelTests
{
    [Fact]
    public async Task RoleStatesFilterTheBrowsersRoleMatchesInOneCall()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1", "box-2", "box-3"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Booleans(true, false, true));
        RoleStates states = new() { Checked = ToggleState.On, Pressed = ToggleState.Mixed, Expanded = false, Selected = true, Level = 2, Disabled = false };

        int count = await page.GetByRole("checkbox", "Remember me", states).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
        JsonObject locator = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!["locator"]!.AsObject();
        Assert.Equal("checkbox", (string?)locator["value"]!["role"]);
        Assert.Equal("Remember me", (string?)locator["value"]!["name"]);
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Contains("elementsMatchAriaStates", (string?)call["functionDeclaration"]);
        JsonArray arguments = call["arguments"]!.AsArray();
        Dictionary<string, JsonNode?> sentStates = arguments[0]!["value"]!.AsArray().ToDictionary(entry => (string)entry![0]!, entry => entry![1]!["value"]);
        Assert.True((bool?)sentStates["checked"]);
        Assert.Equal("mixed", (string?)sentStates["pressed"]);
        Assert.False((bool?)sentStates["expanded"]);
        Assert.True((bool?)sentStates["selected"]);
        Assert.Equal(2, (int?)sentStates["level"]);
        Assert.False((bool?)sentStates["disabled"]);
        Assert.Equal(["box-1", "box-2", "box-3"], arguments.Skip(1).Select(argument => (string?)argument!["sharedId"]));
    }

    [Fact]
    public async Task OnlyTheGivenStatesAreSent()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Booleans(true));

        await page.GetByRole("checkbox", states: new RoleStates() { Checked = ToggleState.Off }).CountAsync(TestContext.Current.CancellationToken);

        JsonArray sentStates = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!["arguments"]![0]!["value"]!.AsArray();
        JsonNode entry = Assert.Single(sentStates)!;
        Assert.Equal("checked", (string?)entry[0]);
        Assert.False((bool?)entry[1]!["value"]);
    }

    [Fact]
    public async Task NoStatesMeansNoScript()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => (string?)parameters["locator"]!["type"] == "css" ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("button-1"));

        int withEmptyStates = await page.GetByRole("button", states: new RoleStates()).CountAsync(TestContext.Current.CancellationToken);
        int withoutMatches = await page.Locate(new CssLocator("form")).GetByRole("button", states: new RoleStates() { Pressed = ToggleState.On }).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, withEmptyStates);
        Assert.Equal(0, withoutMatches);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task LabelLookupRunsInTheSandboxOverTheWholeDocument()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(NodeArray("email", "email", "name")));

        int count = await page.GetByLabel("Email", exact: true).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Contains("findElementsByLabel", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        JsonArray arguments = call["arguments"]!.AsArray();
        Assert.Equal("Email", (string?)arguments[0]!["value"]);
        Assert.True((bool?)arguments[1]!["value"]);
        Assert.Equal(2, arguments.Count);
    }

    [Fact]
    public async Task LabelLookupOnALocatorSearchesWithinItsMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("form-1", "form-2"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(NodeArray("input-1")));

        int count = await page.Locate(new CssLocator("form")).GetByLabel("name").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        JsonArray arguments = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!["arguments"]!.AsArray();
        Assert.False((bool?)arguments[1]!["value"]);
        Assert.Equal(["form-1", "form-2"], arguments.Skip(2).Select(argument => (string?)argument!["sharedId"]));
    }

    [Fact]
    public async Task LabelLookupWithinNoMatchesSendsNoScript()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        int count = await page.Locate(new CssLocator("form")).GetByLabel("name").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task LabelLookupOnAFrameSearchesThatFrame()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext frameContext = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(NodeArray()));

        await page.Frames[1].GetByLabel("name").CountAsync(TestContext.Current.CancellationToken);
        await page.Frames[1].GetByRole("button").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(frameContext.Id, (string?)Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!["target"]!["context"]);
        Assert.Equal(frameContext.Id, (string?)Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!["context"]);
    }

    [Fact]
    public async Task RoleStatesAndLabelsAreDescribed()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        RoleStates all = new() { Checked = ToggleState.Mixed, Pressed = ToggleState.Off, Expanded = true, Selected = false, Level = 3, Disabled = true };

        Assert.Equal("getByRole \"checkbox\" >> states(checked=mixed, pressed=off, expanded=true, selected=false, level=3, disabled=true)", page.GetByRole("checkbox", states: all).ToString());
        Assert.Equal("getByRole \"heading\" >> states(level=1)", page.GetByRole("heading", states: new RoleStates() { Level = 1 }).ToString());
        Assert.Equal("getByRole \"checkbox\" >> states(checked=on)", page.GetByRole("checkbox", states: new RoleStates() { Checked = ToggleState.On }).ToString());
        Assert.Equal("getByRole \"button\"", page.GetByRole("button", states: new RoleStates()).ToString());
        Assert.Equal("getByLabel \"Email\"", page.GetByLabel("Email").ToString());
        Assert.Equal("css \"form\" >> getByLabel \"Email\" exact", page.Locate(new CssLocator("form")).GetByLabel("Email", exact: true).ToString());
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static JsonObject NodeArray(params string[] sharedIds)
    {
        return new JsonObject() { ["type"] = "array", ["value"] = ProtocolJson.Nodes(sharedIds)["nodes"]!.DeepClone() };
    }
}
