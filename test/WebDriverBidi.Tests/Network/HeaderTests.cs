namespace WebDriverBidi.Network;

using Newtonsoft.Json;

[TestFixture]
public class HeaderTests
{
    [Test]
    public void TestCanDeserializeHeader()
    {
        string json = @"{ ""name"": ""headerName"", ""value"": { ""type"": ""string"", ""value"": ""headerValue"" } }";
        Header? header = JsonConvert.DeserializeObject<Header>(json);
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
        Header? header = JsonConvert.DeserializeObject<Header>(json);
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
        Assert.That(() => JsonConvert.DeserializeObject<Header>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'name' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingValueThrows()
    {
        string json = @"{ ""name"": ""headerName"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Header>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'value' not found in JSON"));
    }
}