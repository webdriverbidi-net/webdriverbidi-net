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
        Transport transport = new(connection);
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
            }
        });

        string unknownMessage = string.Empty;
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessage = e.Message;
            taskCompletionSource.TrySetResult();
        });

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Assert.NotEmpty(unknownMessage);
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

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        bool eventSet = syncEvent.Wait(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        Assert.True(eventSet);

        handler.Unobserve();
        syncEvent.Reset();
        await connection.RaiseDataReceivedEventAsync(eventJson);
        eventSet = syncEvent.Wait(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        Assert.False(eventSet);
    }

    [Fact]
    public async Task TestCanExecuteEventHandlersAsynchronously()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
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

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
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
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async e =>
        {
            await Task.Yield();
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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

        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);
        Assert.Contains("Async module handler exception", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await Task.WhenAll(tasks))).Message);
        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(e =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = Task.Run(
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

            return Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task);
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
        Assert.True(errorPropagated);

        AggregateException exception = await Assert.ThrowsAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.IsType<AggregateException>(exception.InnerException);

        AggregateException innerAggregateException = (AggregateException)exception.InnerException;

        Assert.Equal(2, innerAggregateException.InnerExceptions.Count);
        Assert.Single(innerAggregateException.InnerExceptions.OfType<InvalidOperationException>(), e => e.Message == "First aggregate failure");
        Assert.Single(innerAggregateException.InnerExceptions.OfType<WebDriverBiDiException>(), e => e.Message == "Second aggregate failure");
    }

    [Fact]
    public async Task TestAsyncExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
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

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
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
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async e =>
        {
            await Task.Yield();
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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

        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);
        Assert.Contains("Async module handler exception", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await Task.WhenAll(tasks))).Message);
        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(e =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = Task.Run(
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

            return Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task);
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
        Assert.True(errorPropagated);

        WebDriverBiDiException exception = await Assert.ThrowsAsync<WebDriverBiDiException>(async () => await driver.Session.StatusAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.IsType<AggregateException>(exception.InnerException);

        AggregateException innerAggregateException = (AggregateException)exception.InnerException;

        Assert.Equal(2, innerAggregateException.InnerExceptions.Count);
        Assert.Single(innerAggregateException.InnerExceptions.OfType<InvalidOperationException>(), e => e.Message == "First aggregate failure");
        Assert.Single(innerAggregateException.InnerExceptions.OfType<WebDriverBiDiException>(), e => e.Message == "Second aggregate failure");
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
}
