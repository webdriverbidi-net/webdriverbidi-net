namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class HistoryUpdatedEventArgsTests
{
    [Fact]
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
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)((ulong)(milliseconds)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{milliseconds}},
                        "userContext": "myUserContextId"
                      }
                      """;
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)((ulong)(milliseconds)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
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
        Assert.NotNull(eventArgs);
        HistoryUpdatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidTimestampValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json));
    }
}
