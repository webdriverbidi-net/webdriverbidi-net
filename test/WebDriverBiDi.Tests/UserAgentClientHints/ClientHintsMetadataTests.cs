namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ClientHintsMetadataTests
{
    [Test]
    public void TestCanSerialize()
    {
        ClientHintsMetadata metadata = new();
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Not.Null);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
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
        Assert.That(serialized, Has.Count.EqualTo(10));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("brands"));
            Assert.That(serialized["brands"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? brandArray = serialized["brands"]!.Value<JArray>();
            Assert.That(brandArray, Is.Not.Null);
            Assert.That(brandArray, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("fullVersionList"));
            Assert.That(serialized["fullVersionList"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? fullVersionListArray = serialized["fullVersionList"]!.Value<JArray>();
            Assert.That(fullVersionListArray, Is.Not.Null);
            Assert.That(fullVersionListArray, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("platform"));
            Assert.That(serialized["platform"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["platform"]!.Value<string>(), Is.EqualTo("myPlatform"));
            Assert.That(serialized, Contains.Key("platformVersion"));
            Assert.That(serialized["platformVersion"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["platformVersion"]!.Value<string>(), Is.EqualTo("myPlatformVersion"));
            Assert.That(serialized, Contains.Key("architecture"));
            Assert.That(serialized["architecture"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["architecture"]!.Value<string>(), Is.EqualTo("myArchitecture"));
            Assert.That(serialized, Contains.Key("model"));
            Assert.That(serialized["model"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["model"]!.Value<string>(), Is.EqualTo("myModel"));
            Assert.That(serialized, Contains.Key("mobile"));
            Assert.That(serialized["mobile"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["mobile"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("bitness"));
            Assert.That(serialized["bitness"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["bitness"]!.Value<string>(), Is.EqualTo("myBitness"));
            Assert.That(serialized, Contains.Key("wow64"));
            Assert.That(serialized["wow64"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["wow64"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("formFactors"));
            Assert.That(serialized["formFactors"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? formFactorsArray = serialized["formFactors"]!.Value<JArray>();
            Assert.That(formFactorsArray, Is.Not.Null);
            Assert.That(formFactorsArray, Has.Count.EqualTo(1));
            Assert.That(formFactorsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(formFactorsArray![0].Value<string>(), Is.EqualTo("myFormFactor"));
        }
    }

    [Test]
    public void TestCanSerializeWithOptionalBooleanTrueProperties()
    {
        ClientHintsMetadata metadata = new()
        {
            Mobile = true,
            Wow64 = true,
        };
        string json = JsonSerializer.Serialize(metadata);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized["mobile"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["mobile"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("wow64"));
            Assert.That(serialized["wow64"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["wow64"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
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
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("brands"));
            Assert.That(serialized["brands"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? brandArray = serialized["brands"]!.Value<JArray>();
            Assert.That(brandArray, Is.Not.Null);
            Assert.That(brandArray, Is.Empty);
            Assert.That(serialized, Contains.Key("fullVersionList"));
            Assert.That(serialized["fullVersionList"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? fullVersionListArray = serialized["fullVersionList"]!.Value<JArray>();
            Assert.That(fullVersionListArray, Is.Not.Null);
            Assert.That(fullVersionListArray, Is.Empty);
            Assert.That(serialized, Contains.Key("formFactors"));
            Assert.That(serialized["formFactors"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? formFactorsArray = serialized["formFactors"]!.Value<JArray>();
            Assert.That(formFactorsArray, Is.Not.Null);
            Assert.That(formFactorsArray, Is.Empty);
        }
    }
}
