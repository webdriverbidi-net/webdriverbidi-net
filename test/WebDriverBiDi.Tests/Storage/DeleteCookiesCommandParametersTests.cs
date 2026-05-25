namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class DeleteCookiesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        DeleteCookiesCommandParameters properties = new();
        Assert.Equal("storage.deleteCookies", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        DeleteCookiesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeParametersWithCookieFilter()
    {
        DeleteCookiesCommandParameters properties = new()
        {
            Filter = new CookieFilter()
            {
                Name = "cookieName"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("filter"));
        JToken? filterToken = serialized["filter"];
        Assert.NotNull(filterToken);
        Assert.Equal(JTokenType.Object, filterToken.Type);
        JObject? filterObject = filterToken as JObject;
        Assert.NotNull(filterObject);
        Assert.Single(filterObject);
        Assert.True(filterObject.ContainsKey("name"));
        JToken? filterName = filterObject["name"];
        Assert.NotNull(filterName);
        Assert.Equal(JTokenType.String, filterName.Type);
        Assert.Equal("cookieName", filterName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBrowsingContextPartitionDescriptor()
    {
        DeleteCookiesCommandParameters properties = new()
        {
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("partition"));
        JToken? partitionToken = serialized["partition"];
        Assert.NotNull(partitionToken);
        Assert.Equal(JTokenType.Object, partitionToken.Type);
        JObject? partitionObject = partitionToken as JObject;
        Assert.NotNull(partitionObject);
        Assert.Equal(2, partitionObject.Count);
        Assert.True(partitionObject.ContainsKey("type"));
        JToken? partitionType = partitionObject["type"];
        Assert.NotNull(partitionType);
        Assert.Equal(JTokenType.String, partitionType.Type);
        Assert.Equal("context", partitionType.Value<string>());
        Assert.True(partitionObject.ContainsKey("context"));
        JToken? partitionContext = partitionObject["context"];
        Assert.NotNull(partitionContext);
        Assert.Equal(JTokenType.String, partitionContext.Type);
        Assert.Equal("myContext", partitionContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithStorageKeyPartitionDescriptor()
    {
        DeleteCookiesCommandParameters properties = new()
        {
            Partition = new StorageKeyPartitionDescriptor()
            {
                UserContextId = "myUserContext"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("partition"));
        JToken? partitionToken = serialized["partition"];
        Assert.NotNull(partitionToken);
        Assert.Equal(JTokenType.Object, partitionToken.Type);
        JObject? partitionObject = partitionToken as JObject;
        Assert.NotNull(partitionObject);
        Assert.Equal(2, partitionObject.Count);
        Assert.True(partitionObject.ContainsKey("type"));
        JToken? partitionType = partitionObject["type"];
        Assert.NotNull(partitionType);
        Assert.Equal(JTokenType.String, partitionType.Type);
        Assert.Equal("storageKey", partitionType.Value<string>());
        Assert.True(partitionObject.ContainsKey("userContext"));
        JToken? userContext = partitionObject["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myUserContext", userContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAllValues()
    {
        DeleteCookiesCommandParameters properties = new()
        {
            Filter = new CookieFilter()
            {
                Name = "cookieName",
            },
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("filter"));
        JToken? filterToken = serialized["filter"];
        Assert.NotNull(filterToken);
        Assert.Equal(JTokenType.Object, filterToken.Type);
        JObject? filterObject = filterToken as JObject;
        Assert.NotNull(filterObject);
        Assert.Single(filterObject);
        Assert.True(filterObject.ContainsKey("name"));
        JToken? filterName = filterObject["name"];
        Assert.NotNull(filterName);
        Assert.Equal(JTokenType.String, filterName.Type);
        Assert.Equal("cookieName", filterName.Value<string>());

        Assert.True(serialized.ContainsKey("partition"));
        JToken? partitionToken = serialized["partition"];
        Assert.NotNull(partitionToken);
        Assert.Equal(JTokenType.Object, partitionToken.Type);
        JObject? partitionObject = partitionToken as JObject;
        Assert.NotNull(partitionObject);
        Assert.Equal(2, partitionObject.Count);
        Assert.True(partitionObject.ContainsKey("type"));
        JToken? partitionType = partitionObject["type"];
        Assert.NotNull(partitionType);
        Assert.Equal(JTokenType.String, partitionType.Type);
        Assert.Equal("context", partitionType.Value<string>());
        Assert.True(partitionObject.ContainsKey("context"));
        JToken? partitionContext = partitionObject["context"];
        Assert.NotNull(partitionContext);
        Assert.Equal(JTokenType.String, partitionContext.Type);
        Assert.Equal("myContext", partitionContext.Value<string>());
    }
}
