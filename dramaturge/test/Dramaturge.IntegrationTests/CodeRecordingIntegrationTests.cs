// <copyright file="CodeRecordingIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;

public class CodeRecordingIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task UsersActionsAreRecordedAsStatements(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("recording.html"), cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await group.DefaultBrowser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        List<string> statements = [];
        TaskCompletionSource allSettled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnStatement.AddObserver(e =>
        {
            statements.Add(e.Statement);
            if (statements.Count == 4)
            {
                allSettled.TrySetResult();
            }
        });

        await page.Locate(new CssLocator("#go")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#name")).PressSequentiallyAsync("Ada", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#agree")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#done")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Using the toolbar settles the last action, once its messages, sent before, have arrived.
        await ClickToolbarAsync(page, "Record");
        await allSettled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#go\")).ClickAsync();",
                "await page.Locate(new CssLocator(\"#name\")).FillAsync(\"Ada\");",
                "await page.Locate(new CssLocator(\"#agree\")).CheckAsync();",
                "await page.Locate(new CssLocator(\"#done\")).ClickAsync();",
            ],
            statements);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ToolbarPicksLocatorsAndAddsAssertions(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("recording.html"), cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await group.DefaultBrowser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        List<string> statements = [];
        TaskCompletionSource allSettled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnStatement.AddObserver(e =>
        {
            statements.Add(e.Statement);
            if (statements.Count == 9)
            {
                allSettled.TrySetResult();
            }
        });
        TaskCompletionSource<LocatorPickedEventArgs> picked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnLocatorPicked.AddObserver(e => picked.TrySetResult(e));
        ElementLocator message = page.Locate(new CssLocator("#message"));

        await page.Locate(new CssLocator("#go")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Assert visible");
        await message.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Assert text");
        await message.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Keyboard.PressAsync(Keys.Enter, cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#name")).PressSequentiallyAsync("Ada", cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Assert value");
        await page.Locate(new CssLocator("#name")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#agree")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Assert value");
        await page.Locate(new CssLocator("#agree")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Assert snapshot");
        await page.Locate(new CssLocator("#list > li")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Pick locator");
        await page.Locate(new CssLocator("#go")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Record");
        await page.Locate(new CssLocator("#done")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Record");
        await page.Locate(new CssLocator("#done")).DblClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await ClickToolbarAsync(page, "Record");
        await allSettled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        LocatorPickedEventArgs pick = await picked.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#go\")).ClickAsync();",
                "await Expect(page.Locate(new CssLocator(\"#message\"))).ToBeVisibleAsync();",
                "await Expect(page.Locate(new CssLocator(\"#message\"))).ToContainTextAsync(\"Hello\");",
                "await page.Locate(new CssLocator(\"#name\")).FillAsync(\"Ada\");",
                "await Expect(page.Locate(new CssLocator(\"#name\"))).ToHaveValueAsync(\"Ada\");",
                "await page.Locate(new CssLocator(\"#agree\")).CheckAsync();",
                "await Expect(page.Locate(new CssLocator(\"#agree\"))).ToBeCheckedAsync();",
                "await Expect(page.Locate(new CssLocator(\"#list > li\"))).ToMatchAriaSnapshotAsync(\"\"\"\n    - listitem: One\n    \"\"\");",
                "await page.Locate(new CssLocator(\"#done\")).DblClickAsync();",
            ],
            statements);
        Assert.Equal("page.Locate(new CssLocator(\"#go\"))", pick.Code);
        Assert.Same(page, pick.Page);
        Assert.Equal(1, await pick.Locator.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await page.EvaluateAsync<double>("() => document.querySelectorAll('dramaturge-toolbar').length", cancellationToken: TestContext.Current.CancellationToken));
    }

    // The toolbar's six buttons share its width equally, in this order.
    private static async Task ClickToolbarAsync(Page page, string button)
    {
        string[] buttons = ["Record", "Pick locator", "Assert visible", "Assert text", "Assert value", "Assert snapshot"];
        string json = await page.EvaluateAsync<string>("() => JSON.stringify(document.querySelector('dramaturge-toolbar').getBoundingClientRect())", cancellationToken: TestContext.Current.CancellationToken);
        using JsonDocument rect = JsonDocument.Parse(json);
        double left = rect.RootElement.GetProperty("left").GetDouble();
        double width = rect.RootElement.GetProperty("width").GetDouble();
        double top = rect.RootElement.GetProperty("top").GetDouble();
        double height = rect.RootElement.GetProperty("height").GetDouble();
        await page.Mouse.ClickAsync(left + (width * (Array.IndexOf(buttons, button) + 0.5) / buttons.Length), top + (height / 2), cancellationToken: TestContext.Current.CancellationToken);
    }
}
