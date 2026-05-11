namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

[TestFixture]
public class EventDataCollectorTests
{
    [Test]
    public async Task TestCanCaptureEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.That(eventArgsList, Has.Count.EqualTo(1));
        Assert.That(eventArgsList[0].EventValue, Is.EqualTo("myValue"));
    }

    [Test]
    public async Task TestCanFilterCapturedEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector((e) => e.EventValue == "myValue1");
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.That(eventArgsList, Has.Count.EqualTo(1));
        Assert.That(eventArgsList[0].EventValue, Is.EqualTo("myValue1"));
    }


    [Test]
    public async Task TestCanOnlyCaptureNewEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgsList, Has.Count.EqualTo(2));
            Assert.That(eventArgsList[0].EventValue, Is.EqualTo("myValue1"));
            Assert.That(eventArgsList[1].EventValue, Is.EqualTo("myValue2"));
        }
        await testEventSource.RaiseTestEventAsync("myValue3");
        await testEventSource.RaiseTestEventAsync("myValue4");
        eventArgsList = collector.GetCollectedEventData();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgsList, Has.Count.EqualTo(2));
            Assert.That(eventArgsList[0].EventValue, Is.EqualTo("myValue3"));
            Assert.That(eventArgsList[1].EventValue, Is.EqualTo("myValue4"));
        }
    }

    [Test]
    public async Task TestCanReturnZeroEvents()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.That(eventArgsList, Has.Count.Zero);
    }

    [Test]
    public async Task TestDisposeAsyncRemovesCollector()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> disposingCollector = testEventSource.TestObservableEvent.AddDataCollector();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue1");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = disposingCollector.GetCollectedEventData();
        Assert.That(eventArgsList, Has.Count.EqualTo(1));

        await disposingCollector.DisposeAsync();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));

        Assert.That(async () => await testEventSource.RaiseTestEventAsync("myValue2"), Throws.Nothing);
        Assert.That(collector.GetCollectedEventData(), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task TestDisposeAfterDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        Assert.That(() => collector.Dispose(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        Assert.That(async () => await collector.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeAsyncAfterDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        collector.Dispose();
        Assert.That(async () => await collector.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public void TestGettingCollectedDataAfterDisposeThrows()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        collector.Dispose();
        Assert.That(() => collector.GetCollectedEventData(), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestGettingCollectedDataAfterDisposeAsyncThrows()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        Assert.That(() => collector.GetCollectedEventData(), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestConcurrentEnqueueAndDrainYieldsAllItems()
    {
        const int writerCount = 4;
        const int eventsPerWriter = 50;
        const int totalEvents = writerCount * eventsPerWriter;

        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();

        // Barrier ensures all writers and the reader start simultaneously.
        using Barrier startBarrier = new(writerCount + 1);
        using CancellationTokenSource readerCts = new();
        List<TestObservableEventArgs> drained = [];

        // Single reader drains concurrently with all writers.
        Task readerTask = Task.Run(() =>
        {
            startBarrier.SignalAndWait();
            while (!readerCts.IsCancellationRequested)
            {
                drained.AddRange(collector.GetCollectedEventData());
            }

            // Final drain: picks up anything enqueued between the last loop
            // iteration and the writers completing.
            drained.AddRange(collector.GetCollectedEventData());
        });

        List<Task> writerTasks = [];
        for (int i = 0; i < writerCount; i++)
        {
            int writerId = i;
            writerTasks.Add(Task.Run(async () =>
            {
                startBarrier.SignalAndWait();
                for (int j = 0; j < eventsPerWriter; j++)
                {
                    await testEventSource.RaiseTestEventAsync($"w{writerId}-e{j}");
                }
            }));
        }

        // Cancel the reader only after all writers have finished — no new
        // items can arrive after this point, so the final drain is exhaustive.
        await Task.WhenAll(writerTasks);
        readerCts.Cancel();
        await readerTask;

        Assert.That(drained, Has.Count.EqualTo(totalEvents));
    }

    [Test]
    public async Task TestToStringReturnsDescription()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector(description: "My first data collector");
        Assert.That(collector.ToString(), Is.EqualTo("My first data collector"));
    }

    [Test]
    public async Task TestToStringReturnsDefaultDescription()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(collector.ToString(), Does.StartWith("EventDataCollector<TestObservableEventArgs> (id:"));
    }

    [Test]
    public async Task TestEventsYieldsBufferedItemsInOrder()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("value1");
        await testEventSource.RaiseTestEventAsync("value2");
        await testEventSource.RaiseTestEventAsync("value3");
        List<TestObservableEventArgs> received = [];
        await foreach (TestObservableEventArgs args in collector.Events)
        {
            received.Add(args);
            if (received.Count == 3)
            {
                break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Has.Count.EqualTo(3));
            Assert.That(received[0].EventValue, Is.EqualTo("value1"));
            Assert.That(received[1].EventValue, Is.EqualTo("value2"));
            Assert.That(received[2].EventValue, Is.EqualTo("value3"));
        }
    }

    [Test]
    public async Task TestEventsRespectsFilter()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector((e) => e.EventValue == "myValue1");
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        List<TestObservableEventArgs> received = [];
        await foreach (TestObservableEventArgs args in collector.Events)
        {
            received.Add(args);
            break;
        }

        Assert.That(received, Has.Count.EqualTo(1));
        Assert.That(received[0].EventValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public async Task TestEventsTerminatesAndDrainsBufferWhenCollectorDisposed()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("value1");
        await testEventSource.RaiseTestEventAsync("value2");

        // Dispose before iterating — buffered items remain readable but stream ends after draining
        await collector.DisposeAsync();

        List<TestObservableEventArgs> received = [];
        await foreach (TestObservableEventArgs args in collector.Events)
        {
            received.Add(args);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Has.Count.EqualTo(2));
            Assert.That(received[0].EventValue, Is.EqualTo("value1"));
            Assert.That(received[1].EventValue, Is.EqualTo("value2"));
        }
    }

    [Test]
    public async Task TestEventsCancellationTokenCancelsIteration()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        using CancellationTokenSource cts = new();
        cts.Cancel();
        Assert.That(async () =>
        {
            await foreach (TestObservableEventArgs _ in collector.Events.WithCancellation(cts.Token))
            {
            }
        }, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestEventsAndGetCollectedEventDataShareBuffer()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("value1");
        await testEventSource.RaiseTestEventAsync("value2");

        // Consume the first item via Events
        List<TestObservableEventArgs> fromEvents = [];
        await foreach (TestObservableEventArgs args in collector.Events)
        {
            fromEvents.Add(args);
            break;
        }

        // GetCollectedEventData only sees the remaining item
        IReadOnlyList<TestObservableEventArgs> fromDrain = collector.GetCollectedEventData();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromEvents, Has.Count.EqualTo(1));
            Assert.That(fromDrain, Has.Count.EqualTo(1));
            Assert.That(fromEvents[0].EventValue, Is.EqualTo("value1"));
            Assert.That(fromDrain[0].EventValue, Is.EqualTo("value2"));
        }
    }
}