namespace WebDriverBiDi.Log;

using Newtonsoft.Json;

[TestFixture]
public class LogEntryTests
{
    [Test]
    public void TestCanDeserializeWithNullText()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": null, ""timestamp"": " + epochTimestamp + @" }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.Multiple(() =>
        {
            Assert.That(entry!.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(entry.Text, Is.Null);
            Assert.That(entry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(entry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        });
    }

    [Test]
    public void TestCanDeserializeConsoleLogEntry()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"", ""args"": [], ""stackTrace"": { ""callFrames"": [] } }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<ConsoleLogEntry>());
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.Multiple(() =>
        {
            Assert.That(consoleEntry!.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(consoleEntry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(consoleEntry.Text, Is.Not.Null);
            Assert.That(consoleEntry.Text, Is.EqualTo("my log message"));
            Assert.That(consoleEntry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(consoleEntry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(consoleEntry.Method, Is.EqualTo("myMethod"));
            Assert.That(consoleEntry.Args, Is.Empty);
            Assert.That(consoleEntry.StackTrace, Is.Not.Null);
            Assert.That(consoleEntry.StackTrace!.CallFrames, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeConsoleLogEntryWithArgs()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"", ""args"": [{ ""type"": ""string"", ""value"": ""argValue"" }] }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<ConsoleLogEntry>());
        ConsoleLogEntry? consoleEntry = entry as ConsoleLogEntry;
        Assert.Multiple(() =>
        {
            Assert.That(consoleEntry!.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(consoleEntry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(consoleEntry.Text, Is.Not.Null);
            Assert.That(consoleEntry.Text, Is.EqualTo("my log message"));
            Assert.That(consoleEntry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(consoleEntry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(consoleEntry.Method, Is.EqualTo("myMethod"));
            Assert.That(consoleEntry.Args, Has.Count.EqualTo(1));
            Assert.That(consoleEntry.Args[0].HasValue);
            Assert.That(consoleEntry.Args[0].Type, Is.EqualTo("string"));
            Assert.That(consoleEntry.Args[0].ValueAs<string>(), Is.EqualTo("argValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithDebugLogLevel()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.Multiple(() =>
        {
            Assert.That(entry!.Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(entry.Text, Is.Not.Null);
            Assert.That(entry.Text, Is.EqualTo("my log message"));
            Assert.That(entry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(entry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        });
    }

    [Test]
    public void TestCanDeserializeWithInfoLogLevel()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""info"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.Multiple(() =>
        {
            Assert.That(entry!.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(entry.Text, Is.Not.Null);
            Assert.That(entry.Text, Is.EqualTo("my log message"));
            Assert.That(entry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(entry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        });
   }

    [Test]
    public void TestCanDeserializeWithWarnLogLevel()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""warn"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.That(entry!.Level, Is.EqualTo(LogLevel.Warn));
        Assert.Multiple(() =>
        {
            Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(entry.Text, Is.Not.Null);
            Assert.That(entry.Text, Is.EqualTo("my log message"));
            Assert.That(entry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(entry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        });
    }

    [Test]
    public void TestCanDeserializeWithErrorLogLevel()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""error"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        LogEntry? entry = JsonConvert.DeserializeObject<LogEntry>(json);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry, Is.InstanceOf<LogEntry>());
        Assert.Multiple(() =>
        {
            Assert.That(entry!.Level, Is.EqualTo(LogLevel.Error));
            Assert.That(entry.Source.RealmId, Is.EqualTo("realmId"));
            Assert.That(entry.Text, Is.Not.Null);
            Assert.That(entry.Text, Is.EqualTo("my log message"));
            Assert.That(entry.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(entry.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        });
    }

    [Test]
    public void TestDeserializingWithInvalidLevelValueThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""invalid"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializingWithMissingLevelThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
         string json = @"{ ""type"": ""generic"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidLevelTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": {}, ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializingWithMissingTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithNullTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": null, ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithSourceTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidSourceThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": ""realmId"", ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingTextThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidTextThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": {}, ""timestamp"": " + epochTimestamp + @" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithTimestampTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": {}, ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithMissingMethodThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""args"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidMethodThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": {}, ""args"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithMissingArgsThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidArgsThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"", ""args"": ""invalidArgs"" }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingConsoleLogEntryWithInvalidArgTypeThrows()
    {
        DateTime timestamp = DateTime.Now;
        long epochTimestamp = Convert.ToInt64((timestamp - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"", ""args"": [ ""invalidArgs"" ] }";
        Assert.That(() => JsonConvert.DeserializeObject<LogEntry>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCannotSerialize()
    {
        // NOTE: LogEntry does not provide a way to instantiate one directly
        // using a constructor, so we will deserialize one from JSON.
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""type"": ""generic"", ""level"": ""debug"", ""source"": { ""realm"": ""realmId"" }, ""text"": null, ""timestamp"": " + epochTimestamp + @" }";
        LogEntry entry = JsonConvert.DeserializeObject<LogEntry>(json)!;
        Assert.That(() => JsonConvert.SerializeObject(entry), Throws.InstanceOf<NotImplementedException>());
    }
}