namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class HistoryUpdatedEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

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
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options);
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
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options);
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
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullContextValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": null,
                        "url": "http://example.com",
                        "timestamp": {{milliseconds}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullUrlValueThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                        {
                          "context": "myContextId",
                          "url": null,
                          "timestamp": {{milliseconds}}
                        }
                        """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullTimestampValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options));
    }

    [Theory]
    [InlineData(253402300800000UL)]
    [InlineData(ulong.MaxValue)]
    public void TestTimestampBeyondDateTimeRangeIsClampedToMaxValue(ulong timestamp)
    {
        // The protocol's js-uint permits values far beyond what DateTime can represent; a
        // conforming remote end must not cause the event to fail to deserialize.
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{timestamp}}
                      }
                      """;
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);
        Assert.Equal(timestamp, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.MaxValue, eventArgs.Timestamp);
    }

    [Fact]
    public void TestTimestampAtLastRepresentableMillisecondIsConverted()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": 253402300799999
                      }
                      """;
        HistoryUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<HistoryUpdatedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);
        Assert.Equal(253402300799999UL, eventArgs.EpochTimestamp);
        Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc), eventArgs.Timestamp);
    }
}
