namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ContextLocatorTests
{
    [Fact]
    public void TestCanSerializeUsingBaseType()
    {
        Locator locator = new ContextLocator("myContext");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
    }

    [Fact]
    public void TestCanSerializeContextLocator()
    {
        ContextLocator value = new("myContext");
        Assert.Equal("myContext", value.BrowsingContextId);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("context", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);

        JObject? contextValue = valueProperty as JObject;
        Assert.NotNull(contextValue);
        Assert.Single(contextValue);
        Assert.True(contextValue.ContainsKey("context"));

        JToken? context = contextValue["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());
    }
}
