namespace WebDriverBidi.Network;

using Newtonsoft.Json;

[TestFixture]
public class HeaderTests
{
    [Test]
    public void TestCanDeserializeHeader()
    {
        string json = @"{ ""name"": ""headerName"", ""value"": ""headerValue"" }";
        Header? header = JsonConvert.DeserializeObject<Header>(json);
        Assert.That(header, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(header!.Name, Is.EqualTo("headerName"));
            Assert.That(header!.Value, Is.EqualTo("headerValue"));
            Assert.That(header.BinaryValue, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeHeaderWithBinaryValue()
    {
        string json = @"{ ""name"": ""headerName"", ""binaryValue"": [ 65, 66, 67 ] }";
        Header? header = JsonConvert.DeserializeObject<Header>(json);
        Assert.That(header, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(header!.Name, Is.EqualTo("headerName"));
            Assert.That(header!.Value, Is.Null);
            Assert.That(header.BinaryValue, Is.EqualTo(new byte[] { 65, 66, 67 }));
        });
    }

    [Test]
    public void TestDeserializingWithMissingNameThrows()
    {
        string json = @"{ ""value"": ""headerValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Header>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'name' not found in JSON"));
    }
}