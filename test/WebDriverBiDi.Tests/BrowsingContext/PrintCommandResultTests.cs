namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class PrintCommandResultTests
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
                        "data": "some print data"
                      }
                      """;
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json, this.options);
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
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json, this.options);
        Assert.NotNull(result);
        PrintCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrintCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrintCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullDataThrows()
    {
        string json = """
                      {
                        "data": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrintCommandResult>(json, this.options));
    }
}
