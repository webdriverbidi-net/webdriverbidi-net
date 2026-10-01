// <copyright file="PageAssertionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using static Dramaturge.Assertions;

public class PageAssertionsTests
{
    private const string PageUrl = "https://example.com/start";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task UrlExpectationIsMetAtOnceOrWhenTheUrlChanges()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        PageAssertions expectations = Expect(page);
        CancellationToken token = TestContext.Current.CancellationToken;

        await expectations.ToHaveUrlAsync(PageUrl, cancellationToken: token);
        await expectations.ToHaveUrlAsync("HTTPS://EXAMPLE.COM/START", ignoreCase: true, cancellationToken: token);
        Task fragment = expectations.ToHaveUrlAsync(new Regex("#done$"), cancellationToken: token);
        Task away = expectations.Not.ToHaveUrlAsync(PageUrl, cancellationToken: token);
        Assert.False(fragment.IsCompleted);
        Assert.False(away.IsCompleted);
        await session.RaiseNavigationEventAsync("browsingContext.fragmentNavigated", page.Id, PageUrl + "#done");
        await fragment;
        await away;

        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task UrlThatNeverMatchesReportsTheUrlSeen()
    {
        (BiDiDriver driver, FakeSession _, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page).ToHaveUrlAsync("https://example.com/other", cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal($"Expected the page to have URL \"https://example.com/other\"; received \"{PageUrl}\" after 5 seconds.", exception.Message);
        Assert.Equal("to have URL \"https://example.com/other\"", exception.Expected);
        Assert.Equal($"\"{PageUrl}\"", exception.Actual);
        Assert.Equal(TimeSpan.FromSeconds(5), exception.Timeout);
    }

    [Fact]
    public async Task UrlExpectationEndsWhenThePageIsClosed()
    {
        (BiDiDriver driver, FakeSession _, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task expectation = Expect(page).ToHaveUrlAsync("https://example.com/never", cancellationToken: TestContext.Current.CancellationToken);
        await page.CloseAsync(TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => expectation);

        Assert.Equal("The frame was detached while waiting for the page to have URL \"https://example.com/never\".", exception.Message);
    }

    [Fact]
    public async Task TitleIsReadInTheSandboxUntilItMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => ProtocolJson.Success(String(Interlocked.Increment(ref calls[0]) == 1 ? "Loading" : "  Home \n page ")));
        PageAssertions expectations = Expect(page);
        CancellationToken token = TestContext.Current.CancellationToken;

        await DriveAsync(time, expectations.ToHaveTitleAsync("Home  page", cancellationToken: token));
        await expectations.ToHaveTitleAsync("HOME PAGE", ignoreCase: true, cancellationToken: token);
        await expectations.ToHaveTitleAsync(new Regex("^Home page$"), cancellationToken: token);
        await expectations.Not.ToHaveTitleAsync("Loading", cancellationToken: token);

        Assert.All(session.RemoteEnd.CommandsFor("script.callFunction"), call =>
        {
            Assert.Equal("() => document.title", (string?)call["params"]!["functionDeclaration"]);
            Assert.Equal(page.Id, (string?)call["params"]!["target"]!["context"]);
            Assert.Equal(page.Group().Options.SandboxName, (string?)call["params"]!["target"]!["sandbox"]);
        });
    }

    [Fact]
    public async Task TitleThatNeverMatchesReportsTheTitleSeen()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(String("Home")));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page).Not.ToHaveTitleAsync(new Regex("ome"), Timeout, TestContext.Current.CancellationToken)));

        Assert.Equal("Expected the page not to have title matching /ome/; received \"Home\" after 1 seconds.", exception.Message);
        Assert.Equal("\"Home\"", exception.Actual);
    }

    [Fact]
    public async Task TitleReadFailingWhileTheFrameNavigatesIsRetried()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => Interlocked.Increment(ref calls[0]) == 1
            ? FakeResponse.Failure("unknown error", "Inspected target navigated or closed", ("browsingContext.navigationStarted", NavigationStarted(page.Id)))
            : new FakeResponse(ProtocolJson.Success(String("Home"))));

        await DriveAsync(time, Expect(page).ToHaveTitleAsync("Home", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, calls[0]);
    }

    [Fact]
    public async Task TitleThatIsNeverReadReportsWhy()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => FakeResponse.Failure("unknown error", "gone", ("browsingContext.navigationStarted", NavigationStarted(page.Id))));

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => DriveAsync(time, Expect(page).ToHaveTitleAsync("Home", timeout: Timeout, cancellationToken: TestContext.Current.CancellationToken)));

        Assert.Equal("Expected the page to have title \"Home\"; the frame navigated while the title was read after 1 seconds.", exception.Message);
        Assert.Null(exception.Actual);
    }

    // The group's clock never moves here, so only the driver's own command timer ends the call.
    [Fact]
    public async Task TitleReadTimingOutEndsTheExpectation()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.NeverAnswer("script.callFunction");

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => Expect(page).ToHaveTitleAsync("Home", timeout: TimeSpan.FromMilliseconds(200), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("Expected the page to have title \"Home\"; a command was still running after 0.2 seconds.", exception.Message);
    }

    [Fact]
    public async Task TitleReadLetsOtherErrorsThrough()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => FakeResponse.Failure("unknown error", "Something else"));

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => Expect(page).ToHaveTitleAsync("Home", cancellationToken: TestContext.Current.CancellationToken));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(PageUrl, cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static JsonObject NavigationStarted(string contextId)
    {
        return new JsonObject() { ["context"] = contextId, ["navigation"] = "navigation-2", ["timestamp"] = 1790000000000, ["url"] = "https://example.com/next" };
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
