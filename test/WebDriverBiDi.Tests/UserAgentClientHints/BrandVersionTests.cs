namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class BrandVersionTests
{
    [Fact]
    public void TestCanSerialize()
    {
        BrandVersion brandVersion = new("myBrand", "myVersion");
        string json = JsonSerializer.Serialize(brandVersion);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("brand"));
        JToken? brand = serialized["brand"];
        Assert.NotNull(brand);
        Assert.Equal(JTokenType.String, brand.Type);
        Assert.Equal("myBrand", brand.Value<string>());

        Assert.True(serialized.ContainsKey("version"));
        JToken? version = serialized["version"];
        Assert.NotNull(version);
        Assert.Equal(JTokenType.String, version.Type);
        Assert.Equal("myVersion", version.Value<string>());
    }
}
