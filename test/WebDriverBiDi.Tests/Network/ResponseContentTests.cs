namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ResponseContentTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeResponseContent()
    {
        string json = @"{ ""size"": 300 }";
        ResponseContent? responseContent = JsonSerializer.Deserialize<ResponseContent>(json, deserializationOptions);
        Assert.That(responseContent, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(responseContent!.Size, Is.EqualTo(300));
        });
    }

    [Test]
    public void TestDeserializeWithMissingSizeThrows()
    {
        string json = @"{ }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseContent>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: size"));
    }
}