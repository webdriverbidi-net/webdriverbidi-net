namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class GetTreeCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "contexts": [
                          {
                            "context": "myContextId",
                            "clientWindow": "myClientWindow",
                            "url": "http://example.com",
                            "originalOpener": "openerContext",
                            "userContext": "default",
                            "children": []
                          }
                        ]
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json);
        Assert.NotNull(result);
        Assert.Single(result.ContextTree);
    }

    [Fact]
    public void TestCanDeserializeWithNoContexts()
    {
        string json = """
                      {
                        "contexts": []
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json);
        Assert.NotNull(result);
        Assert.Empty(result.ContextTree);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "contexts": [
                          {
                            "context": "myContextId",
                            "clientWindow": "myClientWindow",
                            "url": "http://example.com",
                            "originalOpener": "openerContext",
                            "userContext": "default",
                            "children": []
                          }
                        ]
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json);
        Assert.NotNull(result);
        GetTreeCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextsThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextsTypeThrows()
    {
        string json = """
                      {
                        "contexts": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextValueTypeThrows()
    {
        string json = """
                      {
                        "contexts": [ "invalid" ]
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json));
    }
}
