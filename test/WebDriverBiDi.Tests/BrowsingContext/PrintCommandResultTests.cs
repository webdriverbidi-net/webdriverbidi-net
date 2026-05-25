namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class PrintCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "data": "some print data"
                      }
                      """;
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("some print data", result.Data);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "data": "some print data"
                      }
                      """;
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json);
        Assert.NotNull(result);
        PrintCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrintCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrintCommandResult>(json));
    }
}
