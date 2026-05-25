namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UnsubscribeCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        UnsubscribeByAttributesCommandParameters byAttributesProperties = new();
        Assert.Equal("session.unsubscribe", byAttributesProperties.MethodName);
        UnsubscribeByIdsCommandParameters byIdProperties = new();
        Assert.Equal("session.unsubscribe", byIdProperties.MethodName);
    }

    [Fact]
    public void TestCanSerializeByAttributesParameters()
    {
        UnsubscribeByAttributesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("events"));
        JToken? events = serialized["events"];
        Assert.NotNull(events);
        Assert.Empty(events);
    }

    [Fact]
    public void TestCanSerializeByAttributesParametersWithEvents()
    {
        UnsubscribeByAttributesCommandParameters properties = new();
        properties.Events.Add("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Single(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());
    }

    [Fact]
    public void TestCanSerializeByIdsParameters()
    {
        UnsubscribeByIdsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("subscriptions"));
        JToken? subscriptions = serialized["subscriptions"];
        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public void TestCanSerializeByIdsParametersWithEvents()
    {
        UnsubscribeByIdsCommandParameters properties = new();
        properties.SubscriptionIds.Add("mySubscriptionId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("subscriptions"));
        JToken? subscriptionsToken = serialized["subscriptions"];
        Assert.NotNull(subscriptionsToken);
        Assert.Single(subscriptionsToken);
        Assert.Equal(JTokenType.Array, subscriptionsToken.Type);
        Assert.Equal("mySubscriptionId", subscriptionsToken[0]!.Value<string>());
    }
}
