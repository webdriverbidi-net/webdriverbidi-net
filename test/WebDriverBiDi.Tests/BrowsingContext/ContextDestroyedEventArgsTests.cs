namespace WebDriverBiDi.BrowsingContext;

using WebDriverBiDi.TestUtilities;

public class ContextDestroyedEventArgsTests
{
    [Fact]
    public async Task TestCanDeserialize()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextDestroyed",
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
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
        Assert.Equal("openerContext", eventArgs.OriginalOpener);
        Assert.NotNull(eventArgs.Children);
        Assert.Empty(eventArgs.Children);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithChildren()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextDestroyed",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "default",
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
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
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
                        "method": "browsingContext.contextDestroyed",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "userContext": "myUserContextId",
                          "originalOpener": "openerContext",
                          "children": [],
                          "parent": "parentContextId"
                        }
                      }
                      """;
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
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
                        "method": "browsingContext.contextDestroyed",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": null,
                          "userContext": "myUserContextId",
                          "children": []
                        }
                      }
                      """;
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
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
                        "method": "browsingContext.contextDestroyed",
                        "params": {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "children": null
                        }
                      }
                      """;
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
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
                        "method": "browsingContext.contextDestroyed",
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
        ContextDestroyedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        ContextDestroyedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    private async Task<ContextDestroyedEventArgs?> GenerateEventArgs(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);

        ContextDestroyedEventArgs? eventArgs = null;
        using EventObserver<ContextDestroyedEventArgs> observer = driver.BrowsingContext.OnContextDestroyed.AddObserver(e => eventArgs = e);

        observer.StartCapturingTasks();
        await connection.RaiseDataReceivedEventAsync(json);
        Assert.True(await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        return eventArgs;
    }
}
