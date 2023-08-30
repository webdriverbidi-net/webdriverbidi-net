namespace WebDriverBidi.Protocol;

[TestFixture]
public class DispatcherTests
{
    private Dispatcher<string>? dispatcher;

    [SetUp]
    public void Setup()
    {
        dispatcher = new();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (dispatcher is not null && dispatcher.IsDispatching)
        {
            await dispatcher.StopDispatching();
        }
        
        dispatcher = null;
   }

    [Test]
    public void TestCanDispatchItem()
    {
        string dispatched = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        dispatcher!.ItemDispatched += (sender, e) =>
        {
            dispatched = e.DispatchedItem;
            syncEvent.Set();
        };
        Assert.That(dispatcher!.TryDispatch("Hello dispatcher"), Is.True);
        bool eventFired = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.That(eventFired, Is.True);
        Assert.That(dispatched, Is.EqualTo("Hello dispatcher"));
    }

    [Test]
    public void TestIsDispatchingUponConstruction()
    {
        Assert.That(dispatcher!.IsDispatching, Is.True);
    }

    [Test]
    public async Task TestCannotDispatchItemsWhenShutdown()
    {
        await dispatcher!.StopDispatching();
        Assert.That(dispatcher!.TryDispatch("error"), Is.False);
        Assert.That(dispatcher.IsDispatching, Is.False);
    }

    [Test]
    public async Task TestCanCallStopDispatchingTwice()
    {
        await dispatcher!.StopDispatching();
        await dispatcher.StopDispatching();
        Assert.That(dispatcher.IsDispatching, Is.False);
    }

    [Test]
    public async Task TestQueuedItemsWillDispatchAfterShutdown()
    {
        List<string> dispatchedItems = new();
        CountdownEvent syncEvent = new(2);
        dispatcher!.ItemDispatched += (sender, e) =>
        {
            Task.Delay(TimeSpan.FromMilliseconds(200)).Wait();
            dispatchedItems.Add(e.DispatchedItem);
            syncEvent.Signal();
        };
        dispatcher.TryDispatch("Item1");
        dispatcher.TryDispatch("Item2");
        await dispatcher.StopDispatching();
        syncEvent.Wait(TimeSpan.FromSeconds(3));
        Assert.That(dispatchedItems, Is.EqualTo(new List<string>() { "Item1", "Item2" }));
    }
}