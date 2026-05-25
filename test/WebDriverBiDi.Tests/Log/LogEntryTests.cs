namespace WebDriverBiDi.Log;

using System.Text.Json;
using WebDriverBiDi.Script;

public class LogEntryTests
{
    [Fact]
    public void TestCanDeserializeWithNullText()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": null,
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<GenericLogEntry>(entry);

        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.Null(entry.Text);
        Assert.Equal((ulong)((ulong)(epochTimestamp)), entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }

    [Fact]
    public void TestCanDeserializeConsoleLogEntry()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": "myMethod",
                        "args": [],
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<ConsoleLogEntry>(entry);
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.NotNull(consoleEntry);

        Assert.Equal(LogLevel.Debug, consoleEntry.Level);
        Assert.Equal("realmId", consoleEntry.Source.RealmId);
        Assert.NotNull(consoleEntry.Text);
        Assert.Equal("my log message", consoleEntry.Text);
        Assert.Equal(epochTimestamp, consoleEntry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), consoleEntry.Timestamp);
        Assert.Equal("myMethod", consoleEntry.Method);
        Assert.Empty(consoleEntry.Args);
        Assert.NotNull(consoleEntry.StackTrace);
        Assert.Empty(consoleEntry.StackTrace.CallFrames);
    }

    [Fact]
    public void TestCanDeserializeConsoleLogEntryWithArgs()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": "myMethod",
                        "args": [
                          {
                            "type": "string",
                            "value": "argValue"
                          }
                        ]
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<ConsoleLogEntry>(entry);
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.NotNull(consoleEntry);

        Assert.Equal(LogLevel.Debug, consoleEntry.Level);
        Assert.Equal("realmId", consoleEntry.Source.RealmId);
        Assert.NotNull(consoleEntry.Text);
        Assert.Equal("my log message", consoleEntry.Text);
        Assert.Equal(epochTimestamp, consoleEntry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), consoleEntry.Timestamp);
        Assert.Equal("myMethod", consoleEntry.Method);
        Assert.Single(consoleEntry.Args);
        Assert.Equal(RemoteValueType.String, consoleEntry.Args[0].Type);
        Assert.Equal("argValue", consoleEntry.Args[0].ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public void TestCanDeserializeWithDebugLogLevel()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<GenericLogEntry>(entry);

        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.NotNull(entry.Text);
        Assert.Equal("my log message", entry.Text);
        Assert.Equal(epochTimestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }

    [Fact]
    public void TestCanDeserializeWithInfoLogLevel()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "info",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<GenericLogEntry>(entry);

        Assert.Equal(LogLevel.Info, entry.Level);
        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.NotNull(entry.Text);
        Assert.Equal("my log message", entry.Text);
        Assert.Equal(epochTimestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }

    [Fact]
    public void TestCanDeserializeWithWarnLogLevel()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "warn",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<GenericLogEntry>(entry);
        Assert.Equal(LogLevel.Warn, entry.Level);

        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.NotNull(entry.Text);
        Assert.Equal("my log message", entry.Text);
        Assert.Equal(epochTimestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }

    [Fact]
    public void TestCanDeserializeWithErrorLogLevel()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "error",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.IsType<GenericLogEntry>(entry);

        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.NotNull(entry.Text);
        Assert.Equal("my log message", entry.Text);
        Assert.Equal(epochTimestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }

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
    public void TestDeserializingConsoleLogEntryWithMissingMethodThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "args": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingConsoleLogEntryWithInvalidMethodThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": {},
                        "args": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingConsoleLogEntryWithMissingArgsThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": "myMethod"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingConsoleLogEntryWithInvalidArgsThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": "myMethod",
                        "args": "invalidArgs"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingConsoleLogEntryWithInvalidArgTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "console",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{epochTimestamp}},
                        "method": "myMethod",
                        "args": [ "invalidArgs" ]
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }

    [Fact]
    public void TestDeserializingLogEntryWithAsNonObjectThrows()
    {
        string json = @"[ ""invalid log entry"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<LogEntry>(json));
    }
}
