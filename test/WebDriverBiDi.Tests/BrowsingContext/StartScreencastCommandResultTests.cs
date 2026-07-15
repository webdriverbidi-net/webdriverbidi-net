namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class SrartScrenncastCommandResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public async Task TestCanDeserialize()
    {
        string json = """
                      {
                        "screencast": "myScreencastId",
                        "path": "path/to/screencast/file"
                      }
                      """;
        StartScreencastCommandResult? result = JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options);
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
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
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
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
    }

    [Fact]
    public async Task TestDeserializingWithNullScreencastIdThrows()
    {
        string json = """
                      {
                        "screencast": null,
                        "path": "path/to/screencast/file"
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
    }

    [Fact]
    public async Task TestDeserializingWithMissingPathdThrows()
    {
        string json = """
                      {
                        "screencast": "myScreencastId"
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
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
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
    }

    [Fact]
    public async Task TestDeserializingWithNullPathThrows()
    {
        string json = """
                      {
                        "screencast": "myScreencastId",
                        "path": null
                      }
                      """;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options));
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
        StartScreencastCommandResult? result = JsonSerializer.Deserialize<StartScreencastCommandResult>(json, this.options);
        Assert.NotNull(result);
        StartScreencastCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
