namespace WebDriverBiDi.Session;

using System.Text.Json;

[TestFixture]
public class SubscribeCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "subscription": "mySubscription"
                      }
                      """;
        SubscribeCommandResult? result = JsonSerializer.Deserialize<SubscribeCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SubscriptionId, Is.EqualTo("mySubscription"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "subscription": "mySubscription"
                      }
                      """;
        SubscribeCommandResult? result = JsonSerializer.Deserialize<SubscribeCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        SubscribeCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializationWithMissingSubscriptionThrows()
    {
        string json = """
                      {
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<SubscribeCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializationWithIncorrectSubscriptionTypeThrows()
    {
        string json = """
                      {
                        "subscription": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<SubscribeCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
