namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class CreateCommandResultTests
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
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json, this.options);
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
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json, this.options);
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
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json, this.options);
        Assert.NotNull(result);
        CreateCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullContextThrows()
    {
        string json = """
                      {
                        "context": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateCommandResult>(json, this.options));
    }
}
