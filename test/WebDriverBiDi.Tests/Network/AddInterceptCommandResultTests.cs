namespace WebDriverBiDi.Network;

using System.Text.Json;

public class AddInterceptCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "intercept": "myInterceptId"
                      }
                      """;
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json);
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
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json);
        Assert.NotNull(result);
        AddInterceptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "intercept": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json));
    }
}
