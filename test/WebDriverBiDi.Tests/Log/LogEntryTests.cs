namespace WebDriverBiDi.Log;

using System.Text.Json;

public class LogEntryTests
{
    [Fact]
    public void TestDeserializingWithInvalidLevelValueThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "invalid",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingLevelThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidLevelTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": {},
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithNullTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": null,
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": {},
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingSourceThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidSourceTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": "realmId",
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingTextThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidTextThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": {},
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidTimestampTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingWithNonObjectThrows()
    {
        string json = @"[ ""invalid log entry"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }
}
