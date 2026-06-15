namespace WebDriverBiDi.Log;

using System.Text.Json;

public class JavaScriptLogEntryTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ulong epochTimestamp = Convert.ToUInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "javascript",
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
        Assert.IsType<JavaScriptLogEntry>(entry);

        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("realmId", entry.Source.RealmId);
        Assert.NotNull(entry.Text);
        Assert.Equal("my log message", entry.Text);
        Assert.Equal(epochTimestamp, entry.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), entry.Timestamp);
    }
}
