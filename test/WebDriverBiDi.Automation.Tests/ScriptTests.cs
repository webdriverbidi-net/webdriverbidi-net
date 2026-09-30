// <copyright file="ScriptTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class ScriptTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task EvaluateCallsTheFunctionInThePagesOwnRealm()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Number(5)));

        RemoteValue raw = await page.EvaluateAsync("(a, b) => a + b", [LocalValue.Number(2), LocalValue.Number(3)], cancellationToken: TestContext.Current.CancellationToken);
        int converted = await page.EvaluateAsync<int>("() => 5", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        int waited = await page.WaitForFunctionAsync<int>("() => 5", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(5, raw.As<NumberRemoteValue>().Value);
        Assert.Equal(5, converted);
        Assert.Equal(5, waited);
        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Equal("(a, b) => a + b", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        Assert.False(call["target"]!.AsObject().ContainsKey("sandbox"));
        Assert.True((bool?)call["awaitPromise"]);
        Assert.Equal([2, 3], call["arguments"]!.AsArray().Select(argument => (int?)argument!["value"]));
    }

    [Fact]
    public async Task EvaluateInAFrameCallsItsContext()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String("child")));

        string title = await page.Frames[1].EvaluateAsync<string>("() => document.title", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("child", title);
        Assert.Equal(child.Id, (string?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["target"]!["context"]);
    }

    [Fact]
    public async Task EvaluateOnALocatorPassesTheElementFirst()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] lookups = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => Interlocked.Increment(ref lookups[0]) == 1 ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("item-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String("42")));

        string id = await DriveAsync(time, page.Locate(new CssLocator("#item")).EvaluateAsync<string>("(element, name) => element.dataset[name]", [LocalValue.String("id")], cancellationToken: TestContext.Current.CancellationToken));
        RemoteValue raw = await page.Locate(new CssLocator("#item")).EvaluateAsync("(element) => element.id", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("42", id);
        Assert.IsType<StringRemoteValue>(raw);
        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.False(call["target"]!.AsObject().ContainsKey("sandbox"));
        Assert.Equal("item-1", (string?)call["arguments"]![0]!["sharedId"]);
        Assert.Equal("id", (string?)call["arguments"]![1]!["value"]);
        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction")[1]["params"]!["arguments"]!.AsArray());
    }

    [Fact]
    public async Task WaitForFunctionCallsAgainUntilTheValueIsTruthy()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, ProtocolJson.Success(Null()), ProtocolJson.Success(Boolean(false)), ProtocolJson.Success(Object()));

        RemoteValue value = await DriveAsync(time, page.WaitForFunctionAsync("(name) => window[name]", [LocalValue.String("ready")], cancellationToken: TestContext.Current.CancellationToken));

        Assert.IsType<KeyValuePairCollectionRemoteValue>(value);
        IReadOnlyList<JsonObject> calls = session.RemoteEnd.CommandsFor("script.callFunction");
        Assert.Equal(3, calls.Count);
        Assert.All(calls, call => Assert.Equal("ready", (string?)call["params"]!["arguments"]![0]!["value"]));
        Assert.False(calls[0]["params"]!["target"]!.AsObject().ContainsKey("sandbox"));
    }

    [Theory]
    [InlineData("null", null, "null")]
    [InlineData("undefined", null, "undefined")]
    [InlineData("boolean", "false", "false")]
    [InlineData("number", "0", "0")]
    [InlineData("number", "NaN", "NaN")]
    [InlineData("string", "", "an empty string")]
    [InlineData("bigint", "0", "0n")]
    public async Task WaitForFunctionThatTimesOutSaysWhatItLastReturned(string type, string? value, string described)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject result = new() { ["type"] = type };
        if (value is not null)
        {
            result["value"] = type == "boolean" ? false : type == "number" && value == "0" ? 0 : value;
        }

        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(result));

        Task<string> wait = page.WaitForFunctionAsync<string>("() => value", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, wait));

        Assert.Equal($"Timed out after 1 seconds waiting for the function to return a truthy value; it last returned {described}.", exception.Message);
    }

    [Theory]
    [InlineData("number", 1)]
    [InlineData("string", "x")]
    [InlineData("bigint", "1")]
    public async Task TruthyValuesEndTheWait(string type, object value)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = type, ["value"] = JsonValue.Create(value) }));

        await page.MainFrame.WaitForFunctionAsync("() => value", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task WaitForFunctionCallsAgainWhenTheFrameNavigatesDuringACall()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => Interlocked.Increment(ref calls[0]) == 1
            ? FakeResponse.Failure("unknown error", "Inspected target navigated or closed", ("browsingContext.navigationStarted", new JsonObject() { ["context"] = page.Id, ["navigation"] = "navigation-2", ["timestamp"] = 1790000000000, ["url"] = "https://example.com/next" }))
            : new FakeResponse(ProtocolJson.Success(Boolean(true))));

        RemoteValue value = await DriveAsync(time, page.WaitForFunctionAsync("() => true", cancellationToken: TestContext.Current.CancellationToken));

        Assert.True(value.As<BooleanRemoteValue>().Value);
        Assert.Equal(2, calls[0]);
    }

    [Fact]
    public async Task WaitForFunctionReportsWhatItLastSawWhenTimeRunsOut()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => FakeResponse.Failure("unknown error", "gone", ("browsingContext.navigationStarted", new JsonObject() { ["context"] = page.Id, ["navigation"] = "navigation-2", ["timestamp"] = 1790000000000, ["url"] = "https://example.com/next" })));

        Task<RemoteValue> navigating = page.WaitForFunctionAsync("() => true", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException navigated = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, navigating));
        session.RemoteEnd.NeverAnswer("script.callFunction");
        Task<RemoteValue> hanging = page.WaitForFunctionAsync("() => true", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException running = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, hanging));

        Assert.EndsWith("; the frame navigated while it ran.", navigated.Message);
        Assert.EndsWith("; it was still running.", running.Message);
    }

    [Fact]
    public async Task WaitForFunctionLetsOtherScriptErrorsThrough()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Exception("Error: boom"));

        await Assert.ThrowsAsync<ScriptException>(() => page.WaitForFunctionAsync("() => { throw new Error('boom'); }", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(ReadinessState.Complete, "complete", 0)]
    [InlineData(ReadinessState.Interactive, "interactive", 1)]
    [InlineData(ReadinessState.None, "none", 1)]
    public async Task SetContentWritesTheHtmlAndKnowsTheStateOnlyWhenItWaitedForLoad(ReadinessState wait, string sent, int readyStateReads)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ((string)parameters["functionDeclaration"]!).Contains("document.readyState")
            ? ProtocolJson.Success(String("complete"))
            : ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }));

        await page.SetContentAsync("<p>Hi</p>", wait, cancellationToken: TestContext.Current.CancellationToken);
        await page.WaitForLoadStateAsync(cancellationToken: TestContext.Current.CancellationToken);

        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction").First(command => ((string)command["params"]!["functionDeclaration"]!).Contains("actions.setContent(html, state)"))["params"]!.AsObject();
        Assert.Equal(page.Group().Options.SandboxName, (string?)call["target"]!["sandbox"]);
        Assert.Equal("<p>Hi</p>", (string?)call["arguments"]![0]!["value"]);
        Assert.Equal(sent, (string?)call["arguments"]![1]!["value"]);
        Assert.Equal(readyStateReads, session.RemoteEnd.CommandsFor("script.callFunction").Count(command => ((string)command["params"]!["functionDeclaration"]!).Contains("document.readyState")));
    }

    [Fact]
    public async Task InitScriptsArePreloadedForThePageAndCanBeRemoved()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.addPreloadScript", new JsonObject() { ["script"] = "init-1" });

        InitScript script = await page.AddInitScriptAsync("() => { window.seed = 42; }", TestContext.Current.CancellationToken);
        await script.RemoveAsync(TestContext.Current.CancellationToken);

        Assert.Equal("init-1", script.Id);
        JsonObject added = session.RemoteEnd.CommandsFor("script.addPreloadScript").Last()["params"]!.AsObject();
        Assert.Equal("() => { window.seed = 42; }", (string?)added["functionDeclaration"]);
        Assert.Equal([page.Id], added["contexts"]!.AsArray().Select(context => (string?)context));
        Assert.False(added.ContainsKey("sandbox"));
        Assert.Equal("init-1", (string?)session.RemoteEnd.CommandsFor("script.removePreloadScript").Last()["params"]!["script"]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static void AnswerInTurn(FakeSession session, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone());
    }

    private static JsonObject String(string value) => new() { ["type"] = "string", ["value"] = value };

    private static JsonObject Number(double value) => new() { ["type"] = "number", ["value"] = value };

    private static JsonObject Boolean(bool value) => new() { ["type"] = "boolean", ["value"] = value };

    private static JsonObject Null() => new() { ["type"] = "null" };

    private static JsonObject Object() => new() { ["type"] = "object", ["value"] = new JsonArray() };

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
