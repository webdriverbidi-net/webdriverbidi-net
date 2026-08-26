namespace WebDriverBiDi.Log;

using WebDriverBiDi.Script;
using WebDriverBiDi.TestUtilities;

public class EntryAddedEventArgsTests
{
    [Fact]
    public async Task TestCanDeserializeWithNullText()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "event",
                        "method": "log.entryAdded",
                        "params": {
                          "type": "generic",
                          "level": "debug",
                          "source": {
                            "realm": "realmId"
                          },
                          "text": null,
                          "timestamp": {{epochTimestamp}}
                        }
                      }
                      """;
        EntryAddedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

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
    public async Task TestCanDeserializeConsoleLogEntry()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "event",
                        "method": "log.entryAdded",
                        "params": {
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
                      }
                      """;
        EntryAddedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

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
    public async Task TestCanDeserializeConsoleLogEntryWithArgs()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "event",
                        "method": "log.entryAdded",
                        "params": {
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
                      }
                      """;
        EntryAddedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

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
    public async Task TestCopySemantics()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "type": "event",
                        "method": "log.entryAdded",
                        "params": {
                          "type": "generic",
                          "level": "debug",
                          "source": {
                            "realm": "realmId"
                          },
                          "text": null,
                          "timestamp": {{epochTimestamp}}
                        }
                      }
                      """;
        EntryAddedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        EntryAddedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    private async Task<EntryAddedEventArgs?> GenerateEventArgs(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        EntryAddedEventArgs? eventArgs = null;
        using EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => eventArgs = e);

        observer.StartCapturingTasks();
        await connection.RaiseDataReceivedEventAsync(json);
        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);
        return eventArgs;
    }
}
