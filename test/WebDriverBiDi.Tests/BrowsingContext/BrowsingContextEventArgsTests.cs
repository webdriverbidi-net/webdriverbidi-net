namespace WebDriverBiDi.BrowsingContext;

using WebDriverBiDi.TestUtilities;

public class BrowsingContextEventArgsTests
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
                          "children": []
                        }
                      }
                      """;
        BrowsingContextEventArgs? eventArgs = await this.GenerateEventArgs(json);
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
                        "method": "browsingContext.contextCreated",
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
        BrowsingContextEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("openerContext", eventArgs.OriginalOpener);
        Assert.NotNull(eventArgs.Children);
        Assert.Single(eventArgs.Children);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithOptionalParent()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params":                         {
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
        BrowsingContextEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("openerContext", eventArgs.OriginalOpener);
        Assert.NotNull(eventArgs.Children);
        Assert.Empty(eventArgs.Children);
        Assert.NotNull(eventArgs.Parent);
        Assert.Equal("parentContextId", eventArgs.Parent);
    }

    [Fact]
    public async Task TestCanDeserializeWithNullOriginalOpener()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params":                         {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": null,
                          "userContext": "myUserContextId",
                          "children": []
                        }
                      }
                      """;
        BrowsingContextEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
        Assert.Null(eventArgs.OriginalOpener);
        Assert.NotNull(eventArgs.Children);
        Assert.Empty(eventArgs.Children);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public async Task TestCopySemantics()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "browsingContext.contextCreated",
                        "params":                         {
                          "context": "myContextId",
                          "clientWindow": "myClientWindowId",
                          "url": "http://example.com",
                          "originalOpener": "openerContext",
                          "userContext": "myUserContextId",
                          "children": []
                        }
                      }
                      """;
        BrowsingContextEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        BrowsingContextEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    private async Task<BrowsingContextEventArgs?> GenerateEventArgs(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        BrowsingContextEventArgs? eventArgs = null;
        using EventObserver<BrowsingContextEventArgs> observer = driver.BrowsingContext.OnContextCreated.AddObserver(e => eventArgs = e);

        observer.StartCapturingTasks();
        await connection.RaiseDataReceivedEventAsync(json);
        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(1));
        return eventArgs;
    }
}
