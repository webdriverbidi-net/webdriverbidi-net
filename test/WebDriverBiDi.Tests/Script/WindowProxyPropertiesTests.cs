namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class WindowProxyPropertiesTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        WindowProxyProperties? windowProxyProperties = JsonSerializer.Deserialize<WindowProxyProperties>(json);
        Assert.That(windowProxyProperties, Is.Not.Null);
        Assert.That(windowProxyProperties!.Context, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestDeserializeWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<WindowProxyProperties>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingContextThrows()
    {
        string json = @"{ ""nodeType"": 1 }";
        Assert.That(() => JsonSerializer.Deserialize<WindowProxyProperties>(json), Throws.InstanceOf<JsonException>());
    }
}