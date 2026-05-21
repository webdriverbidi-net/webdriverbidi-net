namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class LocateNodesCommandResultTests
{
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
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json);
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
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json);
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
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json);
        Assert.NotNull(result);
        LocateNodesCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "nodes": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json));
    }
}
