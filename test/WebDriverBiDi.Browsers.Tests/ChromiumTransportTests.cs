// <copyright file="ChromiumTransportTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Text.Json.Nodes;
using WebDriverBiDi.Browsers.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using static WebDriverBiDi.Browsers.TestUtilities.FakeDevToolsConnection;

public class ChromiumTransportTests
{
    private const string ConnectionString = "ws://127.0.0.1:9222/devtools/browser/fake";

    [Fact]
    public async Task ConnectBootstrapsMapperInHiddenTab()
    {
        FakeDevToolsConnection connection = new();
        await using ChromiumTransport transport = new(connection);

        await transport.ConnectAsync(ConnectionString, TestContext.Current.CancellationToken);

        JsonObject[] commands = [.. connection.SentCommands];
        Assert.Equal(
            ["Target.createTarget", "Target.attachToTarget", "Runtime.enable", "Runtime.evaluate", "Target.exposeDevToolsProtocol", "Runtime.evaluate", "Runtime.evaluate", "Runtime.addBinding"],
            commands.Select(command => (string)command["method"]!));
        Assert.True((bool)commands[0]["params"]!["hidden"]!);
        Assert.Equal(MapperTargetId, (string?)commands[1]["params"]!["targetId"]);
        Assert.True((bool)commands[1]["params"]!["flatten"]!);
        Assert.All(commands.Where(command => ((string)command["method"]!).StartsWith("Runtime.", StringComparison.Ordinal)), command => Assert.Equal(MapperSessionId, (string?)command["sessionId"]));
        Assert.Contains("runMapperInstance", (string?)commands[6]["params"]!["expression"]);
        Assert.Equal($"window.runMapperInstance(\"{MapperTargetId}\")", (string?)commands[6]["params"]!["expression"]);
        Assert.Equal("sendBidiResponse", (string?)commands[7]["params"]!["name"]);
    }

    [Fact]
    public async Task CommandsRelayThroughMapperAndResponsesReturnThroughBinding()
    {
        FakeDevToolsConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromSeconds(10), new ChromiumTransport(connection));
        await driver.StartAsync(ConnectionString, TestContext.Current.CancellationToken);

        await driver.BrowsingContext.ActivateAsync(new ActivateCommandParameters("context-with-\"quotes\""), cancellationToken: TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);

