namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

public class SetCookieCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"));
        Assert.Equal("storage.setCookie", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("cookie"));
        JToken? cookieToken = serialized["cookie"];
        Assert.NotNull(cookieToken);
        Assert.Equal(JTokenType.Object, cookieToken.Type);
        JObject? cookieObject = cookieToken as JObject;
        Assert.NotNull(cookieObject);
        Assert.Equal(3, cookieObject.Count);
        Assert.True(cookieObject.ContainsKey("name"));
        JToken? cookieName = cookieObject["name"];
        Assert.NotNull(cookieName);
        Assert.Equal(JTokenType.String, cookieName.Type);
        Assert.Equal("cookieName", cookieName.Value<string>());

        Assert.True(cookieObject.ContainsKey("value"));
        JToken? cookieValueToken = cookieObject["value"];
        Assert.NotNull(cookieValueToken);
        Assert.Equal(JTokenType.Object, cookieValueToken.Type);
        JObject? cookieValueObject = cookieValueToken as JObject;
        Assert.NotNull(cookieValueObject);
        Assert.Equal(2, cookieValueObject.Count);
        Assert.True(cookieValueObject.ContainsKey("type"));
        JToken? cookieValueType = cookieValueObject["type"];
        Assert.NotNull(cookieValueType);
        Assert.Equal(JTokenType.String, cookieValueType.Type);
        Assert.Equal("string", cookieValueType.Value<string>());
        Assert.True(cookieValueObject.ContainsKey("value"));
        JToken? cookieValueValue = cookieValueObject["value"];
        Assert.NotNull(cookieValueValue);
        Assert.Equal(JTokenType.String, cookieValueValue.Type);
        Assert.Equal("cookieValue", cookieValueValue.Value<string>());

        Assert.True(cookieObject.ContainsKey("domain"));
        JToken? cookieDomain = cookieObject["domain"];
        Assert.NotNull(cookieDomain);
        Assert.Equal(JTokenType.String, cookieDomain.Type);
        Assert.Equal("cookieDomain", cookieDomain.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBrowsingContextPartitionDescriptor()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"))
        {
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("cookie"));
        JToken? cookieToken = serialized["cookie"];
        Assert.NotNull(cookieToken);
        Assert.Equal(JTokenType.Object, cookieToken.Type);
        JObject? cookieObject = cookieToken as JObject;
        Assert.NotNull(cookieObject);
        Assert.Equal(3, cookieObject.Count);
        Assert.True(cookieObject.ContainsKey("name"));
        JToken? cookieName = cookieObject["name"];
        Assert.NotNull(cookieName);
        Assert.Equal(JTokenType.String, cookieName.Type);
        Assert.Equal("cookieName", cookieName.Value<string>());

        Assert.True(cookieObject.ContainsKey("value"));
        JToken? cookieValueToken = cookieObject["value"];
        Assert.NotNull(cookieValueToken);
        Assert.Equal(JTokenType.Object, cookieValueToken.Type);
        JObject? cookieValueObject = cookieValueToken as JObject;
        Assert.NotNull(cookieValueObject);
        Assert.Equal(2, cookieValueObject.Count);
        Assert.True(cookieValueObject.ContainsKey("type"));
        JToken? cookieValueType = cookieValueObject["type"];
        Assert.NotNull(cookieValueType);
        Assert.Equal(JTokenType.String, cookieValueType.Type);
        Assert.Equal("string", cookieValueType.Value<string>());
        Assert.True(cookieValueObject.ContainsKey("value"));
        JToken? cookieValueValue = cookieValueObject["value"];
        Assert.NotNull(cookieValueValue);
        Assert.Equal(JTokenType.String, cookieValueValue.Type);
        Assert.Equal("cookieValue", cookieValueValue.Value<string>());

        Assert.True(cookieObject.ContainsKey("domain"));
        JToken? cookieDomain = cookieObject["domain"];
        Assert.NotNull(cookieDomain);
        Assert.Equal(JTokenType.String, cookieDomain.Type);
        Assert.Equal("cookieDomain", cookieDomain.Value<string>());

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
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"))
        {
            Partition = new StorageKeyPartitionDescriptor()
            {
                UserContextId = "myUserContext"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("cookie"));
        JToken? cookieToken = serialized["cookie"];
        Assert.NotNull(cookieToken);
        Assert.Equal(JTokenType.Object, cookieToken.Type);
        JObject? cookieObject = cookieToken as JObject;
        Assert.NotNull(cookieObject);
        Assert.Equal(3, cookieObject.Count);
        Assert.True(cookieObject.ContainsKey("name"));
        JToken? cookieName = cookieObject["name"];
        Assert.NotNull(cookieName);
        Assert.Equal(JTokenType.String, cookieName.Type);
        Assert.Equal("cookieName", cookieName.Value<string>());

        Assert.True(cookieObject.ContainsKey("value"));
        JToken? cookieValueToken = cookieObject["value"];
        Assert.NotNull(cookieValueToken);
        Assert.Equal(JTokenType.Object, cookieValueToken.Type);
        JObject? cookieValueObject = cookieValueToken as JObject;
        Assert.NotNull(cookieValueObject);
        Assert.Equal(2, cookieValueObject.Count);
        Assert.True(cookieValueObject.ContainsKey("type"));
        JToken? cookieValueType = cookieValueObject["type"];
        Assert.NotNull(cookieValueType);
        Assert.Equal(JTokenType.String, cookieValueType.Type);
        Assert.Equal("string", cookieValueType.Value<string>());
        Assert.True(cookieValueObject.ContainsKey("value"));
        JToken? cookieValueValue = cookieValueObject["value"];
        Assert.NotNull(cookieValueValue);
        Assert.Equal(JTokenType.String, cookieValueValue.Type);
        Assert.Equal("cookieValue", cookieValueValue.Value<string>());

        Assert.True(cookieObject.ContainsKey("domain"));
        JToken? cookieDomain = cookieObject["domain"];
        Assert.NotNull(cookieDomain);
        Assert.Equal(JTokenType.String, cookieDomain.Type);
        Assert.Equal("cookieDomain", cookieDomain.Value<string>());

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
}
