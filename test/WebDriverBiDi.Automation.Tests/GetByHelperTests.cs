// <copyright file="GetByHelperTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class GetByHelperTests
{
    public static TheoryData<string, bool, string> AttributeHelpers => new()
    {
        { "placeholder", false, "[placeholder*=\"Email\" i]" },
        { "placeholder", true, "[placeholder=\"Email\"]" },
        { "alt", false, "[alt*=\"Email\" i]" },
        { "alt", true, "[alt=\"Email\"]" },
        { "title", false, "[title*=\"Email\" i]" },
        { "title", true, "[title=\"Email\"]" },
    };

    [Theory]
    [MemberData(nameof(AttributeHelpers))]
    public async Task AttributeHelpersMatchByCss(string attribute, bool exact, string selector)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        ElementLocator locator = attribute switch
        {
            "placeholder" => page.GetByPlaceholder("Email", exact),
            "alt" => page.GetByAltText("Email", exact),
            _ => page.GetByTitle("Email", exact),
        };
        await locator.CountAsync(TestContext.Current.CancellationToken);

        JsonObject sent = LastLocator(session);
        Assert.Equal("css", (string?)sent["type"]);
        Assert.Equal(selector, (string?)sent["value"]);
    }

    [Theory]
    [InlineData("say \"hi\"", "[title*=\"say \\\"hi\\\"\" i]")]
    [InlineData("back\\slash", "[title*=\"back\\\\slash\" i]")]
    [InlineData("two\nlines\t", "[title*=\"two\\a lines\\9 \" i]")]
    [InlineData("", "[title]")]
    public async Task AttributeValuesAreEscapedForCss(string text, string selector)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.GetByTitle(text).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(selector, (string?)LastLocator(session)["value"]);
    }

    [Fact]
    public async Task EmptyExactTextMatchesAnEmptyValue()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.GetByAltText(string.Empty, exact: true).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal("[alt=\"\"]", (string?)LastLocator(session)["value"]);
    }

    [Theory]
    [InlineData(false, "partial", true)]
    [InlineData(true, "full", false)]
    public async Task TextHelperUsesTheInnerTextLocator(bool exact, string matchType, bool ignoreCase)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.GetByText("Sign in", exact).CountAsync(TestContext.Current.CancellationToken);

        JsonObject sent = LastLocator(session);
        Assert.Equal("innerText", (string?)sent["type"]);
        Assert.Equal("Sign in", (string?)sent["value"]);
        Assert.Equal(matchType, (string?)sent["matchType"]);
        Assert.Equal(ignoreCase, (bool?)sent["ignoreCase"]);
    }

    [Fact]
    public async Task RoleHelperUsesTheAccessibilityLocator()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.GetByRole("button", "Save").CountAsync(TestContext.Current.CancellationToken);
        JsonObject named = LastLocator(session);
        await page.GetByRole("link").CountAsync(TestContext.Current.CancellationToken);
        JsonObject unnamed = LastLocator(session);

        Assert.Equal("accessibility", (string?)named["type"]);
        Assert.Equal("button", (string?)named["value"]!["role"]);
        Assert.Equal("Save", (string?)named["value"]!["name"]);
        Assert.Equal("link", (string?)unnamed["value"]!["role"]);
        Assert.False(unnamed["value"]!.AsObject().ContainsKey("name"));
    }

    [Fact]
    public async Task TestIdHelperUsesTheConfiguredAttribute()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { TestIdAttribute = "data-qa" }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.GetByTestId("submit").CountAsync(TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("form")).GetByTestId("submit").CountAsync(TestContext.Current.CancellationToken);

        Assert.All(session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Where(command => !IsForm(command["params"]!.AsObject())), command => Assert.Equal("[data-qa=\"submit\"]", (string?)command["params"]!["locator"]!["value"]));
    }

    [Fact]
    public async Task HelpersOnALocatorSearchWithinItsMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => IsForm(parameters) ? ProtocolJson.Nodes("form-1") : ProtocolJson.Nodes(0));
        ElementLocator form = page.Locate(new CssLocator("form"));

        ElementLocator[] helpers =
        [
            form.GetByText("Name"),
            form.GetByPlaceholder("Name"),
            form.GetByAltText("Name"),
            form.GetByTitle("Name"),
            form.GetByTestId("name"),
            form.GetByRole("textbox", "Name"),
        ];
        foreach (ElementLocator helper in helpers)
        {
            await helper.CountAsync(TestContext.Current.CancellationToken);
        }

        IEnumerable<JsonObject> helperCommands = session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Where(command => !IsForm(command["params"]!.AsObject()));
        Assert.Equal(6, helperCommands.Count());
        Assert.All(helperCommands, command => Assert.Equal("form-1", (string?)command["params"]!["startNodes"]![0]!["sharedId"]));
    }

    [Fact]
    public async Task HelpersOnAFrameSearchThatFrame()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext frameContext = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        Frame frame = page.Frames[1];

        ElementLocator[] helpers =
        [
            frame.GetByText("Name"),
            frame.GetByPlaceholder("Name"),
            frame.GetByAltText("Name"),
            frame.GetByTitle("Name"),
            frame.GetByTestId("name"),
            frame.GetByRole("textbox"),
        ];
        foreach (ElementLocator helper in helpers)
        {
            Assert.Same(frame, helper.Frame);
            await helper.CountAsync(TestContext.Current.CancellationToken);
        }

        Assert.All(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"), command => Assert.Equal(frameContext.Id, (string?)command["params"]!["context"]));
    }

    [Fact]
    public async Task HelpersAreDescribedInTheirOwnTerms()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Equal("getByText \"Sign in\"", page.GetByText("Sign in").ToString());
        Assert.Equal("getByText \"Sign in\" exact", page.GetByText("Sign in", exact: true).ToString());
        Assert.Equal("getByPlaceholder \"Email\"", page.GetByPlaceholder("Email").ToString());
        Assert.Equal("getByAltText \"Logo\" exact", page.GetByAltText("Logo", exact: true).ToString());
        Assert.Equal("getByTitle \"Help\"", page.GetByTitle("Help").ToString());
        Assert.Equal("getByTestId \"submit\"", page.GetByTestId("submit").ToString());
        Assert.Equal("getByRole \"button\"", page.GetByRole("button").ToString());
        Assert.Equal("getByRole \"button\" name \"Save\"", page.GetByRole("button", "Save").ToString());
        Assert.Equal("css \"form\" >> getByRole \"button\"", page.Locate(new CssLocator("form")).GetByRole("button").ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("1data")]
    [InlineData("data testid")]
    [InlineData("data\"id")]
    [InlineData(null)]
    public void TestIdAttributeMustBeAnAttributeName(string? name)
    {
        Assert.Equal("TestIdAttribute", Assert.Throws<ArgumentException>(() => new AutomationOptions() { TestIdAttribute = name! }).ParamName);
    }

    [Fact]
    public void TestIdAttributeDefaultsToDataTestId()
    {
        Assert.Equal("data-testid", new AutomationOptions().TestIdAttribute);
        Assert.Equal("_qa-id2", new AutomationOptions() { TestIdAttribute = "_qa-id2" }.TestIdAttribute);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static bool IsForm(JsonObject parameters)
    {
        return parameters["locator"]!["value"] is JsonValue value && (string?)value == "form";
    }

    private static JsonObject LastLocator(FakeSession session)
    {
        return session.RemoteEnd.CommandsFor("browsingContext.locateNodes")[^1]["params"]!["locator"]!.AsObject();
    }
}
