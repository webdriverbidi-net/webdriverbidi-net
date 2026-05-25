namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetExtraHeadersCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetExtraHeadersCommandParameters properties = new();
        Assert.Equal("network.setExtraHeaders", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetExtraHeadersCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headersArray = headersToken as JArray;
        Assert.NotNull(headersArray);
        Assert.Empty(headersArray);
    }

    [Fact]
    public void TestCanSetHeadersUsingProperty()
    {
        SetExtraHeadersCommandParameters properties = new()
        {
            Headers = ["X-Extra-Header: headerValue"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headersArray = headersToken as JArray;
        Assert.NotNull(headersArray);
        Assert.Single(headersArray);
        Assert.Equal(JTokenType.String, headersArray[0].Type);
        Assert.Equal("X-Extra-Header: headerValue", headersArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithContexts()
    {
        SetExtraHeadersCommandParameters properties = new()
        {
            Contexts = ["myContext"]
        };
        properties.Headers.Add("X-Extra-Header: headerValue");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headersArray = headersToken as JArray;
        Assert.NotNull(headersArray);
        Assert.Single(headersArray);
        Assert.Equal(JTokenType.String, headersArray[0].Type);
        Assert.Equal("X-Extra-Header: headerValue", headersArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        JArray? contextsObject = contextsToken as JArray;
        Assert.NotNull(contextsObject);
        Assert.Single(contextsObject);
        Assert.Equal(JTokenType.String, contextsObject[0].Type);
        Assert.Equal("myContext", contextsObject[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithUserContexts()
    {
        SetExtraHeadersCommandParameters properties = new()
        {
            UserContexts = ["myUserContext"]
        };
        properties.Headers.Add("X-Extra-Header: headerValue");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headersArray = headersToken as JArray;
        Assert.NotNull(headersArray);
        Assert.Single(headersArray);
        Assert.Equal(JTokenType.String, headersArray[0].Type);
        Assert.Equal("X-Extra-Header: headerValue", headersArray[0].Value<string>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        JArray? contextsObject = userContextsToken as JArray;
        Assert.NotNull(contextsObject);
        Assert.Single(contextsObject);
        Assert.Equal(JTokenType.String, contextsObject[0].Type);
        Assert.Equal("myUserContext", contextsObject[0].Value<string>());
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetExtraHeadersCommandParameters properties = SetExtraHeadersCommandParameters.ResetExtraHeaders;
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headersArray = headersToken as JArray;
        Assert.NotNull(headersArray);
        Assert.Empty(headersArray);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetExtraHeadersCommandParameters firstInstance = SetExtraHeadersCommandParameters.ResetExtraHeaders;
        SetExtraHeadersCommandParameters secondInstance = SetExtraHeadersCommandParameters.ResetExtraHeaders;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
