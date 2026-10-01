// <copyright file="ShadowDomTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class ShadowDomTests
{
    [Fact]
    public async Task ShadowRootStepSearchesTheRootsTheBrowserSerializes()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => parameters.ContainsKey("startNodes") ? ProtocolJson.Nodes("button-1") : ProtocolJson.Nodes("host-1", "host-2", "host-3"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(Host("host-1", "root-1"), Host("host-2", null), Host("host-3", "root-3"))));

        int count = await page.Locate(new CssLocator("my-widget")).ShadowRoot().Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Equal("(...elements) => elements", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Group().Options.SandboxName, (string?)call["target"]!["sandbox"]);
        Assert.Equal("all", (string?)call["serializationOptions"]!["includeShadowTree"]);
        Assert.Equal(0, (int?)call["serializationOptions"]!["maxDomDepth"]);
        Assert.Equal(["host-1", "host-2", "host-3"], call["arguments"]!.AsArray().Select(argument => (string?)argument!["sharedId"]));
        Assert.Equal(["root-1", "root-3"], StartNodes(session.RemoteEnd.CommandsFor("browsingContext.locateNodes")[1]));
    }

    [Fact]
    public async Task ShadowRootOfNoMatchesSendsNoScript()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        int count = await page.Locate(new CssLocator("my-widget")).ShadowRoot().CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task ShadowRootIsDescribed()
    {
        (BiDiDriver driver, FakeSession _, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Equal("css \"my-widget\" >> shadowRoot >> css \"button\"", page.Locate(new CssLocator("my-widget")).ShadowRoot().Locate(new CssLocator("button")).ToString());
    }

    [Fact]
    public async Task LookupsDoNotPierceShadowRootsByDefault()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);

        Assert.False(new DramaturgeOptions().PierceShadowRoots);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
        Assert.False(Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!.AsObject().ContainsKey("startNodes"));
    }

    [Fact]
    public async Task PiercingSearchesTheDocumentAndItsOpenShadowRoots()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync(pierce: true);
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => IsRootsCall(parameters) ? ProtocolJson.Success(Array(Node("document-1"), Node("root-1"), Node("root-2"))) : ProtocolJson.Boolean(false));
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));

        bool visible = await page.Locate(new CssLocator("button")).IsVisibleAsync(TestContext.Current.CancellationToken);

        JsonObject rootsCall = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Contains("findOpenShadowRoots", (string?)rootsCall["functionDeclaration"]);
        Assert.False(rootsCall.ContainsKey("arguments"));
        JsonObject lookup = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!.AsObject();
        Assert.Equal(["document-1", "root-1", "root-2"], StartNodes(lookup));
        Assert.False(lookup.ContainsKey("maxNodeCount"));
        Assert.False(visible);
    }

    [Fact]
    public async Task PiercingWithoutShadowRootsLeavesTheLookupAsItWas()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync(pierce: true);
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "null" }));

        await page.Locate(new CssLocator("button")).IsVisibleAsync(TestContext.Current.CancellationToken);

        JsonObject lookup = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"))["params"]!.AsObject();
        Assert.False(lookup.ContainsKey("startNodes"));
        Assert.Equal(2, (int?)lookup["maxNodeCount"]);
    }

    [Fact]
    public async Task PiercingSearchesWithinTheStartNodesAndSkipsXPath()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync(pierce: true);
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => parameters.ContainsKey("startNodes") ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("form-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => IsRootsCall(parameters) && !parameters.ContainsKey("arguments")
            ? ProtocolJson.Success(new JsonObject() { ["type"] = "null" })
            : ProtocolJson.Success(Array(Node("form-1"), Node("root-1"))));

        await page.Locate(new CssLocator("form")).Locate(new CssLocator("input")).CountAsync(TestContext.Current.CancellationToken);
        await page.Locate(new XPathLocator("//form")).CountAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> calls = session.RemoteEnd.CommandsFor("script.callFunction");
        Assert.Equal(2, calls.Count);
        Assert.Equal(["form-1"], calls[1]["params"]!["arguments"]!.AsArray().Select(argument => (string?)argument!["sharedId"]));
        Assert.Equal(["form-1", "root-1"], StartNodes(session.RemoteEnd.CommandsFor("browsingContext.locateNodes")[1]["params"]!.AsObject()));
    }

    [Fact]
    public async Task PiercingExtendsLabelLookups()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync(pierce: true);
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => IsRootsCall(parameters)
            ? ProtocolJson.Success(Array(Node("document-1"), Node("root-1")))
            : ProtocolJson.Success(Array(Node("input-1"))));

        int count = await page.GetByLabel("Email").CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        JsonArray labelArguments = session.RemoteEnd.CommandsFor("script.callFunction")[1]["params"]!["arguments"]!.AsArray();
        Assert.Equal(["document-1", "root-1"], labelArguments.Skip(2).Select(argument => (string?)argument!["sharedId"]));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync(bool pierce = false)
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PierceShadowRoots = pierce }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static bool IsRootsCall(JsonObject parameters)
    {
        return ((string)parameters["functionDeclaration"]!).Contains("findOpenShadowRoots");
    }

    private static JsonObject Node(string sharedId)
    {
        return new JsonObject() { ["type"] = "node", ["sharedId"] = sharedId, ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } };
    }

    private static JsonObject Host(string sharedId, string? rootSharedId)
    {
        JsonObject host = Node(sharedId);
        if (rootSharedId is not null)
        {
            host["value"]!["shadowRoot"] = new JsonObject() { ["type"] = "node", ["sharedId"] = rootSharedId, ["value"] = new JsonObject() { ["nodeType"] = 11, ["childNodeCount"] = 1, ["mode"] = "closed" } };
        }

        return host;
    }

    private static JsonObject Array(params JsonObject[] values)
    {
        return new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. values]) };
    }

    private static IEnumerable<string?> StartNodes(JsonObject command)
    {
        JsonObject parameters = command.ContainsKey("params") ? command["params"]!.AsObject() : command;
        return parameters["startNodes"]!.AsArray().Select(node => (string?)node!["sharedId"]);
    }
}

internal static class PageTestExtensions
{
    public static BrowserGroup Group(this Page page) => page.Browser.Group;
}
