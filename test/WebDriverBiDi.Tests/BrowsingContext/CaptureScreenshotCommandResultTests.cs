namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class CaptureScreenshotCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "data": "some screenshot data"
                      }
                      """;
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json);
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
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json);
        Assert.NotNull(result);
        CaptureScreenshotCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json));
    }
}
