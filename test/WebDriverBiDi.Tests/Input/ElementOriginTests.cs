namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

using WebDriverBiDi.Script;

public class ElementOriginTests
{
    [Fact]
    public void TestCanSerializeOrigin()
    {
        string nodeJson = """
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0
                            },
                            "sharedId": "testSharedId"
                          }
                          """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(nodeJson);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        SharedReference node = ((NodeRemoteValue)remoteValue).ToSharedReference();
        ElementOrigin origin = new(node);
        string json = JsonSerializer.Serialize(origin);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("element", type.Value<string>());

        Assert.True(serialized.ContainsKey("element"));
        JToken? elementToken = serialized["element"];
        Assert.NotNull(elementToken);
        Assert.Equal(JTokenType.Object, elementToken.Type);

        JObject? serializedElementReference = elementToken.Value<JObject>();
        Assert.NotNull(serializedElementReference);
        Assert.Single(serializedElementReference);
        Assert.True(serializedElementReference.ContainsKey("sharedId"));

        JToken? sharedId = serializedElementReference["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal(JTokenType.String, sharedId.Type);
        Assert.Equal("testSharedId", sharedId.Value<string>());
    }
}
