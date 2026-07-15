namespace WebDriverBiDi.Session;

using System.Text.Json;

public class StatusCommandResultTests
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
                        "ready": true,
                        "message": "myMessage"
                      }
                      """;
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json, this.options);
        Assert.NotNull(result);

        Assert.True(result.IsReady);
        Assert.Equal("myMessage", result.Message);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "ready": true,
                        "message": "myMessage"
                      }
                      """;
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json, this.options);
        Assert.NotNull(result);
        StatusCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingReadyThrows()
    {
        string json = """
                      {
                        "message": "myMessage"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidReadyTypeThrows()
    {
        string json = """
                      {
                        "ready": "invalid value",
                        "message": "myMessage"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingMessageThrows()
    {
        string json = """
                      {
                        "ready": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidMessageTypeThrows()
    {
        string json = """
                      {
                        "ready": true,
                        "message": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullMessageThrows()
    {
        string json = """
                      {
                        "ready": true,
                        "message": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json, this.options));
    }
}
