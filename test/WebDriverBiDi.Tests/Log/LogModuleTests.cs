namespace WebDriverBiDi.Log;

using TestUtilities;

public class LogModuleTests
{
    [Fact]
    public async Task TestCanReceiveEntryAddedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        LogModule module = driver.Log;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnEntryAdded.AddObserver(e =>
        {
            Assert.Equal("javascript", e.Type);
            Assert.Equal("my log message", e.Text);
            Assert.Null(e.Method);
            Assert.Null(e.Arguments);
            Assert.NotNull(e.StackTrace);
            Assert.Empty(e.StackTrace.CallFrames);

            taskCompletionSource.TrySetResult();
        });

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "log.entryAdded",
                             "params": {
                               "type": "javascript",
                               "level": "debug",
                               "source": {
                                 "realm": "myRealmId",
                                 "context": "browsingContextId"
                               },
                               "text": "my log message",
                               "timestamp": {{epochTimestamp}},
                               "stackTrace": {
                                 "callFrames": [] 
                               }
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveEntryAddedEventForConsoleLogType()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        LogModule module = driver.Log;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Assert.Equal("console", e.Type);
            Assert.Equal("my log message", e.Text);
            Assert.NotNull(e.Method);
            Assert.NotNull(e.Arguments);
            Assert.NotNull(e.Source);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);
            Assert.NotNull(e.StackTrace);
            Assert.Empty(e.StackTrace.CallFrames);

            taskCompletionSource.TrySetResult();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "log.entryAdded",
                             "params": {
                               "type": "console",
                               "level": "debug",
                               "source": {
                                 "realm": "myRealmId",
                                 "context": "browsingContextId" 
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
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
