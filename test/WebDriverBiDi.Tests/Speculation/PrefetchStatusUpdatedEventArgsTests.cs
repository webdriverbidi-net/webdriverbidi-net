namespace WebDriverBiDi.Speculation;

using System.Text.Json;

[TestFixture]
public class PrefetchStatusUpdatedEventArgsTests
{
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
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Pending));
        }
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
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Ready));
        }
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
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Success));
        }
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
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("https://example.com/index.html"));
            Assert.That(eventArgs.Status, Is.EqualTo(PreloadingStatus.Failure));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        PrefetchStatusUpdatedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<WebDriverBiDiException>());
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
        Assert.That(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
