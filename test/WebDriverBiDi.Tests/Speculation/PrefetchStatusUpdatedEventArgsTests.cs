namespace WebDriverBiDi.Speculation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class PrefetchStatusUpdatedEventArgsTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeWithPendingStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Pending));
        });
    }

    [Test]
    public void TestCanDeserializeWithReadyStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "ready"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Ready));
        });
    }

    [Test]
    public void TestCanDeserializeWithSuccessStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "success"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Success));
        });
    }

    [Test]
    public void TestCanDeserializeWithFailureStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "failure"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Failure));
        });
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "status": "pending"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": {},
                        "status": "pending"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingStatusThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidStatusTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializingWithInvalidStatusValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
