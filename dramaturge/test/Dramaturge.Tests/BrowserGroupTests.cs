// <copyright file="BrowserGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.Session;

public class BrowserGroupTests
{
    [Fact]
    public async Task LaunchedGroupStartsSessionAndEndsItWhenDisposed()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        AutomationOptions options = new();

        BrowserGroup group = await BrowserGroup.LaunchAsync(Launcher(server), options, TestContext.Current.CancellationToken);
        bool wasStarted = group.Driver.IsStarted;
        await group.DisposeAsync();

        Assert.True(wasStarted);
        Assert.Same(options, group.Options);
        Assert.Equal(["session.new", "session.subscribe", "script.addPreloadScript", "browser.getUserContexts", "browsingContext.getTree", "session.end"], server.ReceivedMethods);
        Assert.False(group.Driver.IsStarted);
    }

    [Fact]
    public async Task LaunchedGroupRequestsTheBuildersSessionCapabilities()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        BrowserLauncherBuilder builder = Launcher(server)
            .WithSessionCapability("unhandledPromptBehavior", new UserPromptHandler() { Default = UserPromptHandlerType.Ignore })
            .WithSessionCapability("acceptInsecureCerts", true)
            .WithSessionCapability("vendor:options", new Dictionary<string, object?>() { ["flag"] = true });

        await using BrowserGroup group = await BrowserGroup.LaunchAsync(builder, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject alwaysMatch = server.ReceivedCommands.First(command => (string?)command["method"] == "session.new")["params"]!["capabilities"]!["alwaysMatch"]!.AsObject();
        Assert.Equal("ignore", (string?)alwaysMatch["unhandledPromptBehavior"]!["default"]);
        Assert.True((bool?)alwaysMatch["acceptInsecureCerts"]);
        Assert.True((bool?)alwaysMatch["vendor:options"]!["flag"]);
    }

    [Fact]
    public async Task LaunchedGroupIsDisposedOnce()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        BrowserGroup group = await BrowserGroup.LaunchAsync(Launcher(server), cancellationToken: TestContext.Current.CancellationToken);

        await group.DisposeAsync();
        await group.DisposeAsync();

        Assert.Single(server.ReceivedMethods, method => method == "session.end");
    }

    [Fact]
    public async Task SessionThatCannotBeEndedIsReportedAndDisposalContinues()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        server.FailWith("session.end", "unknown error", "session end refused");
        BrowserGroup group = await BrowserGroup.LaunchAsync(Launcher(server), cancellationToken: TestContext.Current.CancellationToken);
        List<LogMessageEventArgs> messages = [];
        group.OnLogMessage.AddObserver(messages.Add);

        await group.DisposeAsync();

        LogMessageEventArgs message = Assert.Single(messages);
        Assert.Equal(WebDriverBiDiLogLevel.Warn, message.Level);
        Assert.Equal(BrowserGroup.LoggerComponentName, message.ComponentName);
        Assert.Contains("session end refused", message.Message);
        Assert.False(group.Driver.IsStarted);
    }

    [Fact]
    public async Task LaunchThatCannotStartSessionFails()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        server.FailWith("session.new", "session not created", "no sessions here");

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => BrowserGroup.LaunchAsync(Launcher(server), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("no sessions here", exception.Message);
    }

    [Fact]
    public async Task CancelledLaunchFails()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BrowserGroup.LaunchAsync(Launcher(server), cancellationToken: cancellation.Token));

        Assert.Empty(server.ReceivedMethods);
    }

    [Fact]
    public async Task GroupWithoutOptionsUsesTheDefaults()
    {
        (BiDiDriver driver, FakeSession _) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(TimeSpan.FromSeconds(30), group.Options.ActionTimeout);
    }

    [Fact]
    public async Task ConnectedGroupUsesTheDriverAndLeavesItRunning()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        AutomationOptions options = new();

        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, options, TestContext.Current.CancellationToken);
        await group.DisposeAsync();

        Assert.Same(driver, group.Driver);
        Assert.Same(options, group.Options);
        Assert.True(driver.IsStarted);
        Assert.Equal(["session.subscribe", "script.addPreloadScript", "browser.getUserContexts", "browsingContext.getTree", "session.unsubscribe", "script.removePreloadScript"], session.RemoteEnd.SentCommands.Select(command => (string)command["method"]!));
    }

    [Fact]
    public async Task CancelledConnectionFails()
    {
        (BiDiDriver driver, FakeSession _) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BrowserGroup.ConnectAsync(driver, cancellationToken: new CancellationToken(true)));
    }

    // A launcher for an existing endpoint that is not a DevTools one uses a plain transport and starts no session.
    private static BrowserLauncherBuilder Launcher(ScriptedBiDiServer server)
    {
        return BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(server.Url);
    }
}
