namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UnsubscribeCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        UnsubscribeByAttributesCommandParameters byAttributesProperties = new("some.event");
        Assert.Equal("session.unsubscribe", byAttributesProperties.MethodName);
        UnsubscribeByIdsCommandParameters byIdProperties = new("mySubscriptionId");
        Assert.Equal("session.unsubscribe", byIdProperties.MethodName);
    }

    [Fact]
    public void TestCanSerializeByAttributesParameters()
    {
        UnsubscribeByAttributesCommandParameters properties = new("some.event");
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
    public void TestCanSerializeByAttributesParametersWithMultipleEvents()
    {
        UnsubscribeByAttributesCommandParameters properties = new(["some.event", "some.otherEvent"]);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("events"));
        JToken? eventsToken = serialized["events"];
        Assert.NotNull(eventsToken);
        Assert.Equal(JTokenType.Array, eventsToken.Type);
        Assert.Equal(2, eventsToken.Count());
        Assert.Equal("some.event", eventsToken[0]!.Value<string>());
        Assert.Equal("some.otherEvent", eventsToken[1]!.Value<string>());
    }

    [Fact]
    public void TestConstructingByAttributesParametersWithEmptyEventListThrows()
    {
        // The specification requires an unsubscription to name at least one event; an
        // empty events list cannot be meaningful under any revision of the specification.
        Assert.Contains("At least one event must be specified.", Assert.Throws<ArgumentException>(() => new UnsubscribeByAttributesCommandParameters([])).Message);
    }

    [Fact]
    public void TestCanSerializeByIdsParameters()
    {
        UnsubscribeByIdsCommandParameters properties = new("mySubscriptionId");
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

    [Fact]
    public void TestCanSerializeByIdsParametersWithMultipleSubscriptionIds()
    {
        UnsubscribeByIdsCommandParameters properties = new(["mySubscriptionId", "myOtherSubscriptionId"]);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("subscriptions"));
        JToken? subscriptionsToken = serialized["subscriptions"];
        Assert.NotNull(subscriptionsToken);
        Assert.Equal(JTokenType.Array, subscriptionsToken.Type);
        Assert.Equal(2, subscriptionsToken.Count());
        Assert.Equal("mySubscriptionId", subscriptionsToken[0]!.Value<string>());
        Assert.Equal("myOtherSubscriptionId", subscriptionsToken[1]!.Value<string>());
    }

    [Fact]
    public void TestConstructingByIdsParametersWithEmptySubscriptionIdListThrows()
    {
        // The specification requires an unsubscription to name at least one subscription
        // ID; an empty subscription ID list cannot be meaningful under any revision of
        // the specification.
        Assert.Contains("At least one subscription ID must be specified.", Assert.Throws<ArgumentException>(() => new UnsubscribeByIdsCommandParameters([])).Message);
    }
}
