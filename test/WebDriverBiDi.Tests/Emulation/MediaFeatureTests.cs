namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class MediaFeatureTests
{
    [Fact]
    public async Task CanSerializeMediaFeatures()
    {
        MediaFeature mediaFeature = new("featureName", "featureValue");
        string json = JsonSerializer.Serialize(mediaFeature);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("featureName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("featureValue", value.Value<string>());
    }

    [Fact]
    public async Task ConstructingWithNullNameThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MediaFeature(null!, "featureValue"));
    }

    [Fact]
    public async Task ConstructingWithNullValueThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MediaFeature("FeatureName", null!));
    }
}
