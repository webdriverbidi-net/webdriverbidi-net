namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class LocateNodesCommandResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "nodes": [
                          {
                            "type": "node", 
                            "sharedId": "mySharedId",
                            "value": {
                              "nodeType": 1,
                              "nodeValue": "",
                              "childNodeCount": 0
                            }
                          }
                        ]
                      }
                      """;
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Single(result.Nodes);
        Assert.Equal("mySharedId", result.Nodes[0].SharedId);
    }

    [Fact]
    public void TestCanDeserializeWithEmptyResult()
    {
        string json = """
                      {
                        "nodes": []
                      }
                      """;
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Empty(result.Nodes);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "nodes": [
                          {
                            "type": "node", 
                            "sharedId": "mySharedId",
                            "value": {
                              "nodeType": 1,
                              "nodeValue": "",
                              "childNodeCount": 0
                            }
                          }
                        ]
                      }
                      """;
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options);
        Assert.NotNull(result);
        LocateNodesCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingNodesValueThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidNodesTypeThrows()
    {
        string json = """
                      {
                        "nodes": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullNodesThrows()
    {
        string json = """
                      {
                        "nodes": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json, this.options));
    }
}
