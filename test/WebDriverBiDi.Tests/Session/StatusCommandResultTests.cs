namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class StatusCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""ready"": true, ""message"": ""myMessage"" }";
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.IsReady, Is.EqualTo(true));
            Assert.That(result.Message, Is.EqualTo("myMessage"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingReadyThrows()
    {
        string json = @"{ ""message"": ""myMessage"" }";
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidReadyTypeThrows()
    {
        string json = @"{ ""ready"": ""invalid value"", ""message"": ""myMessage"" }";
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingMessageThrows()
    {
        string json = @"{ ""ready"": true }";
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidMessageTypeThrows()
    {
        string json = @"{ ""ready"": true, ""message"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}