// <copyright file="LocatorValueAssertionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using static Dramaturge.Assertions;

public class LocatorValueAssertionsTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task TextIsComparedWithWhiteSpaceNormalized()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("p-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(String("  Hello \n  world "))));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("p")));
        CancellationToken token = TestContext.Current.CancellationToken;

        await expectations.ToHaveTextAsync(" Hello\tworld", cancellationToken: token);
        await expectations.ToHaveTextAsync("hello WORLD", ignoreCase: true, cancellationToken: token);
        await expectations.ToHaveTextAsync(new Regex("^Hello world$"), cancellationToken: token);
        await expectations.ToContainTextAsync("lo  wo", cancellationToken: token);
        await expectations.ToContainTextAsync("LO WO", ignoreCase: true, cancellationToken: token);
        await expectations.ToContainTextAsync(new Regex("wor"), cancellationToken: token);
        await expectations.Not.ToContainTextAsync("LO WO", cancellationToken: token);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveTextAsync("Hello", timeout: Timeout, cancellationToken: token)));

        Assert.Equal("Expected css \"p\" to have text \"Hello\"; received \"Hello world\" after 1 seconds.", exception.Message);
        Assert.All(session.RemoteEnd.CommandsFor("script.callFunction"), call =>
        {
            JsonObject parameters = call["params"]!.AsObject();
            Assert.Contains("state.readTexts(elements, useInnerText)", (string?)parameters["functionDeclaration"]);
            Assert.False((bool?)parameters["arguments"]![0]!["value"]);
            Assert.Equal("p-1", (string?)parameters["arguments"]![1]!["sharedId"]);
        });
    }

    [Fact]
    public async Task ExpectedTextIsDescribedWithHowItIsMatched()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("p-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(String("Hello"))));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("p")));
        CancellationToken token = TestContext.Current.CancellationToken;

        ExpectationFailedException ignoringCase = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToContainTextAsync("bye", ignoreCase: true, timeout: Timeout, cancellationToken: token)));
        ExpectationFailedException pattern = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveTextAsync(new Regex("bye", RegexOptions.IgnoreCase), timeout: Timeout, cancellationToken: token)));
        ExpectationFailedException negated = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.Not.ToContainTextAsync(new Regex("ell"), timeout: Timeout, cancellationToken: token)));

        Assert.Equal("to contain text \"bye\" (ignoring case)", ignoringCase.Expected);
        Assert.Equal("to have text matching /bye/i", pattern.Expected);
        Assert.Equal("not to contain text matching /ell/", negated.Expected);
    }

    [Fact]
    public async Task RenderedTextIsReadWhenAsked()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("p-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => (bool)parameters["arguments"]![0]!["value"]! ? ProtocolJson.Success(Array(String("Rendered"))) : ProtocolJson.Success(Array(Null())));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("p")));

        await expectations.ToHaveTextAsync("Rendered", useInnerText: true, cancellationToken: TestContext.Current.CancellationToken);
        await expectations.ToContainTextAsync(new Regex("Rend"), useInnerText: true, cancellationToken: TestContext.Current.CancellationToken);
        await expectations.ToHaveTextAsync([new Regex("^Rendered$")], useInnerText: true, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RenderedTextOfAnElementThatIsNotHtmlIsRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("text-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(Null())));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("text")));

        InvalidOperationException single = await Assert.ThrowsAsync<InvalidOperationException>(() => expectations.ToHaveTextAsync("SVG", useInnerText: true, cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException list = await Assert.ThrowsAsync<InvalidOperationException>(() => expectations.Not.ToContainTextAsync(["SVG"], useInnerText: true, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"text\" matches an element that is not an HTML element.", single.Message);
        Assert.Equal(single.Message, list.Message);
    }

    [Fact]
    public async Task ListOfTextsIsComparedWithEveryMatchInOrder()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("li-1", "li-2", "li-3"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(String("One"), String(" Two "), String("Three"))));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("li")));
        CancellationToken token = TestContext.Current.CancellationToken;

        await expectations.ToHaveTextAsync(["One", "Two", "Three"], cancellationToken: token);
        await expectations.ToHaveTextAsync(["one", "TWO", "three"], ignoreCase: true, cancellationToken: token);
        await expectations.ToHaveTextAsync([new Regex("O"), new Regex("T"), new Regex("e$")], cancellationToken: token);
        await expectations.Not.ToHaveTextAsync(["One", "Three", "Two"], cancellationToken: token);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveTextAsync(["One", "Two"], timeout: Timeout, cancellationToken: token)));

        Assert.Equal("Expected css \"li\" to have texts [\"One\", \"Two\"]; received [\"One\", \"Two\", \"Three\"] after 1 seconds.", exception.Message);
        JsonArray arguments = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["arguments"]!.AsArray();
        Assert.Equal(["li-1", "li-2", "li-3"], arguments.Skip(1).Select(argument => (string?)argument!["sharedId"]));
        Assert.All(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"), lookup => Assert.False(lookup["params"]!.AsObject().ContainsKey("maxNodeCount")));
    }

    [Fact]
    public async Task ListOfTextsToContainIsFoundInOrderWithOtherElementsBetween()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("li-1", "li-2", "li-3"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(String("First item"), String("Second item"), String("Third item"))));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("li")));
        CancellationToken token = TestContext.Current.CancellationToken;

        await expectations.ToContainTextAsync(["First", "Third"], cancellationToken: token);
        await expectations.ToContainTextAsync(["SECOND"], ignoreCase: true, cancellationToken: token);
        await expectations.ToContainTextAsync([new Regex("^Sec"), new Regex("ird")], cancellationToken: token);
        await expectations.ToContainTextAsync(System.Array.Empty<string>(), cancellationToken: token);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToContainTextAsync(["Third", "First"], timeout: Timeout, cancellationToken: token)));

        Assert.Equal("to contain texts [\"Third\", \"First\"]", exception.Expected);
    }

    [Fact]
    public async Task ListOfTextsWithNoMatchingElementIsEmpty()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("li")));

        await expectations.ToHaveTextAsync(System.Array.Empty<string>(), cancellationToken: TestContext.Current.CancellationToken);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToContainTextAsync(["One"], timeout: Timeout, cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal("[]", exception.Actual);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task ValueIsComparedAsItIs()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String(" Ada ")));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("input")));

        await expectations.ToHaveValueAsync(" Ada ", cancellationToken: TestContext.Current.CancellationToken);
        await expectations.ToHaveValueAsync(new Regex("Ad"), cancellationToken: TestContext.Current.CancellationToken);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveValueAsync("Ada", Timeout, TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"input\" to have value \"Ada\"; received \" Ada \" after 1 seconds.", exception.Message);
        Assert.Contains("state.readValue(element)", (string?)session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!["functionDeclaration"]);
    }

    [Fact]
    public async Task ValueOfAnElementWithoutOneIsRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Null()));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Expect(page.Locate(new CssLocator("div"))).Not.ToHaveValueAsync("Ada", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"div\" is not an input, a text area, or a select.", exception.Message);
    }

    [Fact]
    public async Task AttributeIsExpectedWithOrWithoutAValue()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => (string?)parameters["arguments"]![1]!["value"] switch
        {
            "data-role" => ProtocolJson.Success(String("person")),
            "id" => ProtocolJson.Success(String("main")),
            "class" => ProtocolJson.Success(String("primary wide")),
            _ => ProtocolJson.Success(Null()),
        });
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("input")));
        CancellationToken token = TestContext.Current.CancellationToken;

        await expectations.ToHaveAttributeAsync("data-role", cancellationToken: token);
        await expectations.Not.ToHaveAttributeAsync("title", cancellationToken: token);
        await expectations.ToHaveAttributeAsync("data-role", "person", cancellationToken: token);
        await expectations.ToHaveAttributeAsync("data-role", "PERSON", ignoreCase: true, cancellationToken: token);
        await expectations.ToHaveAttributeAsync("data-role", new Regex("^pers"), cancellationToken: token);
        await expectations.Not.ToHaveAttributeAsync("title", "person", cancellationToken: token);
        await expectations.ToHaveIdAsync("main", cancellationToken: token);
        await expectations.ToHaveIdAsync(new Regex("ai"), cancellationToken: token);
        await expectations.ToHaveClassAsync("primary wide", cancellationToken: token);
        await expectations.ToHaveClassAsync(new Regex(@"\bwide\b"), cancellationToken: token);
        ExpectationFailedException value = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveAttributeAsync("data-role", "admin", timeout: Timeout, cancellationToken: token)));
        ExpectationFailedException missing = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveAttributeAsync("title", Timeout, token)));
        ExpectationFailedException classes = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveClassAsync("primary", Timeout, token)));

        Assert.Equal("Expected css \"input\" to have attribute \"data-role\" with value \"admin\"; received data-role=\"person\" after 1 seconds.", value.Message);
        Assert.Equal("Expected css \"input\" to have attribute \"title\"; received no \"title\" attribute after 1 seconds.", missing.Message);
        Assert.Equal("to have class \"primary\"", classes.Expected);
        Assert.Equal("class=\"primary wide\"", classes.Actual);
    }

    [Fact]
    public async Task CssIsComparedWithTheComputedValue()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String("rgb(0, 0, 0)")));
        LocatorAssertions expectations = Expect(page.Locate(new CssLocator("div")));

        await expectations.ToHaveCssAsync("color", "rgb(0, 0, 0)", cancellationToken: TestContext.Current.CancellationToken);
        await expectations.ToHaveCssAsync("color", new Regex(@"^rgb\("), cancellationToken: TestContext.Current.CancellationToken);
        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, expectations.ToHaveCssAsync("color", "black", Timeout, TestContext.Current.CancellationToken)));

        Assert.Equal("Expected css \"div\" to have CSS \"color\" with value \"black\"; received \"rgb(0, 0, 0)\" after 1 seconds.", exception.Message);
        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Contains("state.readCss(element, property)", (string?)call["functionDeclaration"]);
        Assert.Equal("color", (string?)call["arguments"]![1]!["value"]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static JsonObject Array(params JsonObject[] items)
    {
        return new JsonObject() { ["type"] = "array", ["value"] = new JsonArray([.. items]) };
    }

    private static JsonObject Null()
    {
        return new JsonObject() { ["type"] = "null" };
    }

    private static JsonObject String(string value)
    {
        return new JsonObject() { ["type"] = "string", ["value"] = value };
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
}
