namespace WebDriverBiDi.Log;

using System.Text.Json;
using WebDriverBiDi.Script;

public class EntryAddedEventArgsTests
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
        EntryAddedEventArgs eventArgs = new(entry);

        Assert.Equal("realmId", eventArgs.Source.RealmId);
        Assert.Null(eventArgs.Text);
        Assert.Equal(LogLevel.Debug, eventArgs.Level);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("generic", eventArgs.Type);
        Assert.Null(eventArgs.Method);
        Assert.Null(eventArgs.Arguments);
        Assert.Null(eventArgs.StackTrace);
    }

    [Fact]
    public void TestCanDeserializeConsoleLogEntry()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
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
        EntryAddedEventArgs eventArgs = new(entry);

        Assert.Equal("realmId", eventArgs.Source.RealmId);
        Assert.Equal("my log message", eventArgs.Text);
        Assert.Equal(LogLevel.Debug, eventArgs.Level);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("console", eventArgs.Type);
        Assert.Equal("myMethod", eventArgs.Method);
        Assert.NotNull(eventArgs.Arguments);
        Assert.Empty(eventArgs.Arguments);
        Assert.NotNull(eventArgs.StackTrace);
    }

    [Fact]
    public void TestCanDeserializeConsoleLogEntryWithArgs()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
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
                        ], "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        EntryAddedEventArgs eventArgs = new(entry);

        Assert.Equal("realmId", eventArgs.Source.RealmId);
        Assert.Equal("my log message", eventArgs.Text);
        Assert.Equal(LogLevel.Debug, eventArgs.Level);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("console", eventArgs.Type);
        Assert.Equal("myMethod", eventArgs.Method);
        Assert.NotNull(eventArgs.Arguments);
        Assert.Single(eventArgs.Arguments);
        Assert.Equal(RemoteValueType.String, eventArgs.Arguments[0].Type);
        Assert.Equal("argValue", eventArgs.Arguments[0].ConvertTo<StringRemoteValue>().Value);
        Assert.NotNull(eventArgs.StackTrace);
    }

    [Fact]
    public void TestCopySemantics()
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
        EntryAddedEventArgs eventArgs = new(entry);
        EntryAddedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
