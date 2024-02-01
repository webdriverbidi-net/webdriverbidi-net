namespace WebDriverBiDi.Storage;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetCookieCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""partition"": { ""userContext"": ""myUserContext"", ""sourceOrigin"": ""mySourceOrigin"", ""extraPropertyName"": ""extraPropertyValue"" } }";
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Partition, Is.Not.Null);
            Assert.That(result.Partition.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result.Partition.SourceOrigin, Is.EqualTo("mySourceOrigin"));
            Assert.That(result.Partition.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(result.Partition.AdditionalData, Contains.Key("extraPropertyName"));
            Assert.That(result.Partition.AdditionalData["extraPropertyName"], Is.EqualTo("extraPropertyValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithMissingData()
    {
        string json = @"{ ""partition"": {} }";
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Partition, Is.Not.Null);
            Assert.That(result.Partition.UserContextId, Is.Null);
            Assert.That(result.Partition.SourceOrigin, Is.Null);
            Assert.That(result.Partition.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializingWithMissingPartition()
    {
        string json = @"{ }";
        Assert.That(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidPartitionDataTypeThrows()
    {
        string json = @"{ ""partition"": ""invalidPartitionType"" }";
        Assert.That(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
