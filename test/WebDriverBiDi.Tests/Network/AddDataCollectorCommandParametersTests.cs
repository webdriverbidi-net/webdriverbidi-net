namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class AddDataCollectorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        AddDataCollectorCommandParameters properties = new(1024);
        Assert.Equal("network.addDataCollector", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        AddDataCollectorCommandParameters properties = new(1024);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("response", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(1024u, maxEncodedDataSize.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersSpecifyingResponseCollection()
    {
        AddDataCollectorCommandParameters properties = new(1024, DataType.Response);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("response", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(1024u, maxEncodedDataSize.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersSpecifyingRequestAndResponseCollection()
    {
        AddDataCollectorCommandParameters properties = new(1024, DataType.Response, DataType.Request);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Equal(2, dataTypesArray.Count);
        List<string>? dataTypesValues = dataTypesArray.ToObject<List<string>>();
        Assert.NotNull(dataTypesValues);
        Assert.Contains("request", dataTypesValues);
        Assert.Contains("response", dataTypesValues);

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(1024u, maxEncodedDataSize.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersSpecifyingDuplicateCollection()
    {
        AddDataCollectorCommandParameters properties = new(1024, DataType.Request, DataType.Request, DataType.Response);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Equal(2, dataTypesArray.Count);
        List<string>? dataTypesValues = dataTypesArray.ToObject<List<string>>();
        Assert.NotNull(dataTypesValues);
        Assert.Contains("request", dataTypesValues);
        Assert.Contains("response", dataTypesValues);

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(1024u, maxEncodedDataSize.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersSpecifyingRequestCollection()
    {
        AddDataCollectorCommandParameters properties = new(100, DataType.Request);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("request", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(100u, maxEncodedDataSize.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBrowsingContexts()
    {
        AddDataCollectorCommandParameters properties = new(100);
        properties.BrowsingContexts.Add("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("response", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(100UL, maxEncodedDataSize.Value<ulong>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsArray = contextsToken as JArray;
        Assert.NotNull(contextsArray);
        Assert.Single(contextsArray);
        Assert.Equal("myContext", contextsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUserContexts()
    {
        AddDataCollectorCommandParameters properties = new(100);
        properties.UserContexts.Add("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("response", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(100UL, maxEncodedDataSize.Value<ulong>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken as JArray;
        Assert.NotNull(userContextsArray);
        Assert.Single(userContextsArray);
        Assert.Equal("myUserContext", userContextsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithCollectorType()
    {
        AddDataCollectorCommandParameters properties = new(100)
        {
            CollectorType = CollectorType.Blob,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("dataTypes"));
        JToken? dataTypesToken = serialized["dataTypes"];
        Assert.NotNull(dataTypesToken);
        Assert.Equal(JTokenType.Array, dataTypesToken.Type);
        JArray? dataTypesArray = dataTypesToken as JArray;
        Assert.NotNull(dataTypesArray);
        Assert.Single(dataTypesArray);
        Assert.Equal("response", dataTypesArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("maxEncodedDataSize"));
        JToken? maxEncodedDataSize = serialized["maxEncodedDataSize"];
        Assert.NotNull(maxEncodedDataSize);
        Assert.Equal(JTokenType.Integer, maxEncodedDataSize.Type);
        Assert.Equal(100UL, maxEncodedDataSize.Value<ulong>());

        Assert.True(serialized.ContainsKey("collectorType"));
        JToken? collectorType = serialized["collectorType"];
        Assert.NotNull(collectorType);
        Assert.Equal(JTokenType.String, collectorType.Type);
        Assert.Equal("blob", collectorType.Value<string>());
    }
}
