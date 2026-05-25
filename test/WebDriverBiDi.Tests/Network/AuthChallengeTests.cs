namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AuthChallengeTests
{
    [Fact]
    public void TestCanDeserializeAuthChallenge()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": "example.com"
                      }
                      """;
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json);
        Assert.NotNull(challenge);

        Assert.Equal("basic", challenge.Scheme);
        Assert.Equal("example.com", challenge.Realm);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": "example.com"
                      }
                      """;
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json);
        Assert.NotNull(challenge);
        AuthChallenge copy = challenge with { };
        Assert.Equal(challenge, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingSchemeThrows()
    {
        string json = """
                      {
                        "realm": "example.com"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidSchemeTypeThrows()
    {
        string json = """
                      {
                        "scheme": {},
                        "realm": "example.com"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingRealmThrows()
    {
        string json = """
                      {
                        "scheme": "basic"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json));
    }
}
