namespace WebDriverBidi.Log;

using Newtonsoft.Json;

[TestFixture]
public class LogEntryTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.That(entry!.Level, Is.EqualTo(LogLevel.Debug));
        Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
        Assert.That(entry.Text, Is.Not.Null);
        Assert.That(entry.Text, Is.EqualTo("my log message"));
        Assert.That(entry.Timestamp, Is.EqualTo(123));
    }

    [Test]
    public void TestCanDeserializeWithNullText()
    {
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": null, ""timestamp"": 123 }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.That(entry!.Level, Is.EqualTo(LogLevel.Debug));
        Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
        Assert.That(entry.Text, Is.Null);
        Assert.That(entry.Timestamp, Is.EqualTo(123));
    }

    [Test]
    public void TestCanDeserializeConsoleLogEntry()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"", ""args"": [] }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<ConsoleLogEntry>());
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.That(consoleEntry!.Level, Is.EqualTo(LogLevel.Debug));
        Assert.That(consoleEntry.Source.RealmId, Is.EqualTo("realmId"));
        Assert.That(consoleEntry.Text, Is.Not.Null);
        Assert.That(consoleEntry.Text, Is.EqualTo("my log message"));
        Assert.That(consoleEntry.Timestamp, Is.EqualTo(123));
        Assert.That(consoleEntry.Method, Is.EqualTo("myMethod"));
        Assert.That(consoleEntry.Args.Count, Is.EqualTo(0));
   }

    [Test]
    public void TestCanDeserializeConsoleLogEntryWithArgs()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"", ""args"": [{ ""type"": ""string"", ""value"": ""argValue"" }] }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<ConsoleLogEntry>());
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.That(consoleEntry!.Level, Is.EqualTo(LogLevel.Debug));
        Assert.That(consoleEntry.Source.RealmId, Is.EqualTo("realmId"));
        Assert.That(consoleEntry.Text, Is.Not.Null);
        Assert.That(consoleEntry.Text, Is.EqualTo("my log message"));
        Assert.That(consoleEntry.Timestamp, Is.EqualTo(123));
        Assert.That(consoleEntry.Method, Is.EqualTo("myMethod"));
        Assert.That(consoleEntry.Args.Count, Is.EqualTo(1));
        Assert.That(consoleEntry.Args[0].HasValue);
        Assert.That(consoleEntry.Args[0].Type, Is.EqualTo("string"));
        Assert.That(consoleEntry.Args[0].ValueAs<string>(), Is.EqualTo("argValue"));
   }

    [Test]
    public void TestDeserializingWithInvalidLevelValueThrows()
    {
        string json = @"{ ""type"": ""generic"", ""level"": ""invalid"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingLevelThrows()
    {
        string json = @"{ ""type"": ""generic"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidLevelTypeThrows()
    {
        string json = @"{ ""type"": ""generic"", ""level"": {}, ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingTypeThrows()
    {
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidTypeThrows()
    {
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithSourceTypeThrows()
    {
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidSourceThrows()
    {
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": ""realmId"", ""text"": ""my log message"", ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingTextThrows()
    {
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidTextThrows()
    {
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": {}, ""timestamp"": 123 }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithTimestampTypeThrows()
    {
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithMissingMethodThrows()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""args"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidMethodThrows()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": {}, ""args"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithMissingArgsThrows()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidArgsThrows()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"", ""args"": ""invalidArgs"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidArgTypeThrows()
    {
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"", ""args"": [ ""invalidArgs"" ] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }
}