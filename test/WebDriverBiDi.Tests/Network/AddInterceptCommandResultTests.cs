namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AddInterceptCommandResultTests
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
                        "intercept": "myInterceptId"
                      }
                      """;
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Equal("myInterceptId", result.InterceptId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "intercept": "myInterceptId"
                      }
                      """;
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json, this.options);
        Assert.NotNull(result);
        AddInterceptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingInterceptThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidInterceptDataTypeThrows()
    {
        string json = """
                      {
                        "intercept": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullnterceptThrows()
    {
        string json = """
                      {
                        "intercept": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json, this.options));
    }
}
