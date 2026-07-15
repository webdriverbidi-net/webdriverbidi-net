namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class GetUserContextsCommandResultTests
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
                        "userContexts": [
                          {
                            "userContext": "default"
                          },
                          {
                            "userContext": "myUserContext"
                          }
                        ]
                      }
                      """;
        GetUserContextsCommandResult? result = JsonSerializer.Deserialize<GetUserContextsCommandResult>(json, this.options);
        Assert.NotNull(result);

        Assert.Equal(2, result.UserContexts.Count);
        Assert.Equal("default", result.UserContexts[0].UserContextId);
        Assert.Equal("myUserContext", result.UserContexts[1].UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContexts": [
                          {
                            "userContext": "default"
                          },
                          {
                            "userContext": "myUserContext"
                          }
                        ]
                      }
                      """;
        GetUserContextsCommandResult? result = JsonSerializer.Deserialize<GetUserContextsCommandResult>(json, this.options);
        Assert.NotNull(result);
        GetUserContextsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingUserContextsThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidUserContextsTypeThrows()
    {
        string json = """
                      {
                        "userContexts": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullUserContextsThrows()
    {
        string json = """
                      {
                        "userContexts": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, this.options));
    }
}
