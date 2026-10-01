// <copyright file="FormControlTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;

public class FormControlTests
{
    private const string CheckedQuery = "queryElementState(element, 'checked')";
    private const string StatesQuery = "queryElementStates(element, ['visible', 'enabled'])";
    private const string Readiness = "isInteractionReady";
    private const string SelectOptions = "actions.selectOptions";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task CheckClicksAnUncheckedBoxOnceItIsReadyThenConfirmsIt()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        AnswerScripts(session, (CheckedQuery, [Checked("unchecked"), Checked("checked")]), (Readiness, [NotReady("hidden"), Ready()]));

        await DriveAsync(time, page.Locate(new CssLocator("input")).CheckAsync(new PointerActionOptions() { Modifiers = KeyModifiers.Shift }, TestContext.Current.CancellationToken));

        IReadOnlyList<JsonObject> reads = ScriptCalls(session, CheckedQuery);
        Assert.Equal(2, reads.Count);
        Assert.Equal("box-1", (string?)reads[0]["params"]!["arguments"]![0]!["sharedId"]);
        Assert.Equal("click", (string?)ScriptCalls(session, Readiness)[0]["params"]!["arguments"]![1]!["value"]);
        Assert.Equal(2, ScriptCalls(session, Readiness).Count);
        JsonArray sources = Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"))["params"]!["actions"]!.AsArray();
        JsonArray pointer = sources.Single(source => (string?)source!["type"] == "pointer")!["actions"]!.AsArray();
        Assert.Equal(["pointerMove", "pointerDown", "pointerUp"], pointer.Select(action => (string?)action!["type"]).Where(type => type != "pause"));
        Assert.Contains(sources, source => (string?)source!["type"] == "key");
    }

    [Theory]
    [InlineData(true, "checked")]
    [InlineData(false, "unchecked")]
    [InlineData(false, "indeterminate")]
    public async Task ElementAlreadyInTheStateIsNotClicked(bool isChecked, string state)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        AnswerScripts(session, (CheckedQuery, [Checked(state)]));

        await page.Locate(new CssLocator("input")).SetCheckedAsync(isChecked, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(ScriptCalls(session, CheckedQuery));
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task UncheckClicksACheckedBoxThenConfirmsIt()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        AnswerScripts(session, (CheckedQuery, [Checked("checked"), Checked("unchecked")]), (Readiness, [Ready()]));

        await page.Locate(new CssLocator("input")).UncheckAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task UncheckOfACheckedRadioButtonFailsWithoutAClick()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("radio-1"));
        AnswerScripts(session, (CheckedQuery, [Checked("checked", isRadio: true)]));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("input")).UncheckAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"input\" is a radio button, which clicking cannot uncheck.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Theory]
    [InlineData(true, "unchecked", "check")]
    [InlineData(false, "checked", "uncheck")]
    public async Task ClickThatDoesNotChangeTheStateFails(bool isChecked, string state, string verb)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("box-1"));
        AnswerScripts(session, (CheckedQuery, [Checked(state)]), (Readiness, [Ready()]));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("input")).SetCheckedAsync(isChecked, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal($"Clicking css \"input\" did not {verb} it.", exception.Message);
        Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task CheckOfAnElementThatCannotBeCheckedFailsAtOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, (CheckedQuery, [Received("error:notcheckable")]));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("button")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"button\" is not a checkbox or radio button.", exception.Message);
    }

    [Theory]
    [InlineData(0, "no element matched")]
    [InlineData(1, "the element was removed from the document")]
    public async Task CheckThatCannotReadTheStateInTimeSaysWhy(int matches, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        AnswerScripts(session, (CheckedQuery, [Received("error:notconnected")]));

        Task check = page.Locate(new CssLocator("input")).CheckAsync(new PointerActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, check));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"input\" to report whether it is checked; {observed}.", exception.Message);
    }

    [Fact]
    public async Task SelectOptionWaitsForTheElementAndSendsEachOptionByWhatMatchesIt()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("select-1"));
        AnswerScripts(
            session,
            (StatesQuery, [Status("failure", ("missingState", "disabled")), Status("success")]),
            (SelectOptions, [Status("missing", ("index", 1)), Status("disabled", ("index", 0)), Status("notconnected"), Selected("red", "blue", "green")]));

        IReadOnlyList<string> values = await DriveAsync(time, page.Locate(new CssLocator("select")).SelectOptionAsync([SelectOption.ByValue("red"), SelectOption.ByLabel("Blue"), SelectOption.ByIndex(2)], cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(["red", "blue", "green"], values);
        Assert.Equal(5, ScriptCalls(session, StatesQuery).Count);
        IReadOnlyList<JsonObject> selections = ScriptCalls(session, SelectOptions);
        Assert.Equal(4, selections.Count);
        JsonArray arguments = selections[0]["params"]!["arguments"]!.AsArray();
        Assert.Equal("select-1", (string?)arguments[0]!["sharedId"]);
        Assert.Equal(["value=red", "label=Blue", "index=2"], arguments[1]!["value"]!.AsArray().Select(option => $"{option!["value"]![0]![0]}={option["value"]![0]![1]!["value"]}"));
    }

    [Theory]
    [InlineData(0, "success", null, "no element matched")]
    [InlineData(1, "failure", null, "the element was not visible")]
    [InlineData(1, "error", null, "the element was removed from the document")]
    [InlineData(1, "success", "missing", "no option matched label \"Blue\"")]
    [InlineData(1, "success", "disabled", "the option matching label \"Blue\" was disabled")]
    public async Task SelectOptionThatTimesOutSaysWhy(int matches, string states, string? selection, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        JsonObject stateResult = states switch
        {
            "failure" => Status("failure", ("missingState", "hidden")),
            "error" => Status("error", ("message", "notconnected")),
            _ => Status("success"),
        };
        AnswerScripts(session, (StatesQuery, [stateResult]), (SelectOptions, [Status(selection ?? "selected", ("index", 0))]));

        Task<IReadOnlyList<string>> select = page.Locate(new CssLocator("select")).SelectOptionAsync([SelectOption.ByLabel("Blue")], new ActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, select));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"select\" to be ready to have options selected; {observed}.", exception.Message);
    }

    [Theory]
    [InlineData("notselect", "css \"select\" is not a <select> element.")]
    [InlineData("notmultiple", "css \"select\" is a <select> element that takes one option, but 2 were given.")]
    public async Task SelectOptionMisuseFailsAtOnce(string status, string message)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("select-1"));
        AnswerScripts(session, (SelectOptions, [Status(status)]));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("select")).SelectOptionAsync([SelectOption.ByIndex(0), SelectOption.ByIndex(1)], new ActionOptions() { Force = true }, TestContext.Current.CancellationToken));

        Assert.Equal(message, exception.Message);
        Assert.Empty(ScriptCalls(session, StatesQuery));
    }

    [Fact]
    public async Task SelectingNoOptionsSendsAnEmptyList()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("select-1"));
        AnswerScripts(session, (StatesQuery, [Status("success")]), (SelectOptions, [Selected()]));

        IReadOnlyList<string> values = await page.Locate(new CssLocator("select")).SelectOptionAsync([], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(values);
        Assert.Empty(Assert.Single(ScriptCalls(session, SelectOptions))["params"]!["arguments"]![1]!["value"]!.AsArray());
    }

    [Fact]
    public void SelectOptionsDescribeWhatMatchesThem()
    {
        Assert.Equal("value \"red\"", SelectOption.ByValue("red").ToString());
        Assert.Equal("label \"Red\"", SelectOption.ByLabel("Red").ToString());
        Assert.Equal("index 3", SelectOption.ByIndex(3).ToString());
    }

    [Fact]
    public async Task SetInputFilesWaitsForTheElementThenSetsTheFiles()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] lookups = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => Interlocked.Increment(ref lookups[0]) == 1 ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("file-1"));

        await DriveAsync(time, page.Locate(new CssLocator("input")).SetInputFilesAsync(["/tmp/a.txt", "/tmp/b.txt"], cancellationToken: TestContext.Current.CancellationToken));

        JsonObject parameters = Assert.Single(session.RemoteEnd.CommandsFor("input.setFiles"))["params"]!.AsObject();
        Assert.Equal(page.Id, (string?)parameters["context"]);
        Assert.Equal("file-1", (string?)parameters["element"]!["sharedId"]);
        Assert.Equal(["/tmp/a.txt", "/tmp/b.txt"], parameters["files"]!.AsArray().Select(file => (string?)file));
    }

    [Fact]
    public async Task SetInputFilesThatFindsNoElementTimesOut()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task set = page.Locate(new CssLocator("input")).SetInputFilesAsync([], TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, set));

        Assert.Equal("Timed out after 1 seconds waiting for css \"input\" to be attached; no element matched.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.setFiles"));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Answers each script whose function contains a fragment with that fragment's next result in turn (repeating
    // the last), and every other script, such as a scroll, with undefined.
    private static void AnswerScripts(FakeSession session, params (string Fragment, JsonObject[] Results)[] answers)
    {
        int[] calls = new int[answers.Length];
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            string function = (string)parameters["functionDeclaration"]!;
            for (int i = 0; i < answers.Length; i++)
            {
                if (function.Contains(answers[i].Fragment))
                {
                    JsonObject[] results = answers[i].Results;
                    return ProtocolJson.Success((JsonObject)results[Math.Min(Interlocked.Increment(ref calls[i]), results.Length) - 1].DeepClone());
                }
            }

            return ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" });
        });
    }

    private static JsonObject Object(params (string Name, JsonNode Value)[] properties)
    {
        return new JsonObject() { ["type"] = "object", ["value"] = new JsonArray([.. properties.Select(property => (JsonNode)new JsonArray(property.Name, property.Value))]) };
    }

    private static JsonObject String(string value)
    {
        return new JsonObject() { ["type"] = "string", ["value"] = value };
    }

    private static JsonObject Checked(string received, bool isRadio = false)
    {
        return Object(("matches", new JsonObject() { ["type"] = "boolean", ["value"] = received == "checked" }), ("received", String(received)), ("isRadio", new JsonObject() { ["type"] = "boolean", ["value"] = isRadio }));
    }

    private static JsonObject Received(string received)
    {
        return Object(("matches", new JsonObject() { ["type"] = "boolean", ["value"] = false }), ("received", String(received)));
    }

    private static JsonObject Status(string status, params (string Name, object Value)[] extra)
    {
        return Object([("status", String(status)), .. extra.Select(entry => (entry.Name, (JsonNode)(entry.Value is int number ? new JsonObject() { ["type"] = "number", ["value"] = number } : String((string)entry.Value))))]);
    }

    private static JsonObject Selected(params string[] values)
    {
        return Object(("status", String("selected")), ("values", new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. values.Select(value => (JsonNode)String(value))]) }));
    }

    private static JsonObject Ready()
    {
        JsonObject offset = Object(("x", new JsonObject() { ["type"] = "number", ["value"] = 0 }), ("y", new JsonObject() { ["type"] = "number", ["value"] = 0 }));
        return Object(("status", String("ready")), ("interactionOffset", offset));
    }

    private static JsonObject NotReady(string reason)
    {
        return Object(("status", String("notready")), ("reason", String(reason)));
    }

    private static IReadOnlyList<JsonObject> ScriptCalls(FakeSession session, string functionFragment)
    {
        return [.. session.RemoteEnd.CommandsFor("script.callFunction").Where(command => ((string)command["params"]!["functionDeclaration"]!).Contains(functionFragment))];
    }

    private static async Task DriveAsync(FakeTimeProvider time, Task operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        await operation;
    }

    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        await DriveAsync(time, (Task)operation);
        return await operation;
    }
}
