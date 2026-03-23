namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;

[TestFixture]
public class LogEntryJsonConverterTests
{
    [Test]
    public void TestDeserializeGenericLogEntryWithRequiredProperties()
    {
        string json = """
                      {"type": "javascript", "text": "hello", "level": "info", "source": {"realm": "testRealm"}, "timestamp": 1234567890}
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.That(entry, Is.Not.InstanceOf<ConsoleLogEntry>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry!.Type, Is.EqualTo("javascript"));
            Assert.That(entry.Text, Is.EqualTo("hello"));
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(entry.Source.RealmId, Is.EqualTo("testRealm"));
            Assert.That(entry.EpochTimestamp, Is.EqualTo(1234567890));
        }
    }

    [Test]
    public void TestDeserializeConsoleLogEntryWithMethodAndArgs()
    {
        string json = """
                      {
                        "type": "console",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "method": "log",
                        "args": [{"type": "string", "value": "test"}]
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<ConsoleLogEntry>());
        ConsoleLogEntry consoleEntry = (ConsoleLogEntry)entry!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(consoleEntry.Method, Is.EqualTo("log"));
            Assert.That(consoleEntry.Args, Has.Count.EqualTo(1));
            Assert.That(consoleEntry.Args[0].Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(consoleEntry.Args[0].ConvertTo<StringRemoteValue>().Value, Is.EqualTo("test"));
        }
    }

    [Test]
    public void TestDeserializeWithOptionalStackTrace()
    {
        string json = """
                      {
                        "type": "javascript",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "stackTrace": {"callFrames": []}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.StackTrace, Is.Not.Null);
        Assert.That(entry.StackTrace!.CallFrames, Is.Empty);
    }

    [Test]
    public void TestDeserializeWithMissingTextThrowsJsonException()
    {
        string json = """{"type": "javascript", "level": "info", "source": {"realm": "testRealm"}, "timestamp": 1234567890}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry 'text' property is required"));
    }

    [Test]
    public void TestDeserializeWithMissingTypeThrowsJsonException()
    {
        string json = """{"text": "hello", "level": "info", "source": {"realm": "testRealm"}, "timestamp": 1234567890}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry 'type' property is required"));
    }

    [Test]
    public void TestDeserializeWithMissingLevelThrowsJsonException()
    {
        string json = """{"type": "javascript", "text": "hello", "source": {"realm": "testRealm"}, "timestamp": 1234567890}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry must have a 'level' property"));
    }

    [Test]
    public void TestDeserializeWithMissingSourceThrowsJsonException()
    {
        string json = """{"type": "javascript", "text": "hello", "level": "info", "timestamp": 1234567890}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry must have a 'source' property"));
    }

    [Test]
    public void TestDeserializeWithMissingTimestampThrowsJsonException()
    {
        string json = """{"type": "javascript", "text": "hello", "level": "info", "source": {"realm": "testRealm"}}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry must have a 'timestamp' property"));
    }

    [Test]
    public void TestDeserializeWithNonStringTypeThrowsJsonException()
    {
        string json = """{"type": 123, "text": "hello", "level": "info", "source": {"realm": "testRealm"}, "timestamp": 1234567890}""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry 'type' property must be a string"));
    }

    [Test]
    public void TestDeserializeNonObjectThrowsJsonException()
    {
        string json = """["invalid log entry"]""";
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("LogEntry JSON must be an object"));
    }

    [Test]
    public void TestDeserializeConsoleEntryWithMissingMethodThrowsJsonException()
    {
        string json = """
                      {
                        "type": "console",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "args": [{"type": "string", "value": "test"}]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ConsoleLogEntry 'method' property is required"));
    }

    [Test]
    public void TestDeserializeConsoleEntryWithNonStringMethodThrowsJsonException()
    {
        string json = """
                      {
                        "type": "console",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "method": 123,
                        "args": [{"type": "string", "value": "test"}]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ConsoleLogEntry 'method' property must be a string"));
    }

    [Test]
    public void TestDeserializeConsoleEntryWithMissingArgsThrowsJsonException()
    {
        string json = """
                      {
                        "type": "console",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "method": "log"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ConsoleLogEntry 'args' property is required"));
    }

    [Test]
    public void TestDeserializeConsoleEntryWithNonArrayArgsThrowsJsonException()
    {
        string json = """
                      {
                        "type": "console",
                        "text": "hello",
                        "level": "info",
                        "source": {"realm": "testRealm"},
                        "timestamp": 1234567890,
                        "method": "log",
                        "args": "not an array"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<LogEntry>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ConsoleLogEntry 'args' property value must be an array"));
    }

    [Test]
    public void TestWriteThrowsNotImplementedException()
    {
        string json = """{"type": "javascript", "text": "hello", "level": "info", "source": {"realm": "testRealm"}, "timestamp": 1234567890}""";
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(() => JsonSerializer.Serialize(entry!), Throws.InstanceOf<NotImplementedException>());
    }
}
