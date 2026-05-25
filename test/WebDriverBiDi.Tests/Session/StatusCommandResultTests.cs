namespace WebDriverBiDi.Session;

using System.Text.Json;

public class StatusCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "ready": true,
                        "message": "myMessage"
                      }
                      """;
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json);
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
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingMessageThrows()
    {
        string json = """
                      {
                        "ready": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StatusCommandResult>(json));
    }
}
