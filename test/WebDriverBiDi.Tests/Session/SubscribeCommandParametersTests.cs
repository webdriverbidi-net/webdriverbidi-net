namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SubscribeCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SubscribeCommandParameters properties = new(["my.event"]);
        Assert.Equal("session.subscribe", properties.MethodName);
    }

    [Fact]
    public void TestCannotInitializeWithEmptyEventList()
    {
        Assert.Throws<ArgumentException>(() => new SubscribeCommandParameters(Array.Empty<string>()));
    }

    [Fact]
    public void TestCanSerializeParametersWithASingleEvent()
    {
        SubscribeCommandParameters properties = new("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Single(eventsToken);
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEvents()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Single(eventsToken);
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBrowsingContextData()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        properties.Contexts.Add("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Single(eventsToken);
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        Assert.Single(contextsToken);
        Assert.Equal("myContext", contextsToken[0]!.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUserContextData()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        properties.UserContexts.Add("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Single(eventsToken);
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        Assert.Single(userContextsToken);
        Assert.Equal("myUserContext", userContextsToken[0]!.Value<string>());
    }

    [Fact]
    public void TestInitializeUsingConstructor()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"], ["someUserContext"]);

        Assert.Single(properties.Events);
        Assert.Contains("someEvent", properties.Events);
        Assert.Single(properties.Contexts);
        Assert.Contains("someContext", properties.Contexts);
        Assert.Single(properties.UserContexts);
        Assert.Contains("someUserContext", properties.UserContexts);
    }

    [Fact]
    public void TestInitializeUsingConstructorForBrowsingContexts()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"]);

        Assert.Single(properties.Events);
        Assert.Contains("someEvent", properties.Events);
        Assert.Single(properties.Contexts);
        Assert.Contains("someContext", properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestInitializeUsingConstructorForBrowsingContextsWithEmptyUserContextList()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"], []);

        Assert.Single(properties.Events);
        Assert.Contains("someEvent", properties.Events);
        Assert.Single(properties.Contexts);
        Assert.Contains("someContext", properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestInitializeUsingConstructorForUserContexts()
    {
        SubscribeCommandParameters properties = new(["someEvent"], null, ["someUserContext"]);

        Assert.Single(properties.Events);
        Assert.Contains("someEvent", properties.Events);
        Assert.Empty(properties.Contexts);
        Assert.Single(properties.UserContexts);
        Assert.Contains("someUserContext", properties.UserContexts);
    }

    [Fact]
    public void TestInitializeUsingConstructorForUserContextsWithEmptyBrowsingContextList()
    {
        SubscribeCommandParameters properties = new(["someEvent"], [], ["someUserContext"]);

        Assert.Single(properties.Events);
        Assert.Contains("someEvent", properties.Events);
        Assert.Empty(properties.Contexts);
        Assert.Single(properties.UserContexts);
        Assert.Contains("someUserContext", properties.UserContexts);
    }
}
