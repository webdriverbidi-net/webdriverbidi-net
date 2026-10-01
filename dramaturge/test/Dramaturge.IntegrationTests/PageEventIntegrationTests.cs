// <copyright file="PageEventIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Collections.Concurrent;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

public class PageEventIntegrationTests
{
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ConsoleMessagesAndUncaughtErrorsAreReported(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group.DefaultBrowser, server);
        ConcurrentQueue<ConsoleMessageEventArgs> messages = new();
        TaskCompletionSource<bool> bothLogged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<PageErrorEventArgs> error = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnConsoleMessage.AddObserver(e =>
        {
            messages.Enqueue(e);
            if (messages.Count == 2)
            {
                bothLogged.TrySetResult(true);
            }
        });
        page.OnPageError.AddObserver(e => error.TrySetResult(e));

        await page.Locate(new CssLocator("#log")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#throw")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await bothLogged.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        PageErrorEventArgs thrown = await error.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        ConsoleMessageEventArgs[] logged = [.. messages];
        Assert.Equal(["log:Info:hello 42", "warn:Warn:careful"], logged.Select(message => $"{message.Method}:{message.Level}:{message.Text}"));
        Assert.Equal(2, logged[0].Arguments.Count);
        Assert.Same(page.MainFrame, logged[0].Frame);
        Assert.Contains("boom", thrown.Message);
        Assert.Same(page, thrown.Page);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DialogsLeftOpenAreAnsweredFromTheObserver(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox ignores the unhandledPromptBehavior parameter of browser.createUserContext (https://bugzilla.mozilla.org/show_bug.cgi?id=1975279).");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = await group.CreateBrowserAsync(new BrowserOptions() { UnhandledPromptBehavior = new UserPromptHandler() { Default = UserPromptHandlerType.Ignore } }, TestContext.Current.CancellationToken);
        Page page = await OpenAsync(browser, server);
        ConcurrentQueue<string> seen = new();
        page.OnDialog.AddObserver(async e =>
        {
            Dialog dialog = e.Dialog;
            seen.Enqueue($"{dialog.Type}:{dialog.Message}:{dialog.DefaultValue}:{dialog.Frame.IsMainFrame}");
            switch (dialog.Type)
            {
                case UserPromptType.Prompt:
                    await dialog.AcceptAsync("Grace");
                    break;
                case UserPromptType.Confirm:
                    await dialog.AcceptAsync();
                    break;
                default:
                    await dialog.DismissAsync();
                    break;
            }
        });
        Frame frame = await page.Locate(new CssLocator("#frame")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.Locate(new CssLocator("#alert")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#confirm")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#prompt")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await frame.Locate(new CssLocator("#child-confirm")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Alert:Hi::True", "Confirm:Sure?::True", "Prompt:Name?:Ada:True", "Confirm:From the frame?::False"], seen);
        Assert.True(await page.EvaluateAsync<bool>("() => window.confirmed", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("Grace", await page.EvaluateAsync<string>("() => window.prompted", cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(await frame.EvaluateAsync<bool>("() => window.confirmed", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DialogsTheSessionLeavesOpenAreAnsweredFromTheObserver(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind, configure: builder => builder.WithSessionCapability("unhandledPromptBehavior", new UserPromptHandler() { Default = UserPromptHandlerType.Ignore }));
        Page page = await OpenAsync(group.DefaultBrowser, server);
        List<UserPromptHandlerType> handlers = [];
        page.OnDialog.AddObserver(async e =>
        {
            handlers.Add(e.Dialog.Handler);
            await e.Dialog.AcceptAsync();
        });

        await page.Locate(new CssLocator("#confirm")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([UserPromptHandlerType.Ignore], handlers);
        Assert.True(await page.EvaluateAsync<bool>("() => window.confirmed", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DialogsTheBrowserHandlesAreReportedAndCannotBeAnswered(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox ignores the unhandledPromptBehavior parameter of browser.createUserContext (https://bugzilla.mozilla.org/show_bug.cgi?id=1975279).");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = await group.CreateBrowserAsync(new BrowserOptions() { UnhandledPromptBehavior = new UserPromptHandler() { Default = UserPromptHandlerType.Accept } }, TestContext.Current.CancellationToken);
        Page page = await OpenAsync(browser, server);
        TaskCompletionSource<Dialog> opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDialog.AddObserver(e => opened.TrySetResult(e.Dialog));

        await page.Locate(new CssLocator("#confirm")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        Dialog dialog = await opened.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dialog.DismissAsync(TestContext.Current.CancellationToken));

        Assert.Equal(UserPromptHandlerType.Accept, dialog.Handler);
        Assert.StartsWith("The browser accepted the dialog itself", exception.Message);
        Assert.True(await page.EvaluateAsync<bool>("() => window.confirmed", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PopupsAreReportedToTheirOpenerAndCanBeUsedFromTheObserver(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group.DefaultBrowser, server);
        TaskCompletionSource<Page> loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnPopup.AddObserver(async e =>
        {
            await e.Page.WaitForUrlAsync(server.UrlFor("second.html"));
            loaded.TrySetResult(e.Page);
        });

        await page.Locate(new CssLocator("#popup")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page popup = await loaded.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Same(page, popup.Opener);
        Assert.Null(page.Opener);
        Assert.Contains(popup, group.DefaultBrowser.Pages);
    }

    private static async Task<Page> OpenAsync(Browser browser, TestPageServer server)
    {
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("events.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }
}
