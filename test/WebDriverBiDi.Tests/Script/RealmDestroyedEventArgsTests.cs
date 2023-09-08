namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class RealmDestroyedEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""realm"": ""myRealmId"" }";
        RealmDestroyedEventArgs? eventArgs = JsonConvert.DeserializeObject<RealmDestroyedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.RealmId, Is.EqualTo("myRealmId"));
    }

    [Test]
    public void TestDeserializeWithMissingRealmValueThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<RealmDestroyedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidRealmValueThrows()
    {
        string json = @"{ ""realm"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmDestroyedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
