namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class BrowsingContextInfoTests
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

        Assert.Equal("myContextId", info.BrowsingContextId);
        Assert.Equal("http://example.com", info.Url);
        Assert.Equal("myClientWindowId", info.ClientWindowId);
        Assert.Equal("myUserContextId", info.UserContextId);
        Assert.Equal("openerContext", info.OriginalOpener);
        Assert.NotNull(info.Children);
        Assert.Empty(info.Children);
        Assert.Null(info.Parent);
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

        Assert.Equal("myContextId", info.BrowsingContextId);
        Assert.Equal("http://example.com", info.Url);
        Assert.Equal("myClientWindowId", info.ClientWindowId);
        Assert.Equal("openerContext", info.OriginalOpener);
        Assert.NotNull(info.Children);
        Assert.Single(info.Children);
        Assert.Null(info.Parent);
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

        Assert.Equal("myContextId", info.BrowsingContextId);
        Assert.Equal("http://example.com", info.Url);
        Assert.Equal("myClientWindowId", info.ClientWindowId);
        Assert.Equal("openerContext", info.OriginalOpener);
        Assert.NotNull(info.Children);
        Assert.Empty(info.Children);
        Assert.NotNull(info.Parent);
        Assert.Equal("parentContextId", info.Parent);
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

        Assert.Equal("myContextId", info.BrowsingContextId);
        Assert.Equal("http://example.com", info.Url);
        Assert.Equal("myClientWindowId", info.ClientWindowId);
        Assert.Equal("myUserContextId", info.UserContextId);
        Assert.Null(info.OriginalOpener);
        Assert.NotNull(info.Children);
        Assert.Empty(info.Children);
        Assert.Null(info.Parent);
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
        BrowsingContextInfo copy = info with { };
        Assert.Equal(info, copy);
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingContextThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingClientWindowIdThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingUrlThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingOriginalOpenerThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingChildrenThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithMissingUserContextThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidClientWindowThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": {},
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": {},
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": [] 
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidOriginalOpenerTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": {},
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": {},
                        "children": [] 
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidChildrenTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingBrowsingContextInfoWithInvalidParentTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": [],
                        "parent": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json));
    }
}
