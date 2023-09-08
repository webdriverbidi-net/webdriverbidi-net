namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class WindowProxyPropertiesTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        WindowProxyProperties? windowProxyProperties = JsonConvert.DeserializeObject<WindowProxyProperties>(json);
        Assert.That(windowProxyProperties, Is.Not.Null);
        Assert.That(windowProxyProperties!.Context, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestDeserializeWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<WindowProxyProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingContextThrows()
    {
        string json = @"{ ""nodeType"": 1 }";
        Assert.That(() => JsonConvert.DeserializeObject<WindowProxyProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}