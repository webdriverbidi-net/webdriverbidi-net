namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class CreateCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("myContextId", result.BrowsingContextId);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "userContext": "myUserContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("myContextId", result.BrowsingContextId);
        Assert.Equal("myUserContextId", result.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.NotNull(result);
        CreateCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateCommandResult>(json));
    }
}
