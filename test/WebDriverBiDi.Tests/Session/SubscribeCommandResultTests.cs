namespace WebDriverBiDi.Session;

using System.Text.Json;

public class SubscribeCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "subscription": "mySubscription"
                      }
                      """;
        SubscribeCommandResult? result = JsonSerializer.Deserialize<SubscribeCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("mySubscription", result.SubscriptionId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "subscription": "mySubscription"
                      }
                      """;
        SubscribeCommandResult? result = JsonSerializer.Deserialize<SubscribeCommandResult>(json);
        Assert.NotNull(result);
        SubscribeCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializationWithMissingSubscriptionThrows()
    {
        string json = """
                      {
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SubscribeCommandResult>(json));
    }

    [Fact]
    public void TestDeserializationWithIncorrectSubscriptionTypeThrows()
    {
        string json = """
                      {
                        "subscription": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SubscribeCommandResult>(json));
    }
}
