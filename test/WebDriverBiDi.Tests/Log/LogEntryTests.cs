namespace WebDriverBiDi.Log;

using System.Text.Json;

public class LogEntryTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullLevelThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": null,
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullSourceThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": null,
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidTimestampTypeThrows()
    {
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullTimestampThrows()
    {
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNonObjectThrows()
    {
        string json = @"[ ""invalid log entry"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json, this.options));
    }
}
