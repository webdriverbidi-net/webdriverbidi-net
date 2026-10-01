// <copyright file="ReadTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class ReadTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task IsHiddenIsTheOppositeOfIsVisibleWithoutWaiting()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] lookups = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => Interlocked.Increment(ref lookups[0]) == 1 ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Boolean(true));

        Assert.True(await page.Locate(new CssLocator("div")).IsHiddenAsync(TestContext.Current.CancellationToken));
        Assert.False(await page.Locate(new CssLocator("div")).IsHiddenAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("enabled", true)]
    [InlineData("disabled", false)]
    public async Task IsEnabledAndIsDisabledReadTheEnabledState(string received, bool enabled)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Received(received));

        Assert.Equal(enabled, await page.Locate(new CssLocator("button")).IsEnabledAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(!enabled, await page.Locate(new CssLocator("button")).IsDisabledAsync(cancellationToken: TestContext.Current.CancellationToken));

        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Equal("button-1", (string?)call["arguments"]![0]!["sharedId"]);
        Assert.Contains("inspector.queryElementState(element, 'enabled')", (string?)call["functionDeclaration"]);
    }

    [Fact]
    public async Task StateReadLooksAgainForAnElementRemovedWhileItWasRead()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Received("error:notconnected"), Received("enabled"));

        Assert.True(await DriveAsync(time, page.Locate(new CssLocator("button")).IsEnabledAsync(cancellationToken: TestContext.Current.CancellationToken)));
        Assert.Equal(2, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
    }

    [Theory]
    [InlineData("editable", true)]
    [InlineData("readOnly", false)]
    [InlineData("disabled", false)]
    public async Task IsEditableReadsTheEditableState(string received, bool editable)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, Received(received));

        Assert.Equal(editable, await page.Locate(new CssLocator("input")).IsEditableAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IsEditableAndIsCheckedRejectElementsWithoutTheState()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ProtocolJson.Success(Received(((string)parameters["functionDeclaration"]!).Contains("'checked'") ? "error:notcheckable" : "error:noteditable")));

        InvalidOperationException editable = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("div")).IsEditableAsync(cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException checkable = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("div")).IsCheckedAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"div\" is not an editable element.", editable.Message);
        Assert.Equal("css \"div\" is not a checkbox or radio button.", checkable.Message);
    }

    [Theory]
    [InlineData("checked", true)]
    [InlineData("unchecked", false)]
    [InlineData("indeterminate", false)]
    public async Task IsCheckedCountsOnlyTheCheckedState(string received, bool isChecked)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        AnswerScripts(session, Object(("received", String(received)), ("isRadio", new JsonObject() { ["type"] = "boolean", ["value"] = false })));

        Assert.Equal(isChecked, await page.Locate(new CssLocator("input")).IsCheckedAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TextIsReadByTheElementStateScriptAndHtmlInTheSandbox()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("p-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ((string)parameters["functionDeclaration"]!).Contains("state.readTexts(elements, useInnerText)")
            ? ProtocolJson.Success(Array(String((bool)parameters["arguments"]![0]!["value"]! ? "rendered" : "content")))
            : ProtocolJson.Success(String((string)parameters["functionDeclaration"]!)));

        Assert.Equal("content", await page.Locate(new CssLocator("p")).TextContentAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("rendered", await page.Locate(new CssLocator("p")).InnerTextAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("(element) => element.innerHTML", await page.Locate(new CssLocator("p")).InnerHtmlAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.All(session.RemoteEnd.CommandsFor("script.callFunction"), call =>
        {
            Assert.Equal(page.Group().Options.SandboxName, (string?)call["params"]!["target"]!["sandbox"]);
            Assert.Contains(call["params"]!["arguments"]!.AsArray(), argument => (string?)argument!["sharedId"] == "p-1");
        });
    }

    [Fact]
    public async Task InnerTextRejectsElementsThatAreNotHtml()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("text-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(Null())));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("text")).InnerTextAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"text\" is not an HTML element.", exception.Message);
    }

    [Fact]
    public async Task InputValueIsReadByTheElementStateScript()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("control-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String("Ada")));

        Assert.Equal("Ada", await page.Locate(new CssLocator("#control")).InputValueAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("state.readValue(element)", (string?)Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!["functionDeclaration"]);
    }

    [Fact]
    public async Task InputValueRejectsOtherElements()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Null()));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("div")).InputValueAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"div\" is not an input, a text area, or a select.", exception.Message);
    }

    [Fact]
    public async Task GetAttributeReadsTheAttributeOrNull()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => (string?)parameters["arguments"]![1]!["value"] == "data-role"
            ? ProtocolJson.Success(String("person"))
            : ProtocolJson.Success(Null()));

        Assert.Equal("person", await page.Locate(new CssLocator("input")).GetAttributeAsync("data-role", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Null(await page.Locate(new CssLocator("input")).GetAttributeAsync("title", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("state.readAttribute(element, name)", (string?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["functionDeclaration"]);
    }

    [Fact]
    public async Task BoundingBoxReadsTheBoxOrNullForAnElementThatIsNotVisible()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => Interlocked.Increment(ref calls[0]) == 1
            ? ProtocolJson.Success(Object(("x", Number(20.5)), ("y", Number(30)), ("width", Number(100)), ("height", Number(50))))
            : ProtocolJson.Success(Null()));

        Assert.Equal(new BoundingBox(page.MainFrame, 20.5, 30, 100, 50), await page.Locate(new CssLocator("div")).BoundingBoxAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Null(await page.Locate(new CssLocator("div")).BoundingBoxAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("inspector.isElementVisible(element)", (string?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["functionDeclaration"]);
    }

    [Fact]
    public async Task ScreenshotWaitsForAVisibleStableElementThenCapturesItFromTheDocument()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        AnswerScripts(session, Object(("status", String("failure")), ("missingState", String("hidden"))), Object(("status", String("success"))));
        session.RemoteEnd.AnswerWith("browsingContext.captureScreenshot", new JsonObject() { ["data"] = Convert.ToBase64String([1, 2, 3]) });

        byte[] image = await DriveAsync(time, page.Locate(new CssLocator("div")).ScreenshotAsync(new ImageFormat() { Type = "image/jpeg", Quality = 0.5 }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([1, 2, 3], image);
        Assert.Contains("['stable', 'visible']", (string?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["functionDeclaration"]);
        JsonObject parameters = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.captureScreenshot"))["params"]!.AsObject();
        Assert.Equal(page.Id, (string?)parameters["context"]);
        Assert.Equal("element", (string?)parameters["clip"]!["type"]);
        Assert.Equal("div-1", (string?)parameters["clip"]!["element"]!["sharedId"]);
        Assert.Equal("document", (string?)parameters["origin"]);
        Assert.Equal("image/jpeg", (string?)parameters["format"]!["type"]);
    }

    [Theory]
    [InlineData(0, "success", "no element matched")]
    [InlineData(1, "error", "the element was removed from the document")]
    public async Task ReadThatTimesOutSaysWhy(int matches, string status, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        AnswerScripts(session, Object(("status", String(status)), ("message", String("notconnected"))));

        Task<byte[]> screenshot = page.Locate(new CssLocator("div")).ScreenshotAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, screenshot));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"div\" to be attached; {observed}.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.captureScreenshot"));
    }

    [Fact]
    public async Task ReadsRejectSeveralMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("p")).TextContentAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Answers each script with the next result in turn, repeating the last.
    private static void AnswerScripts(FakeSession session, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => ProtocolJson.Success((JsonObject)results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone()));
    }

    private static JsonObject Array(params JsonObject[] items)
    {
        return new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. items]) };
    }

    private static JsonObject Null()
    {
        return new JsonObject() { ["type"] = "null" };
    }

    private static JsonObject Object(params (string Name, JsonNode Value)[] properties)
    {
        return new JsonObject() { ["type"] = "object", ["value"] = new JsonArray([.. properties.Select(property => (JsonNode)new JsonArray(property.Name, property.Value))]) };
    }

    private static JsonObject Received(string received)
    {
        return Object(("matches", new JsonObject() { ["type"] = "boolean", ["value"] = false }), ("received", String(received)));
    }

    private static JsonObject String(string value)
    {
        return new JsonObject() { ["type"] = "string", ["value"] = value };
    }

    private static JsonObject Number(double value)
    {
        return new JsonObject() { ["type"] = "number", ["value"] = value };
    }

    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        return await operation;
    }
}
