namespace WebDriverBiDi.Speculation;

using TestUtilities;

[TestFixture]
public class SpeculationModuleTests
{
     [Test]
    public async Task TestCanReceivePrefetchStatusUpdatedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        SpeculationModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnPrefetchStatusUpdated.AddObserver((PrefetchStatusUpdatedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com/index.html"));
                Assert.That(e.Status, Is.EqualTo(PreloadingStatus.Pending));
            }
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
        Assert.That(eventRaised, Is.True);
    }
}
