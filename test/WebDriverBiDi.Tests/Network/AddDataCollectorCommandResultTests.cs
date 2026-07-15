namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AddDataCollectorCommandResultTests
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
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Equal("myCollectorId", result.CollectorId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, this.options);
        Assert.NotNull(result);
        AddDataCollectorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingCollectorThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidCollectorDataTypeThrows()
    {
        string json = """
                      {
                        "collector": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullCollectorThrows()
    {
        string json = """
                      {
                        "collector": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, this.options));
    }
}
