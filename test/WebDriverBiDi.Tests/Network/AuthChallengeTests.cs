namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class AuthChallengeTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeAuthChallenge()
    {
        string json = @"{ ""scheme"": ""basic"", ""realm"": ""example.com"" }";
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json, deserializationOptions);
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
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidSchemeTypeThrows()
    {
        string json = @"{ ""scheme"": {}, ""realm"": ""example.com"" }";
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingRealmThrows()
    {
        string json = @"{ ""scheme"": ""basic"" }";
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidRealmTypeThrows()
    {
        string json = @"{ ""scheme"": ""basic"", ""realm"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}