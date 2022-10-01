namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[TestFixture]
public class StatusCommandResultPropertiesTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""ready"": true, ""message"": ""myMessage"" }";
        StatusCommandResult? result = JsonConvert.DeserializeObject<StatusCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsReady, Is.EqualTo(true));
        Assert.That(result.Message, Is.EqualTo("myMessage"));
    }

    [Test]
    public void TestDeserializingWithMissingReadyThrows()
    {
        string json = @"{ ""message"": ""myMessage"" }";
        Assert.That(() => JsonConvert.DeserializeObject<StatusCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidReadyTypeThrows()
    {
        string json = @"{ ""ready"": ""invalid value"", ""message"": ""myMessage"" }";
        Assert.That(() => JsonConvert.DeserializeObject<StatusCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingMessageThrows()
    {
        string json = @"{ ""ready"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<StatusCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidMessageTypeThrows()
    {
        string json = @"{ ""ready"": true, ""message"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<StatusCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}