        JsonObject relay = connection.SentCommands[^1];
        Assert.Equal("Runtime.evaluate", (string?)relay["method"]);
        Assert.Equal(MapperSessionId, (string?)relay["sessionId"]);
        string expression = (string)relay["params"]!["expression"]!;
        JsonNode bidiCommand = JsonNode.Parse((string)JsonNode.Parse(expression["window.onBidiMessage(".Length..^1])!)!;
        Assert.Equal("browsingContext.activate", (string?)bidiCommand["method"]);
        Assert.Equal("context-with-\"quotes\"", (string?)bidiCommand["params"]!["context"]);
    }

    [Fact]
    public async Task FailedBootstrapCommandIsRetriedWithSameParametersAndSession()
    {
        FakeDevToolsConnection connection = new();
        connection.FailNext("Runtime.enable", 2);
        connection.FailNext("Target.attachToTarget", 1);
        await using ChromiumTransport transport = new(connection);

        await transport.ConnectAsync(ConnectionString, TestContext.Current.CancellationToken);

        JsonObject[] enables = [.. connection.SentCommands.Where(command => (string?)command["method"] == "Runtime.enable")];
        Assert.Equal(3, enables.Length);
        Assert.All(enables, command => Assert.Equal(MapperSessionId, (string?)command["sessionId"]));
        Assert.Equal(3, enables.Select(command => (long)command["id"]!).Distinct().Count());
        JsonObject[] attaches = [.. connection.SentCommands.Where(command => (string?)command["method"] == "Target.attachToTarget")];
        Assert.Equal(2, attaches.Length);
        Assert.All(attaches, command => Assert.Equal(MapperTargetId, (string?)command["params"]!["targetId"]));
    }

    [Fact]
    public async Task UnansweredBootstrapCommandFailsWithinInitializationTimeoutAndDisconnects()
    {
        FakeDevToolsConnection connection = new();
        connection.NeverAnswer("Runtime.addBinding");
        await using ChromiumTransport transport = new(connection) { InitializationTimeout = TimeSpan.FromMilliseconds(500) };

        WebDriverBiDiException exception = await Assert.ThrowsAsync<WebDriverBiDiException>(() => transport.ConnectAsync(ConnectionString, TestContext.Current.CancellationToken));

        Assert.Contains("'Runtime.addBinding' within 0.5 seconds", exception.Message);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task PersistentlyFailingBootstrapCommandReportsRetries()
    {
        FakeDevToolsConnection connection = new();
        connection.FailNext("Target.createTarget", int.MaxValue);
        await using ChromiumTransport transport = new(connection) { InitializationTimeout = TimeSpan.FromMilliseconds(500) };

        WebDriverBiDiException exception = await Assert.ThrowsAsync<WebDriverBiDiException>(() => transport.ConnectAsync(ConnectionString, TestContext.Current.CancellationToken));

        Assert.Contains("'Target.createTarget'", exception.Message);
        Assert.Contains("retried", exception.Message);
    }

    [Theory]
    [InlineData("Target.createTarget", "targetId", "target ID", true)]
    [InlineData("Target.attachToTarget", "sessionId", "session ID", true)]
    [InlineData("Target.createTarget", "targetId", "target ID", false)]
    [InlineData("Target.attachToTarget", "sessionId", "session ID", false)]
    public async Task EmptyIdentifierFromBrowserFailsConnect(string method, string property, string expectedMessage, bool isEmptyString)
    {
        FakeDevToolsConnection connection = new();
        connection.AnswerWith(method, new JsonObject() { [property] = isEmptyString ? string.Empty : null });
        await using ChromiumTransport transport = new(connection);

        WebDriverBiDiException exception = await Assert.ThrowsAsync<WebDriverBiDiException>(() => transport.ConnectAsync(ConnectionString, TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task CancelledConnectThrowsOperationCanceled()
    {
        FakeDevToolsConnection connection = new();
        connection.NeverAnswer("Target.createTarget");
        await using ChromiumTransport transport = new(connection);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transport.ConnectAsync(ConnectionString, cancellationSource.Token));
    }

    // DevTools traffic other than the mapper's responses must not reach the WebDriver BiDi pipeline.
    [Fact]
    public async Task UnrelatedDevToolsMessagesAreIgnored()
    {
        FakeDevToolsConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromSeconds(10), new ChromiumTransport(connection));
        List<object> unexpected = [];
        driver.OnUnexpectedErrorReceived.AddObserver(e => unexpected.Add(e));
        await driver.StartAsync(ConnectionString, TestContext.Current.CancellationToken);

        await connection.DeliverAsync(new JsonObject() { ["id"] = 999_999, ["result"] = new JsonObject() });
        await connection.DeliverAsync(new JsonObject() { ["method"] = "Target.targetCreated", ["params"] = new JsonObject() });
        await connection.DeliverAsync(new JsonObject() { ["method"] = "Runtime.bindingCalled" });
        await connection.DeliverAsync(new JsonObject() { ["method"] = "Runtime.bindingCalled", ["params"] = new JsonObject() { ["name"] = "otherBinding", ["payload"] = "{}" } });
        await connection.DeliverAsync(new JsonObject() { ["method"] = "Runtime.bindingCalled", ["params"] = new JsonObject() { ["name"] = "sendBidiResponse", ["payload"] = null } });
        await driver.BrowsingContext.ActivateAsync(new ActivateCommandParameters("context"), cancellationToken: TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);

        Assert.Empty(unexpected);
    }
}
