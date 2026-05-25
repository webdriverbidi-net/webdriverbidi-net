namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ClientHintsMetadataTests
{
    [Fact]
    public void TestCanSerialize()
    {
        ClientHintsMetadata metadata = new();
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.NotNull(serialized);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeWithOptionalProperties()
    {
        ClientHintsMetadata metadata = new()
        {
            Brands = [new BrandVersion("myBrand", "myVersion")],
            FullVersionList = [new BrandVersion("myVersionListBrand", "myVersionListVersion")],
            Platform = "myPlatform",
            PlatformVersion = "myPlatformVersion",
            Architecture = "myArchitecture",
            Model = "myModel",
            Mobile = false,
            Bitness = "myBitness",
            Wow64 = false,
            FormFactors = ["myFormFactor"],
        };
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(10, serialized.Count);

        Assert.True(serialized.ContainsKey("brands"));
        JToken? brandsToken = serialized["brands"];
        Assert.NotNull(brandsToken);
        Assert.Equal(JTokenType.Array, brandsToken.Type);
        JArray? brandArray = brandsToken.Value<JArray>();
        Assert.NotNull(brandArray);
        Assert.Single(brandArray);

        Assert.True(serialized.ContainsKey("fullVersionList"));
        JToken? fullVersionListToken = serialized["fullVersionList"];
        Assert.NotNull(fullVersionListToken);
        Assert.Equal(JTokenType.Array, fullVersionListToken.Type);
        JArray? fullVersionListArray = fullVersionListToken.Value<JArray>();
        Assert.NotNull(fullVersionListArray);
        Assert.Single(fullVersionListArray);

        Assert.True(serialized.ContainsKey("platform"));
        JToken? platform = serialized["platform"];
        Assert.NotNull(platform);
        Assert.Equal(JTokenType.String, platform.Type);
        Assert.Equal("myPlatform", platform.Value<string>());

        Assert.True(serialized.ContainsKey("platformVersion"));
        JToken? platformVersion = serialized["platformVersion"];
        Assert.NotNull(platformVersion);
        Assert.Equal(JTokenType.String, platformVersion.Type);
        Assert.Equal("myPlatformVersion", platformVersion.Value<string>());

        Assert.True(serialized.ContainsKey("architecture"));
        JToken? architecture = serialized["architecture"];
        Assert.NotNull(architecture);
        Assert.Equal(JTokenType.String, architecture.Type);
        Assert.Equal("myArchitecture", architecture.Value<string>());

        Assert.True(serialized.ContainsKey("model"));
        JToken? model = serialized["model"];
        Assert.NotNull(model);
        Assert.Equal(JTokenType.String, model.Type);
        Assert.Equal("myModel", model.Value<string>());

        Assert.True(serialized.ContainsKey("mobile"));
        JToken? mobile = serialized["mobile"];
        Assert.NotNull(mobile);
        Assert.Equal(JTokenType.Boolean, mobile.Type);
        Assert.False(mobile.Value<bool>());

        Assert.True(serialized.ContainsKey("bitness"));
        JToken? bitness = serialized["bitness"];
        Assert.NotNull(bitness);
        Assert.Equal(JTokenType.String, bitness.Type);
        Assert.Equal("myBitness", bitness.Value<string>());

        Assert.True(serialized.ContainsKey("wow64"));
        JToken? wow64 = serialized["wow64"];
        Assert.NotNull(wow64);
        Assert.Equal(JTokenType.Boolean, wow64.Type);
        Assert.False(wow64.Value<bool>());

        Assert.True(serialized.ContainsKey("formFactors"));
        JToken? formFactorsToken = serialized["formFactors"];
        Assert.NotNull(formFactorsToken);
        Assert.Equal(JTokenType.Array, formFactorsToken.Type);
        JArray? formFactorsArray = formFactorsToken.Value<JArray>();
        Assert.NotNull(formFactorsArray);
        Assert.Single(formFactorsArray);
        Assert.Equal(JTokenType.String, formFactorsArray[0].Type);
        Assert.Equal("myFormFactor", formFactorsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOptionalBooleanTrueProperties()
    {
        ClientHintsMetadata metadata = new()
        {
            Mobile = true,
            Wow64 = true,
        };
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        JToken? mobile = serialized["mobile"];
        Assert.NotNull(mobile);
        Assert.Equal(JTokenType.Boolean, mobile.Type);
        Assert.True(mobile.Value<bool>());

        Assert.True(serialized.ContainsKey("wow64"));
        JToken? wow64 = serialized["wow64"];
        Assert.NotNull(wow64);
        Assert.Equal(JTokenType.Boolean, wow64.Type);
        Assert.True(wow64.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithEmptyListProperties()
    {
        ClientHintsMetadata metadata = new()
        {
            Brands = [],
            FullVersionList = [],
            FormFactors = [],
        };
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("brands"));
        JToken? brandsToken = serialized["brands"];
        Assert.NotNull(brandsToken);
        Assert.Equal(JTokenType.Array, brandsToken.Type);
        JArray? brandArray = brandsToken.Value<JArray>();
        Assert.NotNull(brandArray);
        Assert.Empty(brandArray);

        Assert.True(serialized.ContainsKey("fullVersionList"));
        JToken? fullVersionListToken = serialized["fullVersionList"];
        Assert.NotNull(fullVersionListToken);
        Assert.Equal(JTokenType.Array, fullVersionListToken.Type);
        JArray? fullVersionListArray = fullVersionListToken.Value<JArray>();
        Assert.NotNull(fullVersionListArray);
        Assert.Empty(fullVersionListArray);

        Assert.True(serialized.ContainsKey("formFactors"));
        JToken? formFactorsToken = serialized["formFactors"];
        Assert.NotNull(formFactorsToken);
        Assert.Equal(JTokenType.Array, formFactorsToken.Type);
        JArray? formFactorsArray = formFactorsToken.Value<JArray>();
        Assert.NotNull(formFactorsArray);
        Assert.Empty(formFactorsArray);
    }
}
