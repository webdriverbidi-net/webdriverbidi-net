namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoteReferenceJsonConverterTests
{
    [Test]
    public void TestCanSerializeRemoteReference()
    {
        RemoteReference reference = new("myHandle");
        string json = JsonConvert.SerializeObject(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject.ContainsKey("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
        });
    }

    [Test]
    public void TestCanSerializeRemoteReferenceWithAdditionalProperties()
    {
        RemoteReference reference = new("myHandle");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonConvert.SerializeObject(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject.ContainsKey("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
            Assert.That(referenceObject.ContainsKey("myPropertyName"));
            Assert.That(referenceObject["myPropertyName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(referenceObject["myPropertyName"]!.Value<string>(), Is.EqualTo("myValue"));
        });
    }
}
