namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class BrowsingContextEventArgsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<BrowsingContextInfo>(info);
        BrowsingContextEventArgs eventArgs = new(info);

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
    public void TestCanDeserializeWithChildren()
    {
        string json = """
                      {
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
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<BrowsingContextInfo>(info);
        BrowsingContextEventArgs eventArgs = new(info);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myClientWindowId", eventArgs.ClientWindowId);
        Assert.Equal("openerContext", eventArgs.OriginalOpener);
        Assert.NotNull(eventArgs.Children);
        Assert.Single(eventArgs.Children);
        Assert.Null(eventArgs.Parent);
    }

    [Fact]
    public void TestCanDeserializeWithOptionalParent()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "userContext": "myUserContextId",
                        "originalOpener": "openerContext",
                        "children": [],
                        "parent": "parentContextId"
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<BrowsingContextInfo>(info);
        BrowsingContextEventArgs eventArgs = new(info);

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
    public void TestCanDeserializeWithNullOriginalOpener()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": null,
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<BrowsingContextInfo>(info);
        BrowsingContextEventArgs eventArgs = new(info);

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
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.NotNull(info);
        BrowsingContextEventArgs eventArgs = new(info);
        BrowsingContextEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
