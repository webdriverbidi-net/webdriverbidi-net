namespace WebDriverBiDi.Log;

using System.Text.Json;

public class GenericLogEntryTests
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

    [Theory]
    [InlineData(253402300800000UL)]
    [InlineData(ulong.MaxValue)]
    public void TestTimestampBeyondDateTimeRangeIsClampedToMaxValue(ulong timestamp)
    {
        // The protocol's js-uint permits values far beyond what DateTime can represent; a
        // conforming remote end must not cause the log entry to fail to deserialize.
        string json = $$"""
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": {{timestamp}}
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.Equal(timestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.MaxValue, entry.Timestamp);
    }

    [Fact]
    public void TestTimestampAtLastRepresentableMillisecondIsConverted()
    {
        string json = """
                      {
                        "type": "generic",
                        "level": "debug",
                        "source": {
                          "realm": "realmId"
                        },
                        "text": "my log message",
                        "timestamp": 253402300799999
                      }
                      """;
        LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(json);
        Assert.NotNull(entry);
        Assert.Equal(253402300799999UL, entry.EpochTimestamp);
        Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc), entry.Timestamp);
    }
}
