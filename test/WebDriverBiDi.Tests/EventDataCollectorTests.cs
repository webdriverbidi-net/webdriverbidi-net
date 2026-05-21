namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

public class EventDataCollectorTests
{
    [Fact]
    public async Task TestCanCaptureEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.Single(eventArgsList);
        Assert.Equal("myValue", eventArgsList[0].EventValue);
    }

    [Fact]
    public async Task TestCanFilterCapturedEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector((e) => e.EventValue == "myValue1");
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.Single(eventArgsList);
        Assert.Equal("myValue1", eventArgsList[0].EventValue);
    }

    [Fact]
    public async Task TestCanOnlyCaptureNewEventData()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();

        Assert.Equal(2, eventArgsList.Count);
        Assert.Equal("myValue1", eventArgsList[0].EventValue);
        Assert.Equal("myValue2", eventArgsList[1].EventValue);

        await testEventSource.RaiseTestEventAsync("myValue3");
        await testEventSource.RaiseTestEventAsync("myValue4");
        eventArgsList = collector.GetCollectedEventData();

        Assert.Equal(2, eventArgsList.Count);
        Assert.Equal("myValue3", eventArgsList[0].EventValue);
        Assert.Equal("myValue4", eventArgsList[1].EventValue);
    }

    [Fact]
    public async Task TestCanReturnZeroEvents()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        IReadOnlyList<TestObservableEventArgs> eventArgsList = collector.GetCollectedEventData();
        Assert.Empty(eventArgsList);
    }

    [Fact]
    public async Task TestDisposeAsyncRemovesCollector()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> disposingCollector = testEventSource.TestObservableEvent.AddDataCollector();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await testEventSource.RaiseTestEventAsync("myValue1");
        IReadOnlyList<TestObservableEventArgs> eventArgsList = disposingCollector.GetCollectedEventData();
        Assert.Single(eventArgsList);

        await disposingCollector.DisposeAsync();
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal(2, collector.GetCollectedEventData().Count);
    }

    [Fact]
    public async Task TestDisposeAfterDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        collector.Dispose();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        await collector.DisposeAsync();
    }

    [Fact]
    public async Task TestDisposeAsyncAfterDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        collector.Dispose();
        await collector.DisposeAsync();
    }

    [Fact]
    public async Task TestGettingCollectedDataAfterDisposeThrows()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        collector.Dispose();
        Assert.ThrowsAny<ObjectDisposedException>(() => collector.GetCollectedEventData());
    }

    [Fact]
    public async Task TestGettingCollectedDataAfterDisposeAsyncThrows()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        await collector.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => collector.GetCollectedEventData());
    }

    [Fact]
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
        }
        , TestContext.Current.CancellationToken);

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
            },
            TestContext.Current.CancellationToken));
        }

        // Cancel the reader only after all writers have finished — no new
        // items can arrive after this point, so the final drain is exhaustive.
        await Task.WhenAll(writerTasks);
        readerCts.Cancel();
        await readerTask;

        Assert.Equal(totalEvents, drained.Count);
    }

    [Fact]
    public async Task TestToStringReturnsDescription()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector(description: "My first data collector");
        Assert.Equal("My first data collector", collector.ToString());
    }

    [Fact]
    public async Task TestToStringReturnsDefaultDescription()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        Assert.StartsWith("EventDataCollector<TestObservableEventArgs> (id:", collector.ToString());
    }

    [Fact]
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

        Assert.Equal(3, received.Count);
        Assert.Equal("value1", received[0].EventValue);
        Assert.Equal("value2", received[1].EventValue);
        Assert.Equal("value3", received[2].EventValue);
    }

    [Fact]
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

        Assert.Single(received);
        Assert.Equal("myValue1", received[0].EventValue);
    }

    [Fact]
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

        Assert.Equal(2, received.Count);
        Assert.Equal("value1", received[0].EventValue);
        Assert.Equal("value2", received[1].EventValue);
    }

    [Fact]
    public async Task TestEventsCancellationTokenCancelsIteration()
    {
        TestEventSource testEventSource = new();
        await using EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        using CancellationTokenSource cts = new();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (TestObservableEventArgs _ in collector.Events.WithCancellation(cts.Token))
            {
            }
        });
    }

    [Fact]
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

        Assert.Single(fromEvents);
        Assert.Single(fromDrain);
        Assert.Equal("value1", fromEvents[0].EventValue);
        Assert.Equal("value2", fromDrain[0].EventValue);
    }
}
