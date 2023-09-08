namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class AuthChallengeTests
{
    [Test]
    public void TestCanDeserializeAuthChallenge()
    {
        string json = @"{ ""scheme"": ""basic"", ""realm"": ""example.com"" }";
        AuthChallenge? challenge = JsonConvert.DeserializeObject<AuthChallenge>(json);
        Assert.That(challenge, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(challenge!.Scheme, Is.EqualTo("basic"));
            Assert.That(challenge.Realm, Is.EqualTo("example.com"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingSchemeThrows()
    {
        string json = @"{ ""realm"": ""example.com"" }";
        Assert.That(() => JsonConvert.DeserializeObject<AuthChallenge>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidSchemeTypeThrows()
    {
        string json = @"{ ""scheme"": {}, ""realm"": ""example.com"" }";
        Assert.That(() => JsonConvert.DeserializeObject<AuthChallenge>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingRealmThrows()
    {
        string json = @"{ ""scheme"": ""basic"" }";
        Assert.That(() => JsonConvert.DeserializeObject<AuthChallenge>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidRealmTypeThrows()
    {
        string json = @"{ ""scheme"": ""basic"", ""realm"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<AuthChallenge>(json), Throws.InstanceOf<JsonReaderException>());
    }
}