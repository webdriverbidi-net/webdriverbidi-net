namespace WebDriverBiDi.BrowsingContext;

using WebDriverBiDi.TestUtilities;

public class ContextCreatedEventArgsTests
{
    [Fact]
    public async Task TestCanDeserialize()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": false,
                          "children": []
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
        Assert.Equal("openerContext", eventArgs.OriginalOpener);
        Assert.False(eventArgs.HasPlannedNavigation);
        Assert.NotNull(eventArgs.Children);
        Assert.Empty(eventArgs.Children);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithHasPlannedNavigation()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": true,
                          "children": []
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.True(eventArgs.HasPlannedNavigation);
    }

    [Fact]
    public async Task TestCanDeserializeWithChildren()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "default",
                          "hasPlannedNavigation": false,
                          "children": [
                            {
                              "context": "childContextId",
                              "clientWindow": "myClientWindowId",
                              "url": "http://example.com/subdirectory",
                              "originalOpener": null,
                              "userContext": "default",
                              "children": []
                            }
                          ]
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.NotNull(eventArgs.Children);
        Assert.Single(eventArgs.Children);
        Assert.Equal("childContextId", eventArgs.Children[0].BrowsingContextId);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithOptionalParent()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "userContext": "myUserContextId",
                          "originalOpener": "openerContext",
                          "hasPlannedNavigation": false,
                          "children": [],
                          "parent": "parentContextId"
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("parentContextId", eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithNullOriginalOpener()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": null,
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": false,
                          "children": []
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Null(eventArgs.OriginalOpener);
    }

    [Fact]
    public async Task TestCanDeserializeWithNullChildren()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": false,
                          "children": null
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Null(eventArgs.Children);
    }

    [Fact]
    public async Task TestCopySemantics()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": true,
                          "children": []
                        }
                      }
                      """;
        ContextCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        ContextCreatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public async Task TestDeserializingWithMissingHasPlannedNavigationFails()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "children": []
                        }
                      }
                      """;
        await this.AssertEventParsingFails(json);
    }

    [Fact]
    public async Task TestDeserializingWithInvalidHasPlannedNavigationTypeFails()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "hasPlannedNavigation": "false",
                          "children": []
                        }
                      }
                      """;
        await this.AssertEventParsingFails(json);
    }

    private async Task<ContextCreatedEventArgs?> GenerateEventArgs(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);

        ContextCreatedEventArgs? eventArgs = null;
        using EventObserver<ContextCreatedEventArgs> observer = driver.BrowsingContext.OnContextCreated.AddObserver(e => eventArgs = e);

        observer.StartCapturingTasks();
        await connection.RaiseDataReceivedEventAsync(json);
        Assert.True(await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        return eventArgs;
    }

    private async Task AssertEventParsingFails(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);

        TaskCompletionSource<string> errorLogged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        driver.OnLogMessage.AddObserver(e =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Error)
            {
                errorLogged.TrySetResult(e.Message);
            }
        });

        bool observerCalled = false;
        driver.BrowsingContext.OnContextCreated.AddObserver(e => observerCalled = true);

        await connection.RaiseDataReceivedEventAsync(json);
        string message = await errorLogged.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Contains("Unexpected error parsing event JSON", message);
        Assert.False(observerCalled);
    }
}
