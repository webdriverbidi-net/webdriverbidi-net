namespace WebDriverBiDi.Storage;

using System.Text.Json;

public class SetCookieCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "partitionKey": {
                          "userContext": "myUserContext",
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json);
        Assert.NotNull(result);

        Assert.NotNull(result.PartitionKey);
        Assert.Equal("myUserContext", result.PartitionKey.UserContextId);
        Assert.Equal("mySourceOrigin", result.PartitionKey.SourceOrigin);
        Assert.Single(result.PartitionKey.AdditionalData);
        Assert.True(result.PartitionKey.AdditionalData.ContainsKey("extraPropertyName"));
        Assert.Equal("extraPropertyValue", result.PartitionKey.AdditionalData["extraPropertyName"]);
    }

    [Fact]
    public void TestCanDeserializeWithMissingData()
    {
        string json = """
                      {
                        "partitionKey": {}
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json);
        Assert.NotNull(result);

        Assert.NotNull(result.PartitionKey);
        Assert.Null(result.PartitionKey.UserContextId);
        Assert.Null(result.PartitionKey.SourceOrigin);
        Assert.Empty(result.PartitionKey.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializingWithMissingPartition()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "partitionKey": {
                          "userContext": "myUserContext",
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json);
        Assert.NotNull(result);
        SetCookieCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithInvalidPartitionDataTypeThrows()
    {
        string json = """
                      {
                        "partitionKey": "invalidPartitionType"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json));
    }
}
