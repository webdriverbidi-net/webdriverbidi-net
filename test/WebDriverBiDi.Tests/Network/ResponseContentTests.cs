namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class ResponseContentTests
{
    [Test]
    public void TestCanDeserializeResponseContent()
    {
        string json = @"{ ""size"": 300 }";
        ResponseContent? responseContent = JsonConvert.DeserializeObject<ResponseContent>(json);
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
        Assert.That(() => JsonConvert.DeserializeObject<ResponseContent>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'size' not found in JSON"));
    }
}
