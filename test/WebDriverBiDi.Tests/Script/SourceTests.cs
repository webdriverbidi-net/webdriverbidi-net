namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class SourceTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""realm"": ""realmId"" }";
        Source? source = JsonConvert.DeserializeObject<Source>(json);
        Assert.That(source, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(source!.RealmId, Is.EqualTo("realmId"));
            Assert.That(source.Context, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithMissingRealmThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<Source>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidRealmTypeThrows()
    {
        string json = @"{ ""realm"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<Source>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalContext()
    {
        string json = @"{ ""realm"": ""realmId"", ""context"": ""contextId"" }";
        Source? source = JsonConvert.DeserializeObject<Source>(json);
        Assert.That(source, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(source!.RealmId, Is.EqualTo("realmId"));
            Assert.That(source.Context, Is.Not.Null);
            Assert.That(source.Context, Is.EqualTo("contextId"));
        });
    }

    [Test]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = @"{ ""realm"": ""realmId"", ""context"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<Source>(json), Throws.InstanceOf<JsonReaderException>());
    }
}