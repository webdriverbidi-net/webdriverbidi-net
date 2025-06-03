namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class HistoryUpdatedEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{milliseconds}}
                      }
                      """;
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
        });
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "url": "http://example.com",
                        "timestamp": {{milliseconds}}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": {},
                        "url": "http://example.com",
                        "timestamp": {{milliseconds}}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "timestamp": {{milliseconds}}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

  [Test]
  public void TestDeserializeWithInvalidUrlValueThrows()
  {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                        {
                          "context": "myContextId",
                          "url": {},
                          "timestamp": {{milliseconds}}
                        }
                        """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingTimestampValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTimestampValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

}
