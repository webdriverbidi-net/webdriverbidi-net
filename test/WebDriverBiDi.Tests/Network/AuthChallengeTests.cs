namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AuthChallengeTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserializeAuthChallenge()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": "example.com"
                      }
                      """;
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json, this.options);
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
        AuthChallenge? challenge = JsonSerializer.Deserialize<AuthChallenge>(json, this.options);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullSchemeThrows()
    {
        string json = """
                      {
                        "scheme": null,
                        "realm": "example.com"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingRealmThrows()
    {
        string json = """
                      {
                        "scheme": "basic"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullRealmThrows()
    {
        string json = """
                      {
                        "scheme": "basic",
                        "realm": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AuthChallenge>(json, this.options));
    }
}
