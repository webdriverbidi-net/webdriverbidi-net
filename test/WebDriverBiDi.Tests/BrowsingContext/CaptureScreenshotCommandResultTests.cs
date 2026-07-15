namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class CaptureScreenshotCommandResultTests
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
                        "data": "some screenshot data"
                      }
                      """;
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Equal("some screenshot data", result.Data);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "data": "some screenshot data"
                      }
                      """;
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, this.options);
        Assert.NotNull(result);
        CaptureScreenshotCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullDataThrows()
    {
        string json = """
                      {
                        "data": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, this.options));
    }
}
