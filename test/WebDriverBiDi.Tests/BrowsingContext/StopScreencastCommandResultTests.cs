namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class StopScreencastCommandResultTests
{
    [Fact]
    public async Task TestCanDeserialize()
    {
        string json = """
                      {
                        "path": "path/to/screencast/file"
                      }
                      """;
        StopScreencastCommandResult? result = JsonSerializer.Deserialize<StopScreencastCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("path/to/screencast/file", result.Path);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task TestCanDeserializeWithError()
    {
        string json = """
                      {
                        "path": "path/to/screencast/file",
                        "error": "could not write to file"
                      }
                      """;
        StopScreencastCommandResult? result = JsonSerializer.Deserialize<StopScreencastCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("path/to/screencast/file", result.Path);
        Assert.Equal("could not write to file", result.Error);
    }

    [Fact]
    public async Task TestDeserializingWithMissingPathThrows()
    {
        string json = """
                      {
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StopScreencastCommandResult>(json));
    }

    [Fact]
    public async Task TestDeserializingWithInvalidPathTypeThrows()
    {
        string json = """
                      {
                        "path": {}
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StopScreencastCommandResult>(json));
    }

    [Fact]
    public async Task TestDeserializingWithInvalidErrorTypeThrows()
    {
        string json = """
                      {
                        "path": "path/to/screencast/file",
                        "error": {}
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StopScreencastCommandResult>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "path": "path/to/screencast/file",
                        "error": "could not write to file"
                      }
                      """;
        StopScreencastCommandResult? result = JsonSerializer.Deserialize<StopScreencastCommandResult>(json);
        Assert.NotNull(result);
        StopScreencastCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
