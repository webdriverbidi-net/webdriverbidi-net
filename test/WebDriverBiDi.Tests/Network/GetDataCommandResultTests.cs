namespace WebDriverBiDi.Network;

using System.Text.Json;

public class GetDataCommandResultTests
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
                        "bytes": {
                          "type": "string",
                          "value": "myNetworkData"
                        }
                      }
                      """;
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json, this.options);
        Assert.NotNull(result);
        Assert.Equal(BytesValueType.String, result.Bytes.Type);
        Assert.Equal("myNetworkData", result.Bytes.Value);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "bytes": {
                          "type": "string",
                          "value": "myNetworkData"
                        }
                      }
                      """;
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json, this.options);
        Assert.NotNull(result);
        GetDataCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetDataCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "bytes": "invalidValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetDataCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullDataThrows()
    {
        string json = """
                      {
                        "bytes": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetDataCommandResult>(json, this.options));
    }
}
