namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class LocateNodesCommandResultTests
{
    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nodes, Has.Count.EqualTo(1));
        Assert.That(result.Nodes[0].SharedId, Is.EqualTo("mySharedId"));
    }

    [Test]
    public void TestCanDeserializeWithEmptyResult()
    {
        string json = """
                      {
                        "nodes": []
                      }
                      """;
        LocateNodesCommandResult? result = JsonSerializer.Deserialize<LocateNodesCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nodes, Has.Count.EqualTo(0));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        LocateNodesCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "nodes": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<LocateNodesCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
