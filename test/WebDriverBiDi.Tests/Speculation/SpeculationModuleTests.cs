namespace WebDriverBiDi.Speculation;

using TestUtilities;

public class SpeculationModuleTests
{
    [Fact]
    public async Task TestCanReceivePrefetchStatusUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        SpeculationModule module = driver.Speculation;

        ManualResetEvent syncEvent = new(false);
        module.OnPrefetchStatusUpdated.AddObserver((PrefetchStatusUpdatedEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com/index.html", e.Url);
            Assert.Equal(PreloadingStatus.Pending, e.Status);

            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }
}
