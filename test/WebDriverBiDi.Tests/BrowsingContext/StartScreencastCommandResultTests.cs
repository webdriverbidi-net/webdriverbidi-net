namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class SrartScrenncastCommandResultTests
{
    [Fact]
    public async Task TestCanDeserialize()
    {
        string json = """
                      {
                        "screencast": "myScreencastId",
                        "path": "path/to/screencast/file"
                      }
                      """;
        StartScreencastCommandResult? result = JsonSerializer.Deserialize<StartScreencastCommandResult>(json);
        Assert.NotNull(result);

        Assert.Equal("myScreencastId", result.ScreencastId);
        Assert.Equal("path/to/screencast/file", result.Path);
    }

    [Fact]
    public async Task TestDeserializingWithMissingScreencastIdThrows()
    {
        string json = """
                      {
                        "path": "path/to/screencast/file"
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json));        
    }

    [Fact]
    public async Task TestDeserializingWithInvalidScreencastIdTypeThrows()
    {
        string json = """
                      {
                        "screencast": {},
                        "path": "path/to/screencast/file"
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json));        
    }

    [Fact]
    public async Task TestDeserializingWithMissingPathdThrows()
    {
        string json = """
                      {
                        "screencast": "myScreencastId"
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json));        
    }

    [Fact]
    public async Task TestDeserializingWithInvalidPathTypeThrows()
    {
        string json = """
                      {
                        "screencast": "myScreencastId",
                        "path": {}
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json));        
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "screencast": "myScreencastId",
                        "path": "path/to/screencast/file"
                      }
                      """;
        StartScreencastCommandResult? result = JsonSerializer.Deserialize<StartScreencastCommandResult>(json);
        Assert.NotNull(result);
        StartScreencastCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
