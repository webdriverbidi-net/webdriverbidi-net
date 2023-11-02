namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class LocateNodesCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""nodes"": [{ ""type"": ""node"", ""sharedId"": ""mySharedId"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } }] }";
        LocateNodesCommandResult? result = JsonConvert.DeserializeObject<LocateNodesCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nodes, Has.Count.EqualTo(1));
        Assert.That(result!.Nodes[0].SharedId, Is.EqualTo("mySharedId"));
    }

    [Test]
    public void TestCanDeserializeWithEmptyResult()
    {
        string json = @"{ ""nodes"": [] }";
        LocateNodesCommandResult? result = JsonConvert.DeserializeObject<LocateNodesCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nodes, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<LocateNodesCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""nodes"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<LocateNodesCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}
