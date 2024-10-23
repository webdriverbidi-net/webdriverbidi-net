namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class HistoryUpdatedEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com"
                      }
                      """;
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = """
                      {
                        "url": "http://example.com"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = """
                      {
                        "context": {},
                        "url": "http://example.com"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidAcceptedValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
