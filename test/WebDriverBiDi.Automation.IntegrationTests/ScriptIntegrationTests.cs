// <copyright file="ScriptIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Numerics;
using Dramaturge.Browsers;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class ScriptIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task EvaluateConvertsResultsFromThePagesOwnRealm(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;
        await page.EvaluateAsync("() => { window.pageGlobal = 'visible'; }", cancellationToken: token);

        Assert.Equal("visible", await page.EvaluateAsync<string>("() => window.pageGlobal", cancellationToken: token));
        Assert.Equal(5, await page.EvaluateAsync<int>("(a, b) => a + b", [LocalValue.Number(2), LocalValue.Number(3)], cancellationToken: token));
        Assert.Equal(new BigInteger(9007199254740993), await page.EvaluateAsync<BigInteger>("() => 9007199254740993n", cancellationToken: token));
        int[] numbers = await page.EvaluateAsync<int[]>("() => [1, 2, 3]", cancellationToken: token);
        List<string> letters = await page.EvaluateAsync<List<string>>("() => new Set(['a', 'b'])", cancellationToken: token);
        Assert.Equal([1, 2, 3], numbers);
        Assert.Equal(["a", "b"], letters);
        Assert.Equal(new Dictionary<string, double[]>() { ["x"] = [1.5] }, await page.EvaluateAsync<Dictionary<string, double[]>>("() => ({ x: [1.5] })", cancellationToken: token));
        Assert.Null(await page.EvaluateAsync<int?>("() => undefined", cancellationToken: token));
        Assert.True(await page.EvaluateAsync<bool>("async () => { await new Promise((resolve) => setTimeout(resolve, 10)); return true; }", cancellationToken: token));
        object? tree = await page.EvaluateAsync<object?>("() => ({ name: 'n', list: [1, null] })", cancellationToken: token);
        Dictionary<string, object?> dictionary = Assert.IsType<Dictionary<string, object?>>(tree);
        Assert.Equal("n", dictionary["name"]);
        List<object?> list = Assert.IsType<List<object?>>(dictionary["list"]);
        Assert.Equal([1.0, null], list);
        Assert.IsType<NodeRemoteValue>(await page.EvaluateAsync("() => document.body", cancellationToken: token));
        await Assert.ThrowsAsync<ScriptException>(() => page.EvaluateAsync("() => { throw new Error('boom'); }", cancellationToken: token));
        await Assert.ThrowsAsync<InvalidCastException>(() => page.EvaluateAsync<int>("() => 1.5", cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task EvaluateOnALocatorPassesTheElement(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        string value = await page.Locate(new CssLocator("#item")).EvaluateAsync<string>("(element, name) => element.dataset[name]", [LocalValue.String("id")], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("42", value);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitForFunctionWaitsForATruthyValue(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        await page.Locate(new CssLocator("#ready")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        string ready = await page.WaitForFunctionAsync<string>("() => window.readiness", cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.WaitForFunctionAsync("() => 0", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("yes", ready);
        Assert.Equal("Timed out after 1 seconds waiting for the function to return a truthy value; it last returned 0.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task InitScriptsRunBeforeTheDocumentsOwnScriptsUntilRemoved(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        CancellationToken token = TestContext.Current.CancellationToken;

        InitScript script = await page.AddInitScriptAsync("() => { window.seed = 42; }", token);
        await page.NavigateAsync(server.UrlFor("script.html"), cancellationToken: token);
        double? seeded = await page.EvaluateAsync<double?>("() => window.seenSeed", cancellationToken: token);
        await script.RemoveAsync(token);
        await page.ReloadAsync(cancellationToken: token);
        double? removed = await page.EvaluateAsync<double?>("() => window.seenSeed", cancellationToken: token);

        Assert.Equal(42, seeded);
        Assert.Null(removed);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SetContentReplacesTheDocumentAndWaitsForItToLoad(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        await page.SetContentAsync($"<p id=\"written\">Written</p><img src=\"{server.UrlFor("gated.gif")}\">", ReadinessState.Interactive, cancellationToken: token);
        string parsed = await page.Locate(new CssLocator("#written")).TextContentAsync(cancellationToken: token);
        server.ReleaseGatedResource();
        await page.WaitForLoadStateAsync(cancellationToken: token);
        await page.SetContentAsync("<p id=\"second\">Second</p>", cancellationToken: token);

        Assert.Equal("Written", parsed);
        Assert.Equal("Second", await page.Locate(new CssLocator("#second")).TextContentAsync(cancellationToken: token));
        Assert.Equal("complete", await page.EvaluateAsync<string>("() => document.readyState", cancellationToken: token));
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("script.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }
}
