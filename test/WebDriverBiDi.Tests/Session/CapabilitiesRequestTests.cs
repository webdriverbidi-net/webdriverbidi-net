namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CapabilitiesRequestTests
{
    [Fact]
    public void TestCanSerialize()
    {
        CapabilitiesRequest capabilities = new();
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Empty(result);
    }

    [Fact]
    public void TestCanSerializeWithZeroLengthFirstMatch()
    {
        // Distinct from TestCanSerialize: the list has been populated and emptied again, so this
        // pins that the shim reports the current count rather than whether the list was ever used.
        CapabilitiesRequest capabilities = new();
        capabilities.FirstMatch.Add(new CapabilityRequest());
        capabilities.FirstMatch.Clear();
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Empty(result);
    }

    [Fact]
    public void TestCanSerializeWithFirstMatchEntries()
    {
        CapabilitiesRequest capabilities = new();
        capabilities.FirstMatch.Add(new CapabilityRequest());
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);
        Assert.True(result.ContainsKey("firstMatch"));
        JToken? firstMatchToken = result["firstMatch"];
        Assert.NotNull(firstMatchToken);
        Assert.Equal(JTokenType.Array, firstMatchToken.Type);
        JArray? firstMatchArray = firstMatchToken as JArray;
        Assert.NotNull(firstMatchArray);
        JToken capabilityToken = Assert.Single(firstMatchArray);
        Assert.Equal(JTokenType.Object, capabilityToken.Type);
        JObject? capabilityObject = capabilityToken as JObject;
        Assert.NotNull(capabilityObject);
        Assert.Empty(capabilityObject);
    }
}
