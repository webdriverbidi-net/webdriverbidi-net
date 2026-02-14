namespace WebDriverBiDi.Log;

using System.Text.Json;

[TestFixture]
public class EntryAddedEventArgsTests
{
    [Test]
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
        EntryAddedEventArgs eventArgs = new(entry!);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(eventArgs.Text, Is.Null);
            Assert.That(eventArgs.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.Type, Is.EqualTo("generic"));
            Assert.That(eventArgs.Method, Is.Null);
            Assert.That(eventArgs.Arguments, Is.Null);
            Assert.That(eventArgs.StackTrace, Is.Null);
        }
    }

    [Test]
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
        Assert.That(entry, Is.Not.Null);
        EntryAddedEventArgs eventArgs = new(entry);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(eventArgs.Text,Is.EqualTo("my log message"));
            Assert.That(eventArgs.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.Type, Is.EqualTo("console"));
            Assert.That(eventArgs.Method, Is.EqualTo("myMethod"));
            Assert.That(eventArgs.Arguments, Is.Empty);
            Assert.That(eventArgs.StackTrace, Is.Not.Null);
        }
    }

    [Test]
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
        Assert.That(entry, Is.Not.Null);
        EntryAddedEventArgs eventArgs = new(entry);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(eventArgs.Text,Is.EqualTo("my log message"));
            Assert.That(eventArgs.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.Type, Is.EqualTo("console"));
            Assert.That(eventArgs.Method, Is.EqualTo("myMethod"));
            Assert.That(eventArgs.Arguments, Has.Count.EqualTo(1));
            Assert.That(eventArgs.Arguments![0].HasValue);
            Assert.That(eventArgs.Arguments![0].Type, Is.EqualTo("string"));
            Assert.That(eventArgs.Arguments![0].ValueAs<string>(), Is.EqualTo("argValue"));
            Assert.That(eventArgs.StackTrace, Is.Not.Null);
        }
    }

    [Test]
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
        EntryAddedEventArgs eventArgs = new(entry!);
        Assert.That(entry, Is.Not.Null);
        EntryAddedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }
}
