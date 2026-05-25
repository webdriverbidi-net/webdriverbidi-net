namespace WebDriverBiDi.Speculation;

using TestUtilities;

public class SpeculationModuleTests
{
    [Fact]
    public async Task TestCanReceivePrefetchStatusUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        SpeculationModule module = driver.Speculation;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnPrefetchStatusUpdated.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com/index.html", e.Url);
            Assert.Equal(PreloadingStatus.Pending, e.Status);
            taskCompletionSource.TrySetResult();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "speculation.prefetchStatusUpdated",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com/index.html",
                               "status": "pending"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
