// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class CaptureTests
{
    [Fact]
    public async Task ScreenshotCapturesTheViewportByDefault()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.captureScreenshot", new JsonObject() { ["data"] = Convert.ToBase64String([1, 2]) });

        byte[] image = await page.ScreenshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], image);
        JsonObject parameters = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.captureScreenshot"))["params"]!.AsObject();
        Assert.Equal(page.Id, (string?)parameters["context"]);
        Assert.Equal("viewport", (string?)parameters["origin"]);
        Assert.False(parameters.ContainsKey("clip"));
        Assert.False(parameters.ContainsKey("format"));
    }

    [Fact]
    public async Task ScreenshotOptionsChooseTheDocumentAClipAndTheFormat()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.captureScreenshot", new JsonObject() { ["data"] = Convert.ToBase64String([3]) });
        PageScreenshotOptions options = new() { FullPage = true, Clip = new BoxClipRectangle() { X = 1, Y = 2, Width = 3, Height = 4 }, Format = new ImageFormat() { Type = "image/jpeg", Quality = 0.8 } };

        await page.ScreenshotAsync(options, TestContext.Current.CancellationToken);

        JsonObject parameters = Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.captureScreenshot"))["params"]!.AsObject();
        Assert.Equal("document", (string?)parameters["origin"]);
        Assert.Equal("box", (string?)parameters["clip"]!["type"]);
        Assert.Equal([1.0, 2.0, 3.0, 4.0], new[] { "x", "y", "width", "height" }.Select(name => (double?)parameters["clip"]![name]));
        Assert.Equal("image/jpeg", (string?)parameters["format"]!["type"]);
        Assert.Equal(0.8, (double?)parameters["format"]!["quality"]);
    }

    [Fact]
    public async Task PdfPrintsWithTheBrowsersDefaultsOrTheGivenSetup()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.print", new JsonObject() { ["data"] = Convert.ToBase64String([0x25, 0x50]) });
        PdfOptions options = new() { Background = true, Margins = new PrintMarginParameters() { Top = 1 }, Orientation = PrintOrientation.Landscape, PageSize = new PrintPageParameters() { Width = 10 }, Scale = 0.5, ShrinkToFit = false };
        options.PageRanges.Add(1);
        options.PageRanges.Add("3-4");

        byte[] plain = await page.PdfAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.PdfAsync(options, TestContext.Current.CancellationToken);

        Assert.Equal([0x25, 0x50], plain);
        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browsingContext.print");
        JsonObject defaults = commands[0]["params"]!.AsObject();
        Assert.Equal(page.Id, (string?)defaults["context"]);
        Assert.False(defaults.ContainsKey("orientation"));
        JsonObject configured = commands[1]["params"]!.AsObject();
        Assert.True((bool?)configured["background"]);
        Assert.Equal(1, (double?)configured["margin"]!["top"]);
        Assert.Equal("landscape", (string?)configured["orientation"]);
        Assert.Equal(10, (double?)configured["page"]!["width"]);
        Assert.Equal(0.5, (double?)configured["scale"]);
        Assert.False((bool?)configured["shrinkToFit"]);
        Assert.Equal(["1", "3-4"], configured["pageRanges"]!.AsArray().Select(range => range!.ToString()));
    }

    [Fact]
    public async Task ViewportSizeIsSetAndResetForThePage()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.SetViewportSizeAsync(640, 480, TestContext.Current.CancellationToken);
        await page.ResetViewportSizeAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browsingContext.setViewport");
        Assert.Equal(2, commands.Count);
        Assert.All(commands, command => Assert.Equal(page.Id, (string?)command["params"]!["context"]));
        Assert.Equal(640UL, (ulong?)commands[0]["params"]!["viewport"]!["width"]);
        Assert.Equal(480UL, (ulong?)commands[0]["params"]!["viewport"]!["height"]);
        Assert.True(commands[1]["params"]!.AsObject().ContainsKey("viewport"));
        Assert.Null(commands[1]["params"]!["viewport"]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }
}
