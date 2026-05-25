namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class CreateUserContextCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("myUserContext", result.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json);
        Assert.NotNull(result);
        CreateUserContextCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json));
    }
}
