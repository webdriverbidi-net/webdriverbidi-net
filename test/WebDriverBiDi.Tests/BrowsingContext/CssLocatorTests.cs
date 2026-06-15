namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CssLocatorTests
{
    [Fact]
    public void TestCanSerializeUsingBaseType()
    {
        Locator locator = new CssLocator("locator");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
    }

    [Fact]
    public void TestCanSerializeCssLocator()
    {
        CssLocator value = new(".selector");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("css", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal(".selector", valueProperty.Value<string>());
    }
}
