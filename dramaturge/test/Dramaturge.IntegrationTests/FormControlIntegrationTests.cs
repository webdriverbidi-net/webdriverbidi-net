// <copyright file="FormControlIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class FormControlIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CheckAndUncheckChangeCheckboxesRadioButtonsAndCheckableRoles(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "forms.html");

        await page.Locate(new CssLocator("#agree")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#agree")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#subscribed")).UncheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#large")).SetCheckedAsync(true, cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#toggle")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "agree=true subscribed=false small=false large=true toggle=true",
            await EvaluateAsync(group, page.Id, "() => ['agree', 'subscribed', 'small', 'large'].map((id) => `${id}=${document.getElementById(id).checked}`).join(' ') + ` toggle=${document.getElementById('toggle').getAttribute('aria-checked')}`"));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CheckThatCannotHappenSaysWhy(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "forms.html");

        InvalidOperationException radio = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#small")).UncheckAsync(cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException button = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#plain")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException stuck = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#stuck")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"#small\" is a radio button, which clicking cannot uncheck.", radio.Message);
        Assert.Equal("css \"#plain\" is not a checkbox or radio button.", button.Message);
        Assert.Equal("Clicking css \"#stuck\" did not check it.", stuck.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SelectOptionChoosesByValueLabelAndIndexAndWaitsForOptions(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "forms.html");

        IReadOnlyList<string> color = await page.Locate(new CssLocator("#color")).SelectOptionAsync([SelectOption.ByLabel("Green")], cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<string> toppings = await page.Locate(new CssLocator("#toppings")).SelectOptionAsync([SelectOption.ByValue("cheese"), SelectOption.ByIndex(2)], cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<string> later = await page.Locate(new CssLocator("#later")).SelectOptionAsync([SelectOption.ByValue("loaded")], cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<string> none = await page.Locate(new CssLocator("#toppings")).SelectOptionAsync([], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["green"], color);
        Assert.Equal(["cheese", "peppers"], toppings);
        Assert.Equal(["loaded"], later);
        Assert.Empty(none);
        Assert.Equal(["input:green", "change:green"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SelectOptionThatCannotHappenSaysWhy(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "forms.html");

        WebDriverBiDiTimeoutException disabled = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.Locate(new CssLocator("#color")).SelectOptionAsync([SelectOption.ByValue("blue")], new ActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken));
        InvalidOperationException several = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#color")).SelectOptionAsync([SelectOption.ByValue("red"), SelectOption.ByValue("green")], cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException notSelect = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#plain")).SelectOptionAsync([SelectOption.ByIndex(0)], cancellationToken: TestContext.Current.CancellationToken));

        Assert.EndsWith("; the option matching value \"blue\" was disabled.", disabled.Message);
        Assert.Equal("css \"#color\" is a <select> element that takes one option, but 2 were given.", several.Message);
        Assert.Equal("css \"#plain\" is not a <select> element.", notSelect.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SetInputFilesSetsAndClearsTheFiles(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "forms.html");
        string directory = Directory.CreateTempSubdirectory("automation-files-").FullName;
        try
        {
            string first = Path.Combine(directory, "first.txt");
            string second = Path.Combine(directory, "second.txt");
            File.WriteAllText(first, "first");
            File.WriteAllText(second, "second");

            await page.Locate(new CssLocator("#uploads")).SetInputFilesAsync([first, second], cancellationToken: TestContext.Current.CancellationToken);
            await page.Locate(new CssLocator("#upload")).SetInputFilesAsync([first], cancellationToken: TestContext.Current.CancellationToken);
            string chosen = await FileNamesAsync(group, page, "upload");
            await page.Locate(new CssLocator("#upload")).SetInputFilesAsync([], cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal("first.txt,second.txt", await FileNamesAsync(group, page, "uploads"));
            Assert.Equal("first.txt", chosen);
            Assert.Equal(string.Empty, await FileNamesAsync(group, page, "upload"));
            await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Locate(new CssLocator("#upload")).SetInputFilesAsync([first, second], cancellationToken: TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server, string pageName)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor(pageName), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static Task<string> FileNamesAsync(BrowserGroup group, Page page, string id)
    {
        return EvaluateAsync(group, page.Id, $"() => [...document.getElementById('{id}').files].map((file) => file.name).join(',')");
    }

    private static async Task<string> EvaluateAsync(BrowserGroup group, string contextId, string function)
    {
        StringRemoteValue value = await group.Driver.Script.CallFunctionAsync<StringRemoteValue>(contextId, function, cancellationToken: TestContext.Current.CancellationToken);
        return value.Value;
    }

    private static async Task<string[]> EventsAsync(BrowserGroup group, Page page)
    {
        CollectionRemoteValue events = await group.Driver.Script.CallFunctionAsync<CollectionRemoteValue>(page.Id, "() => window.events", cancellationToken: TestContext.Current.CancellationToken);
        return [.. events.Value!.Select(value => value.As<StringRemoteValue>().Value)];
    }
}
