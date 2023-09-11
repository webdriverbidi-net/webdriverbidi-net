namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class HeaderTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeHeader()
    {
        string json = @"{ ""name"": ""headerName"", ""value"": { ""type"": ""string"", ""value"": ""headerValue"" } }";
        Header? header = JsonSerializer.Deserialize<Header>(json, deserializationOptions);
        Assert.That(header, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(header!.Name, Is.EqualTo("headerName"));
            Assert.That(header.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(header.Value.Value, Is.EqualTo("headerValue"));
        });
    }

    [Test]
    public void TestCanDeserializeHeaderWithBinaryValue()
    {
        byte[] byteArray = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(byteArray);
        string json = $@"{{ ""name"": ""headerName"", ""value"": {{ ""type"": ""base64"", ""value"": ""{base64Value}"" }} }}";
        Header? header = JsonSerializer.Deserialize<Header>(json, deserializationOptions);
        Assert.That(header, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(header!.Name, Is.EqualTo("headerName"));
            Assert.That(header.Value.Type, Is.EqualTo(BytesValueType.Base64));
            Assert.That(header.Value.Value, Is.EqualTo(base64Value));
        });
    }

    [Test]
    public void TestDeserializingWithMissingNameThrows()
    {
        string json = @"{ ""value"":  { ""type"": ""string"", ""value"": ""headerValue"" } }";
        Assert.That(() => JsonSerializer.Deserialize<Header>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: name"));
    }

    [Test]
    public void TestDeserializingWithMissingValueThrows()
    {
        string json = @"{ ""name"": ""headerName"" }";
        Assert.That(() => JsonSerializer.Deserialize<Header>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: value"));
    }
}