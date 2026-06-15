namespace WebDriverBiDi.Log;

using System.Text.Json;
using WebDriverBiDi.Script;

public class ConsoleLogEntryTests
{
    [Fact]
    public void TestCanDeserialize()
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
    public void TestCanDeserializeWithArgs()
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
    public void TestDeserializingWithMissingMethodThrows()
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
    public void TestDeserializingWithInvalidMethodThrows()
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
    public void TestDeserializingWithMissingArgsThrows()
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
    public void TestDeserializingWithInvalidArgsThrows()
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
    public void TestDeserializingWithInvalidArgTypeThrows()
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
}
