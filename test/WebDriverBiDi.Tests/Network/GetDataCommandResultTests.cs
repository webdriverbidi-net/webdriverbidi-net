namespace WebDriverBiDi.Network;

using System.Text.Json;

public class GetDataCommandResultTests
{
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
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json);
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
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json);
        Assert.NotNull(result);
        GetDataCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetDataCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "bytes": "invalidValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetDataCommandResult>(json));
    }
}
