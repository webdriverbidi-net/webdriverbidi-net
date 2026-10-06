// <copyright file="CodegenTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using Dramaturge.Tool.TestUtilities;

// Each test records in a fake browser, connected to in place of the one the command configures, sending what the
// recorder in its page would.
public sealed class CodegenTests : IDisposable
{
    private const string SessionName = "codegen";
    private readonly TemporaryDirectory directory = new();

    [Fact]
    public async Task RecordingEndsWhenTheLastPageClosesAndWritesTheWholeFile()
    {
        await using FakeBrowserServer server = await FakeBrowserServer.StartAsync();

        Task<ToolResult> running = RunAsync(server, CancellationToken.None, "codegen", "https://example.com/");
        FakeSession session = await ConnectedSessionAsync(server);
        string channel = await ChannelAsync(session);
        string page = (string)(await session.RemoteEnd.WaitForCommandAsync("browsingContext.navigate"))["params"]!["context"]!;
        await session.RaiseNavigationEventAsync("browsingContext.navigationStarted", page, "https://example.com/");
        await SendClickAsync(session, channel, page, "#go");
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextDestroyed", Context(page));
        ToolResult result = await running;

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(
            [
                "await page.NavigateAsync(\"https://example.com/\");",
                "await page.Locate(new CssLocator(\"#go\")).ClickAsync();",
                "using Dramaturge;",
                "using Dramaturge.Browsers;",
                "using WebDriverBiDi.BrowsingContext;",
                "await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.ConfigureFromEnvironment().WithHeadlessOption(false));",
                "Page page = await group.DefaultBrowser.NewPageAsync();",
                "await page.NavigateAsync(\"https://example.com/\");",
                "await page.Locate(new CssLocator(\"#go\")).ClickAsync();",
            ],
            result.OutputLines);
        Assert.Contains("Recording. Close the browser, or press Ctrl+C, to stop.", result.Error);
    }

    [Fact]
    public async Task OutputFileIsKeptCurrentUntilTheRecordingIsCancelled()
    {
        await using FakeBrowserServer server = await FakeBrowserServer.StartAsync();
        server.OnConnect = session => session.AddContext();
        string path = Path.Combine(this.directory.Path, "RecordedTests.cs");
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        Task<ToolResult> running = RunAsync(server, cancellation.Token, "codegen", "--target", "xunit", "--test-id-attribute", "data-test", "--browser", "firefox", "--channel", "beta", "-o", path);
        FakeSession session = await ConnectedSessionAsync(server);
        string channel = await ChannelAsync(session);
        string page = session.Contexts[0].Id;
        await SendClickAsync(session, channel, page, "#first");
        await SendClickAsync(session, channel, page, "#second");
        await WaitUntilAsync(() => File.Exists(path) && File.ReadAllText(path).Contains("#first"));
        string current = File.ReadAllText(path);
        cancellation.Cancel();
        ToolResult result = await running;

        Assert.Equal(0, result.ExitCode);
        Assert.DoesNotContain("#second", current);
        Assert.Equal(["await page.Locate(new CssLocator(\"#first\")).ClickAsync();", "await page.Locate(new CssLocator(\"#second\")).ClickAsync();"], result.OutputLines);
        string code = File.ReadAllText(path);
        Assert.Contains("public class RecordedTests : PageTest", code);
        Assert.Contains("protected override DramaturgeOptions? GroupOptions => new() { TestIdAttribute = \"data-test\" };", code);
        Assert.Contains("        await page.Locate(new CssLocator(\"#second\")).ClickAsync();\n", code);
        Assert.Contains($"Wrote {path}", result.Error);
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.create"));
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.navigate"));
    }

    [Fact]
    public async Task PickedLocatorIsReported()
    {
        await using FakeBrowserServer server = await FakeBrowserServer.StartAsync();
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        string path = Path.Combine(this.directory.Path, "Recorded.cs");

        // Messages are handled in order, so the pick is reported once the click after it settles.
        Task<ToolResult> running = RunAsync(server, cancellation.Token, "codegen", "-o", path);
        FakeSession session = await ConnectedSessionAsync(server);
        string channel = await ChannelAsync(session);
        await SendAsync(session, channel, session.Contexts[0].Id, Action("pick", "#go"));
        await SendClickAsync(session, channel, session.Contexts[0].Id, "#first");
        await SendClickAsync(session, channel, session.Contexts[0].Id, "#second");
        await WaitUntilAsync(() => File.Exists(path) && File.ReadAllText(path).Contains("#first"));
        cancellation.Cancel();
        ToolResult result = await running;

        Assert.Contains("Picked: page.Locate(new CssLocator(\"#go\"))", result.Error);
    }

