namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class ResponseContentTests
{
    [Test]
    public void TestCanDeserializeResponseContent()
    {
        string json = @"{ ""size"": 300 }";
        ResponseContent? responseContent = JsonSerializer.Deserialize<ResponseContent>(json);
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
        Assert.That(() => JsonSerializer.Deserialize<ResponseContent>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: size"));
    }
}