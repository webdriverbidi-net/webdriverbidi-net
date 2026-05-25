namespace WebDriverBiDi.Storage;

using System.Text.Json;

public class PartitionKeyTests
{
    [Fact]
    public void TestCanDeserializePartitionKey()
    {
        string json = "{}";
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.NotNull(result);

        Assert.Null(result.UserContextId);
        Assert.Null(result.SourceOrigin);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializePartitionKeyWithUserContext()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.NotNull(result);

        Assert.Equal("myUserContext", result.UserContextId);
        Assert.Null(result.SourceOrigin);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializePartitionKeyWithSourceOrigin()
    {
        string json = """
                      {
                        "sourceOrigin": "mySourceOrigin"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.NotNull(result);

        Assert.Null(result.UserContextId);
        Assert.Equal("mySourceOrigin", result.SourceOrigin);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializePartitionKeyWithAdditionalData()
    {
        string json = """
                      {
                        "extraData": "myExtraData"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.NotNull(result);

        Assert.Null(result.UserContextId);
        Assert.Null(result.SourceOrigin);
        Assert.Single(result.AdditionalData);
        Assert.True(result.AdditionalData.ContainsKey("extraData"));
        Assert.NotNull(result.AdditionalData["extraData"]);
        object? extraData = result.AdditionalData["extraData"];
        Assert.NotNull(extraData);
        Assert.Equal(typeof(string), extraData.GetType());
        Assert.Equal("myExtraData", extraData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = "{}";
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.NotNull(result);
        PartitionKey copy = result with { };
        Assert.Equal(result, copy);
    }
}
