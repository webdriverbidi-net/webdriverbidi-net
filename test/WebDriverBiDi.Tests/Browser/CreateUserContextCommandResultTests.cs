namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class CreateUserContextCommandResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options);
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
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options);
        Assert.NotNull(result);
        CreateUserContextCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingUserContextThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidUserContextDataTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullUserContext()
    {
        string json = """
                      {
                        "userContext": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }
}
