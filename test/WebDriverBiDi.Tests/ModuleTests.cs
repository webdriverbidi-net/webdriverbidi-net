namespace WebDriverBiDi;

using TestUtilities;
using WebDriverBiDi.Protocol;

[Collection("EventSourceTests")]
public class ModuleTests
{
    [Fact]
    public async Task TestEventWithInvalidEventArgsThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        module.OnEventInvoked.AddObserver(e =>
        {
        });

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<string> driverLog = [];
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Error)
            {
                driverLog.Add(e.Message);
                taskCompletionSource.TrySetResult();
            }
        });

        bool unknownMessageEventRaised = false;
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
        });

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "context": "invalid"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(driverLog);
        Assert.Contains("Unexpected error parsing event JSON", driverLog[0]);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed payload of a registered event was captured once, as a protocol error,
        // and was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestCanRemoveEventHandler()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        // Use a ManualResetEventSlim because we want to reset the event.
        ManualResetEventSlim syncEvent = new(false);
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddObserver(e =>
        {
            syncEvent.Set();
        });

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventSet = syncEvent.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(eventSet);

        handler.Unobserve();
        syncEvent.Reset();

        // A second observer gives the negative assertion a causal signal rather than an elapsed-time
        // one. Observers are notified in the order they were added, and a synchronously-run handler is
        // awaited before the next observer is notified, so once this one has been called the removed
        // handler has demonstrably had its turn and did not run. Waiting a fixed 50 ms instead would
        // pass just as readily when message processing is merely slow, which is the failure the
        // assertion is meant to catch.
        ManualResetEventSlim replacementSyncEvent = new(false);
        using EventObserver<TestEventArgs> replacementHandler = module.OnEventInvoked.AddObserver(e =>
        {
            replacementSyncEvent.Set();
        });

        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool replacementEventSet = replacementSyncEvent.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(replacementEventSet);
        Assert.False(syncEvent.IsSet);
    }

    [Fact]
    public async Task TestCanExecuteEventHandlersAsynchronously()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        TaskCompletionSource handlerStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerFinished = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEventDispatched = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int synchronousInvocationCount = 0;

        // The asynchronous handler blocks until the test releases it.
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddObserver(async e =>
        {
            handlerStarted.TrySetResult();
            await handlerGate.Task;
            handlerFinished.TrySetResult();
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // A synchronous observer records how many events have been dispatched.
        module.OnEventInvoked.AddObserver(e =>
        {
            if (Interlocked.Increment(ref synchronousInvocationCount) == 2)
            {
                secondEventDispatched.TrySetResult();
            }
        });

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // While the asynchronous handler is still blocked, a second event is dispatched
        // and observed: message processing did not wait for the handler to complete.
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await secondEventDispatched.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(handlerFinished.Task.IsCompleted);

        handlerGate.TrySetResult();
        await handlerFinished.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, synchronousInvocationCount);
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(async e =>
        {
            try
            {
                await Task.Yield();
                throw new WebDriverBiDiException("Async module handler exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Collect);
        Assert.True(errorPropagated);
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.Contains("Async module handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCapturedByCheckpointDoesNotCollect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async e =>
        {
            await Task.Yield();
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);

        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);
        Assert.Contains("Async module handler exception", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await Task.WhenAll(tasks))).Message);
        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Task> faultingTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(e =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            // Published through a TaskCompletionSource rather than assigned to a captured local, so
            // that the test reads the task only after the handler has stored it. The test awaits it
            // at the end, which is what turns a failure inside this producer into its own error
            // rather than an unobserved exception.
            Task faultingTask = Task.Run(
                async () =>
                {
                    try
                    {
                        await Task.Yield();
                        firstTaskCompletionSource.SetException(new InvalidOperationException("First aggregate failure"));
                        secondTaskCompletionSource.SetException(new WebDriverBiDiException("Second aggregate failure"));
                    }
                    finally
                    {
                        handlerCompleted.TrySetResult(true);
                    }
                });
            faultingTaskSource.TrySetResult(faultingTask);

            return Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task);
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Collect);
        Assert.True(errorPropagated);

        AggregateException exception = await Assert.ThrowsAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.IsType<AggregateException>(exception.InnerException);

        AggregateException innerAggregateException = (AggregateException)exception.InnerException;

        Assert.Equal(2, innerAggregateException.InnerExceptions.Count);
        Assert.Single(innerAggregateException.InnerExceptions.OfType<InvalidOperationException>(), e => e.Message == "First aggregate failure");
        Assert.Single(innerAggregateException.InnerExceptions.OfType<WebDriverBiDiException>(), e => e.Message == "Second aggregate failure");

        // Awaited so that a failure inside the handler's producer is reported as itself.
        Task producerTask = await faultingTaskSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await producerTask;
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(async e =>
        {
            try
            {
                await Task.Yield();
                throw new WebDriverBiDiException("Async module handler exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Terminate);
        Assert.True(errorPropagated);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.Session.StatusAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Unhandled exception in user event handler", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Async module handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCapturedByCheckpointDoesNotTerminate()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async e =>
        {
            await Task.Yield();
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);

        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);
        Assert.Contains("Async module handler exception", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await Task.WhenAll(tasks))).Message);
        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Task> faultingTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(e =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            // Published through a TaskCompletionSource rather than assigned to a captured local, so
            // that the test reads the task only after the handler has stored it. The test awaits it
            // at the end, which is what turns a failure inside this producer into its own error
            // rather than an unobserved exception.
            Task faultingTask = Task.Run(
                async () =>
                {
                    try
                    {
                        await Task.Yield();
                        firstTaskCompletionSource.SetException(new InvalidOperationException("First aggregate failure"));
                        secondTaskCompletionSource.SetException(new WebDriverBiDiException("Second aggregate failure"));
                    }
                    finally
                    {
                        handlerCompleted.TrySetResult(true);
                    }
                });
            faultingTaskSource.TrySetResult(faultingTask);

            return Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task);
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        string eventJson = """
                           {
                             "type": "event",
                             "method": "protocol.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Terminate);
        Assert.True(errorPropagated);

        WebDriverBiDiException exception = await Assert.ThrowsAsync<WebDriverBiDiException>(async () => await driver.Session.StatusAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.IsType<AggregateException>(exception.InnerException);

        AggregateException innerAggregateException = (AggregateException)exception.InnerException;

        Assert.Equal(2, innerAggregateException.InnerExceptions.Count);
        Assert.Single(innerAggregateException.InnerExceptions.OfType<InvalidOperationException>(), e => e.Message == "First aggregate failure");
        Assert.Single(innerAggregateException.InnerExceptions.OfType<WebDriverBiDiException>(), e => e.Message == "Second aggregate failure");

        // Awaited so that a failure inside the handler's producer is reported as itself.
        Task producerTask = await faultingTaskSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await producerTask;
    }

    [Fact]
    public async Task TestCanGetMaxObserverCount()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.Equal(0u, module.OnEventInvoked.MaxObserverCount);
    }

    [Fact]
    public async Task TestSubclassCanGetAccessDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.Equal("protocol", module.ModuleName);
        Assert.Equal(driver, module.HostingDriver);
    }

    [Fact]
    public async Task TestExceedingMaxObserverCountThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver, 1);

        module.OnEventInvoked.AddObserver(e =>
        {
        });

        Assert.ThrowsAny<WebDriverBiDiException>(() => module.OnEventInvoked.AddObserver(e => { }));
    }

    [Fact]
    public async Task TestModuleWithNonReporterDriverDoesNotSetErrorReporter()
    {
        NonReporterDriver driver = new();
        TestProtocolModule module = new(driver);

        // Verify the module was constructed without throwing; observer error reporting
        // is silently skipped when the driver does not implement IEventObserverErrorReporter.
        Assert.Equal(0, module.OnEventInvoked.CurrentObserverCount);
    }

    /// <summary>
    /// The counterpart of the non-reporter case: an executor that implements
    /// <see cref="IEventObserverErrorReporter"/> receives the failure of an asynchronously-run observer
    /// of an event its module raises. The interface is public precisely so a custom executor can do
    /// this; without it such a failure is observed and discarded, and nothing in the consumer's code
    /// ever learns of it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestModuleWithReporterDriverRoutesAsynchronousObserverFaultToIt()
    {
        ReporterDriver driver = new();
        TestProtocolModule module = new(driver);

        module.OnEventInvoked.AddObserver(
            _ => Task.FromException(new InvalidOperationException("handler blew up")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.RaiseRegisteredEventAsync("protocol.event", new TestEventArgs());

        EventObserverErrorInfo errorInfo = await driver.ReportedFault.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("protocol.event", errorInfo.ObservableEventName);
        Assert.True(errorInfo.IsAsynchronousHandler);
        Assert.True(errorInfo.FaultOccurredAfterHandlerReturned);
        Assert.Equal("handler blew up", errorInfo.Exception.Message);
    }

    /// <summary>
    /// A consumer-defined command executor that reports late observer failures, standing in for the
    /// custom driver an advanced consumer writes. It keeps the invoker each module registers so that a
    /// test can deliver an event without a transport.
    /// </summary>
    private sealed class ReporterDriver : IBiDiCommandExecutor, IEventObserverErrorReporter
    {
        private readonly Dictionary<string, Func<object?, Task>> eventInvokers = [];

        public TaskCompletionSource<EventObserverErrorInfo> ReportedFault { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TimeSpan DefaultCommandTimeout => TimeSpan.FromSeconds(30);

        public bool IsStarted => true;

        public Func<EventObserverErrorInfo, Task> EventObserverErrorReporter => this.RecordFaultAsync;

        public Task StartAsync(string connectionString, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
            where T : CommandResult => throw new NotImplementedException();

        public Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
            where T : CommandResult => throw new NotImplementedException();

        public void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
        {
            this.eventInvokers[eventName] = eventData => eventInvoker(new EventInfo<T>((T)eventData!, ReceivedDataDictionary.EmptyDictionary));
        }

        public Task RaiseRegisteredEventAsync(string eventName, object eventData) => this.eventInvokers[eventName](eventData);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private Task RecordFaultAsync(EventObserverErrorInfo errorInfo)
        {
            this.ReportedFault.TrySetResult(errorInfo);
            return Task.CompletedTask;
        }
    }

    private sealed class NonReporterDriver : IBiDiCommandExecutor
    {
        private readonly List<string> registeredEvents = [];

        public TimeSpan DefaultCommandTimeout => TimeSpan.FromSeconds(30);

        public bool IsStarted => false;

        public Task StartAsync(string connectionString, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
            where T : CommandResult => throw new NotImplementedException();

        public Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
            where T : CommandResult => throw new NotImplementedException();

        public void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
        {
            this.registeredEvents.Add(eventName);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public async Task TestEventExposesPayloadAndEnvelopeExtensionDataSeparately()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        TaskCompletionSource<TestEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnEventInvoked.AddObserver(e => received.TrySetResult(e));

        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync("""{"type":"event","method":"protocol.event","goog:channel":"channel value","params":{"paramName":"paramValue","goog:extra":"payload value"}}""");
        TestEventArgs eventArgs = await received.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal("paramValue", eventArgs.ParamName);
        Assert.Single(eventArgs.AdditionalData);
        Assert.Equal("payload value", eventArgs.AdditionalData["goog:extra"]);
        Assert.Single(eventArgs.AdditionalEventProperties);
        Assert.Equal("channel value", eventArgs.AdditionalEventProperties["goog:channel"]);
    }
}
