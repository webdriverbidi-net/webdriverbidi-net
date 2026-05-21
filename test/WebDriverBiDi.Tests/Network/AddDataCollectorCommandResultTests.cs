namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AddDataCollectorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json);
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
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json);
        Assert.NotNull(result);
        AddDataCollectorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "collector": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json));
    }
}
