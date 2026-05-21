namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class UserContextInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContext": "default"
                      }
                      """;
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json);
        Assert.NotNull(result);
        Assert.Equal("default", result.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContext": "default"
                      }
                      """;
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json);
        Assert.NotNull(result);
        UserContextInfo copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingUserContextThrows()
    {
        string json = """
                      {
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserContextInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithIncorrectUserContextTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserContextInfo>(json));
    }
}
