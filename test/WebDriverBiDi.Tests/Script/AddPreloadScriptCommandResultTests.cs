namespace WebDriverBiDi.Script;

using System.Text.Json;

public class AddPreloadScriptCommandResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Equal("myLoadScript", result.PreloadScriptId);
    }

    [Fact]
    public void TestDeserializingWithMissingScriptThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidScriptDataTypeThrows()
    {
        string json = """
                      {
                        "script": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullScriptThrows()
    {
        string json = """
                      {
                        "script": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, this.options));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, this.options);
        Assert.NotNull(result);
        AddPreloadScriptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