    [Fact]
    public async Task InvalidTestIdAttributeStopsTheCommandBeforeTheBrowserStarts()
    {
        await using FakeBrowserServer server = await FakeBrowserServer.StartAsync();

        ToolResult result = await RunAsync(server, CancellationToken.None, "codegen", "--test-id-attribute", "1st");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("'1st' is not an attribute name.", result.Error);
        Assert.Throws<KeyNotFoundException>(() => server.SessionFor(SessionName));
    }

    [Fact]
    public async Task BrowserThatCannotStartEndsTheCommand()
    {
        StringWriter output = new();
        StringWriter error = new();

        int exitCode = await DramaturgeTool.RunAsync(["codegen"], output, error, configureLauncher: _ => BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(new Uri("ws://127.0.0.1:9/session")), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.StartsWith("The browser could not be started: ", error.ToString());
    }

    [Fact]
    public async Task OutputFileThatCannotBeWrittenEndsTheCommand()
    {
        await using FakeBrowserServer server = await FakeBrowserServer.StartAsync();
        string path = Path.Combine(this.directory.Path, "missing", "Recorded.cs");

        ToolResult result = await RunAsync(server, CancellationToken.None, "codegen", "-o", path);

        Assert.Equal(1, result.ExitCode);
        Assert.StartsWith($"{path}: ", result.Error);
    }

    [Fact]
    public async Task HelpDescribesTheCommand()
    {
        StringWriter output = new();

        int exitCode = await DramaturgeTool.RunAsync(["codegen", "--help"], output, new StringWriter(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("Record the actions you take in a browser as C#", output.ToString());
        Assert.Contains("--target", output.ToString());
        Assert.Contains("--test-id-attribute", output.ToString());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.directory.Dispose();
    }

    private static async Task<ToolResult> RunAsync(FakeBrowserServer server, CancellationToken cancellationToken, params string[] args)
    {
        StringWriter output = new();
        StringWriter error = new();
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, TestContext.Current.CancellationToken);
        int exitCode = await DramaturgeTool.RunAsync(args, output, error, configureLauncher: _ => BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(server.UrlFor(SessionName)), cancellationToken: linked.Token);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }

    private static async Task<FakeSession> ConnectedSessionAsync(FakeBrowserServer server)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (true)
        {
            try
            {
                return server.SessionFor(SessionName);
            }
            catch (KeyNotFoundException) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);
            }
        }
    }

    private static async Task<string> ChannelAsync(FakeSession session)
    {
        JsonObject install = await session.RemoteEnd.WaitForCommandAsync("script.callFunction");
        return (string)install["params"]!["arguments"]![0]!["value"]!["channel"]!;
    }

    private static JsonObject Action(string kind, string cssPath)
    {
        return new JsonObject()
        {
            ["kind"] = kind,
            ["target"] = new JsonObject() { ["cssPath"] = cssPath, ["labels"] = new JsonArray() },
            ["ancestors"] = new JsonArray(),
            ["button"] = "left",
            ["clickCount"] = 1,
            ["modifiers"] = new JsonArray(),
        };
    }

    private static Task SendClickAsync(FakeSession session, string channel, string contextId, string cssPath) => SendAsync(session, channel, contextId, Action("click", cssPath));

    private static Task SendAsync(FakeSession session, string channel, string contextId, JsonObject message)
    {
        return session.RemoteEnd.RaiseEventAsync("script.message", new JsonObject()
        {
            ["channel"] = channel,
            ["data"] = new JsonObject()
            {
                ["type"] = "array",
                ["value"] = new JsonArray(
                    new JsonObject() { ["type"] = "string", ["value"] = message.ToJsonString() },
                    new JsonObject() { ["type"] = "node", ["sharedId"] = $"node-{message["target"]!["cssPath"]}", ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } }),
            },
            ["source"] = new JsonObject() { ["realm"] = "realm-1", ["context"] = contextId },
        });
    }

    private static JsonObject Context(string contextId)
    {
        return new JsonObject() { ["context"] = contextId, ["clientWindow"] = $"window-for-{contextId}", ["url"] = "https://example.com/", ["userContext"] = "default", ["children"] = null, ["originalOpener"] = null };
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        Assert.True(condition());
    }
}
