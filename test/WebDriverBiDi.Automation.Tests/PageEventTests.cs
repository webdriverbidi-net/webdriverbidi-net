// <copyright file="PageEventTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

public class PageEventTests
{
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ConsoleCallsAreReportedForTheFrameThatMadeThem()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        TaskCompletionSource<ConsoleMessageEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnConsoleMessage.AddObserver(e => received.TrySetResult(e));

        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", ConsoleEntry(child.Id, "warn", "careful", String("careful")));
        ConsoleMessageEventArgs message = await received.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Same(page.Frames[1], message.Frame);
        Assert.Same(page, message.Page);
        Assert.Equal("warn", message.Method);
        Assert.Equal(LogLevel.Warn, message.Level);
        Assert.Equal("careful", message.Text);
        Assert.Equal("careful", Assert.Single(message.Arguments).As<StringRemoteValue>().Value);
    }

    [Fact]
    public async Task UncaughtErrorsAreReportedAsPageErrors()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<PageErrorEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnPageError.AddObserver(e => received.TrySetResult(e));

        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", Entry("javascript", "error", page.Id, "Error: boom"));
        PageErrorEventArgs error = await received.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal("Error: boom", error.Message);
        Assert.Same(page, error.Page);
        Assert.Same(page.MainFrame, error.Frame);
    }

    [Fact]
    public async Task EntriesWithoutAPageFrameOrOfOtherKindsAreNotReported()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> reported = [];
        TaskCompletionSource<bool> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnConsoleMessage.AddObserver(e =>
        {
            reported.Add($"console:{e.Text}");
            if (e.Text == "done")
            {
                done.TrySetResult(true);
            }
        });
        page.OnPageError.AddObserver(e => reported.Add($"error:{e.Message}"));

        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", ConsoleEntry(null, "log", "worker"));
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", ConsoleEntry("untracked-context", "log", "elsewhere"));
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", Entry("custom", "info", page.Id, "other kind"));
        JsonObject withoutText = ConsoleEntry(page.Id, "log", "ignored");
        withoutText.Remove("text");
        withoutText["text"] = null;
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", withoutText);
        JsonObject errorWithoutText = Entry("javascript", "error", page.Id, "ignored");
        errorWithoutText["text"] = null;
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", errorWithoutText);
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", ConsoleEntry(page.Id, "log", "done"));
        await done.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(["console:", "error:", "console:done"], reported);
    }

    [Fact]
    public async Task DialogsLeftOpenAreAnsweredFromTheObserver()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<Dialog> answered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDialog.AddObserver(async e =>
        {
            await e.Dialog.AcceptAsync("Grace");
            await e.Dialog.DismissAsync();
            answered.TrySetResult(e.Dialog);
        });

        await session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt(page.Id, "prompt", "ignore", "Name?", "Ada"));
        Dialog dialog = await answered.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(UserPromptType.Prompt, dialog.Type);
        Assert.Equal("Name?", dialog.Message);
        Assert.Equal("Ada", dialog.DefaultValue);
        Assert.Equal(UserPromptHandlerType.Ignore, dialog.Handler);
        Assert.Same(page, dialog.Page);
        Assert.Same(page.MainFrame, dialog.Frame);
        IReadOnlyList<JsonObject> handled = session.RemoteEnd.CommandsFor("browsingContext.handleUserPrompt");
        Assert.Equal(page.Id, (string?)handled[0]["params"]!["context"]);
        Assert.True((bool?)handled[0]["params"]!["accept"]);
        Assert.Equal("Grace", (string?)handled[0]["params"]!["userText"]);
        Assert.False((bool?)handled[1]["params"]!["accept"]);
    }

    [Theory]
    [InlineData("accept", "accepted")]
    [InlineData("dismiss", "dismissed")]
    public async Task DialogsTheBrowserHandledCannotBeAnswered(string handler, string handled)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<Dialog> opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDialog.AddObserver(e => opened.TrySetResult(e.Dialog));

        await session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt(page.Id, "alert", handler, "Hi", null));
        Dialog dialog = await opened.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dialog.AcceptAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal($"The browser {handled} the dialog itself, as its user prompt handler says; to answer dialogs, set the handler to ignore, such as with BrowserOptions.UnhandledPromptBehavior.", exception.Message);
        Assert.Null(dialog.DefaultValue);
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.handleUserPrompt"));
    }

    [Fact]
    public async Task DialogsFromUntrackedContextsAreNotReported()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> messages = [];
        TaskCompletionSource<bool> tracked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDialog.AddObserver(e =>
        {
            messages.Add(e.Dialog.Message);
            tracked.TrySetResult(true);
        });

        await session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt("untracked-context", "alert", "ignore", "Elsewhere", null));
        await session.RemoteEnd.RaiseEventAsync("browsingContext.userPromptOpened", Prompt(page.Id, "alert", "ignore", "Here", null));
        await tracked.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(["Here"], messages);
    }

    [Fact]
    public async Task PopupsAreReportedToTheirOpenerAfterTheBrowserTracksThem()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> order = [];
        TaskCompletionSource<Page> popped = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.Browser.OnPageCreated.AddObserver(e => order.Add("created"));
        page.OnPopup.AddObserver(async e =>
        {
            order.Add("popup");
            await e.Page.NavigateAsync("https://example.com/popup");
            popped.TrySetResult(e.Page);
        });

        FakeContext popupContext = session.AddContext();
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", CreatedContext(popupContext.Id, page.Id));
        Page popup = await popped.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(["created", "popup"], order);
        Assert.Same(page, popup.Opener);
        Assert.Null(page.Opener);
        Assert.Equal("https://example.com/popup", popup.Url);
        Assert.Contains(popup, page.Browser.Pages);
    }

    [Fact]
    public async Task PagesOpenedByAnUntrackedContextHaveNoOpener()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<Page> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.Browser.OnPageCreated.AddObserver(e => created.TrySetResult(e.Page));

        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", CreatedContext("popup-2", "untracked-context"));
        Page other = await created.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Null(other.Opener);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static JsonObject Entry(string type, string level, string? contextId, string text)
    {
        JsonObject source = new() { ["realm"] = "realm-1" };
        if (contextId is not null)
        {
            source["context"] = contextId;
        }

        return new JsonObject() { ["type"] = type, ["level"] = level, ["source"] = source, ["text"] = text, ["timestamp"] = 1790000000000 };
    }

    private static JsonObject ConsoleEntry(string? contextId, string method, string text, params JsonObject[] arguments)
    {
        JsonObject entry = Entry("console", method == "warn" ? "warn" : "info", contextId, text);
        entry["method"] = method;
        entry["args"] = new JsonArray([.. arguments]);
        return entry;
    }

    private static JsonObject Prompt(string contextId, string type, string handler, string message, string? defaultValue)
    {
        JsonObject prompt = new() { ["context"] = contextId, ["type"] = type, ["handler"] = handler, ["message"] = message, ["userContext"] = "default" };
        if (defaultValue is not null)
        {
            prompt["defaultValue"] = defaultValue;
        }

        return prompt;
    }

    private static JsonObject CreatedContext(string contextId, string openerId)
    {
        return new JsonObject()
        {
            ["context"] = contextId,
            ["clientWindow"] = $"window-for-{contextId}",
            ["originalOpener"] = openerId,
            ["url"] = "about:blank",
            ["userContext"] = "default",
            ["children"] = null,
        };
    }

    private static JsonObject String(string value) => new() { ["type"] = "string", ["value"] = value };
}
