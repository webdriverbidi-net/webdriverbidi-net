namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GetDataCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        GetDataCommandParameters properties = new("myRequestId");
        Assert.Equal("network.getData", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        GetDataCommandParameters properties = new("myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("dataType"));
        JToken? dataType = serialized["dataType"];
        Assert.NotNull(dataType);
        Assert.Equal(JTokenType.String, dataType.Type);
        Assert.Equal("response", dataType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithCollectorId()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("dataType"));
        JToken? dataType = serialized["dataType"];
        Assert.NotNull(dataType);
        Assert.Equal(JTokenType.String, dataType.Type);
        Assert.Equal("response", dataType.Value<string>());

        Assert.True(serialized.ContainsKey("collector"));
        JToken? collector = serialized["collector"];
        Assert.NotNull(collector);
        Assert.Equal(JTokenType.String, collector.Type);
        Assert.Equal("myCollectorId", collector.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDisownDataTrue()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId",
            DisownCollectedData = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("dataType"));
        JToken? dataType = serialized["dataType"];
        Assert.NotNull(dataType);
        Assert.Equal(JTokenType.String, dataType.Type);
        Assert.Equal("response", dataType.Value<string>());

        Assert.True(serialized.ContainsKey("collector"));
        JToken? collector = serialized["collector"];
        Assert.NotNull(collector);
        Assert.Equal(JTokenType.String, collector.Type);
        Assert.Equal("myCollectorId", collector.Value<string>());

        Assert.True(serialized.ContainsKey("disown"));
        JToken? disown = serialized["disown"];
        Assert.NotNull(disown);
        Assert.Equal(JTokenType.Boolean, disown.Type);
        Assert.True(disown.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDisownDataFalse()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId",
            DisownCollectedData = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("dataType"));
        JToken? dataType = serialized["dataType"];
        Assert.NotNull(dataType);
        Assert.Equal(JTokenType.String, dataType.Type);
        Assert.Equal("response", dataType.Value<string>());

        Assert.True(serialized.ContainsKey("collector"));
        JToken? collector = serialized["collector"];
        Assert.NotNull(collector);
        Assert.Equal(JTokenType.String, collector.Type);
        Assert.Equal("myCollectorId", collector.Value<string>());

        Assert.True(serialized.ContainsKey("disown"));
        JToken? disown = serialized["disown"];
        Assert.NotNull(disown);
        Assert.Equal(JTokenType.Boolean, disown.Type);
        Assert.False(disown.Value<bool>());
    }
}
