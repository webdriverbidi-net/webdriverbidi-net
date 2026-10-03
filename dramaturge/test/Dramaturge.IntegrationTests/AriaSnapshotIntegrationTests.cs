// <copyright file="AriaSnapshotIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using static Dramaturge.Assertions;

public class AriaSnapshotIntegrationTests
{
    private const string PageSnapshot = """
        - navigation "Main" [ref=e1]:
          - link "Home" [ref=e2]:
            - /url: index.html
        - main [ref=e3]:
          - heading "Sign in" [level=1] [ref=e4]
          - text: Email
          - textbox "Email" [ref=e5]:
            - /placeholder: you@example.com
          - button "Sign in" [ref=e6]
          - iframe [ref=e7]:
            - paragraph [ref=f1e1]: Cookies?
            - button "Accept" [ref=f1e2]
        """;

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PageSnapshotIncludesFramesWithTheirOwnRefs(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(PageSnapshot, snapshot.ToString());
        AriaNode frame = Find(snapshot.Root, "iframe", string.Empty);
        Assert.Equal(["paragraph", "button"], frame.Children.Select(child => child.Role));
        Assert.Equal("Cookies?", frame.Children[0].Children[0].Text);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RefLocatorsActOnTheirElementsInEveryFrame(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        await snapshot.Locator(Find(snapshot.Root, "button", "Sign in").Ref!).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator accept = snapshot.Locator(Find(snapshot.Root, "button", "Accept").Ref!);
        await accept.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        await Expect(page.Locate(new CssLocator("#go"))).ToHaveTextAsync("Signed in", cancellationToken: TestContext.Current.CancellationToken);
        await Expect(accept).ToHaveTextAsync("Accepted", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotSame(page.MainFrame, accept.Frame);
        Assert.Equal("ref f1e2", accept.ToString());
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RefsLastAsLongAsTheirElements(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");
        AriaSnapshot first = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        AriaSnapshot second = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.EvaluateAsync("() => document.getElementById('go').remove()", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(first.ToString(), second.ToString());
        Assert.Equal(0, await first.Locator("e6").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await first.Locator("e2").CountAsync(TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentException>(() => first.Locator("e99"));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RefIntoAReplacedDocumentFails(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox resolves a shared reference to an element of a document its frame has navigated away from, where the protocol requires a no such node error.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => snapshot.Locator("e2").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal("The element of ref e2 is no longer in its document, which has been replaced or discarded.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LocatorSnapshotCoversItsElementWithTheChosenOptions(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");
        ElementLocator main = page.Locate(new CssLocator("main"));

        AriaSnapshot withoutFrames = await main.AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeRefs = false, IncludeFrames = false }, cancellationToken: TestContext.Current.CancellationToken);
        AriaSnapshot withoutRefs = await main.AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeRefs = false }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            """
            - main:
              - heading "Sign in" [level=1]
              - text: Email
              - textbox "Email":
                - /placeholder: you@example.com
              - button "Sign in"
              - iframe
            """,
            withoutFrames.ToString());
        Assert.EndsWith(
            """
              - iframe:
                - paragraph: Cookies?
                - button "Accept"
            """,
            withoutRefs.ToString());
        Assert.Throws<ArgumentException>(() => withoutRefs.Locator("e3"));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CrossOriginFrameIsIncluded(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "cross-origin-frame.html");
        Frame frame = await page.Locate(new CssLocator("#other-origin")).ContentFrameAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await frame.Locate(new CssLocator("p")).WaitForAsync(cancellationToken: TestContext.Current.CancellationToken);

        AriaSnapshot snapshot = await page.AriaSnapshotAsync(new AriaSnapshotOptions() { IncludeRefs = false }, TestContext.Current.CancellationToken);

        Assert.Equal(
            """
            - paragraph: Host page
            - iframe:
              - paragraph: Frame content
            """,
            snapshot.ToString());
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SnapshotExpectationWaitsUntilTheSnapshotMatches(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");
        ElementLocator main = page.Locate(new CssLocator("main"));

        Task expectation = Expect(main).ToMatchAriaSnapshotAsync(
            """
            - main:
              - heading "Sign in" [level=1]
              - button "Signed in"
            """,
            cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#go")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        await expectation;
        await Expect(main).Not.ToMatchAriaSnapshotAsync("- button \"Sign in\"", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task UnmetSnapshotExpectationShowsTheDifference(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => Expect(page.Locate(new CssLocator("nav"))).ToMatchAriaSnapshotAsync(
            """
                - navigation "Main":
                  - link "About"
            """,
            TimeSpan.FromSeconds(1),
            TestContext.Current.CancellationToken));

        Assert.Equal(
            """
            Expected css "nav" to match aria snapshot; the snapshot did not match after 1 seconds.
            - expected
            + received

              - navigation "Main":
            -   - link "About"
            +   - link "Home":
            +     - /url: index.html
            """,
            exception.Message);
        Assert.Equal("to match aria snapshot", exception.Expected);
        Assert.Equal("- navigation \"Main\":\n  - link \"Home\":\n    - /url: index.html", exception.Actual);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task InvalidSnapshotTemplateFailsAtOnce(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "aria-snapshot.html");

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => Expect(page.Locate(new CssLocator("#missing"))).ToMatchAriaSnapshotAsync("- buton", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("template", exception.ParamName);
        Assert.StartsWith("Invalid aria snapshot template, line 1: Unknown role \"buton\"", exception.Message);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server, string path)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor(path), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static AriaNode Find(AriaNode node, string role, string name)
    {
        return FindOrNull(node, role, name) ?? throw new InvalidOperationException($"No {role} named \"{name}\" in the snapshot.");
    }

    private static AriaNode? FindOrNull(AriaNode node, string role, string name)
    {
        return node.Role == role && node.Name == name ? node : node.Children.Select(child => FindOrNull(child, role, name)).FirstOrDefault(found => found is not null);
    }
}
