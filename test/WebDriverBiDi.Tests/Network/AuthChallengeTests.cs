namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class AuthChallengeTests
{
    [Test]
    public void TestCanDeserializeAuthChallenge()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": "example.com"
                      }
                      """;
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json);
        Assert.That(challenge, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(challenge.Scheme, Is.EqualTo("basic"));
            Assert.That(challenge.Realm, Is.EqualTo("example.com"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": "example.com"
                      }
                      """;
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json);
        Assert.That(challenge, Is.Not.Null);
        AuthChallenge copy = challenge with { };
        Assert.That(copy, Is.EqualTo(challenge));
    }

    [Test]
    public void TestDeserializingWithMissingSchemeThrows()
    {
        string json = """
                      {
                        "realm": "example.com"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidSchemeTypeThrows()
    {
        string json = """
                      {
                        "scheme": {},
                        "realm": "example.com"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingRealmThrows()
    {
        string json = """
                      {
                        "scheme": "basic"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AuthChallenge>(json), Throws.InstanceOf<JsonException>());
    }
}
