namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class StorageKeyPartitionDescriptorTests
{
    [Fact]
    public void TestCanSerialize()
    {
        StorageKeyPartitionDescriptor properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("storageKey", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithUserContext()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            UserContextId = "myUserContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("storageKey", type.Value<string>());

        Assert.True(serialized.ContainsKey("userContext"));
        JToken? userContext = serialized["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myUserContext", userContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithSourceOrigin()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            SourceOrigin = "mySourceOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("storageKey", type.Value<string>());

        Assert.True(serialized.ContainsKey("sourceOrigin"));
        JToken? sourceOrigin = serialized["sourceOrigin"];
        Assert.NotNull(sourceOrigin);
        Assert.Equal(JTokenType.String, sourceOrigin.Type);
        Assert.Equal("mySourceOrigin", sourceOrigin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithAllProperties()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            UserContextId = "myUserContext",
            SourceOrigin = "mySourceOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("storageKey", type.Value<string>());

        Assert.True(serialized.ContainsKey("userContext"));
        JToken? userContext = serialized["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myUserContext", userContext.Value<string>());

        Assert.True(serialized.ContainsKey("sourceOrigin"));
        JToken? sourceOrigin = serialized["sourceOrigin"];
        Assert.NotNull(sourceOrigin);
        Assert.Equal(JTokenType.String, sourceOrigin.Type);
        Assert.Equal("mySourceOrigin", sourceOrigin.Value<string>());
    }
}
