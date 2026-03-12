namespace WebDriverBiDi;

using TestUtilities;
using WebDriverBiDi.Protocol;

[TestFixture]
public class ModuleTests
{
    [Test]
    public async Task TestEventWithInvalidEventArgsThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
        });

        ManualResetEvent syncEvent = new(false);
        List<string> driverLog = [];
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Error)
            {
                driverLog.Add(e.Message);
            }
        });

        string unknownMessage = string.Empty;
        transport.OnUnknownMessageReceived.AddObserver((e) =>
        {
            unknownMessage = e.Message;
            syncEvent.Set();
        });

        await driver.StartAsync("ws:localhost");
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
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(10000));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(driverLog, Has.Count.EqualTo(1));
            Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(unknownMessage, Is.Not.Empty);
        }
    }

    [Test]
    public async Task TestCanRemoveEventHandler()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
            syncEvent.Set();
        });

        await driver.StartAsync("ws:localhost");
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
        bool eventSet = syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.True);

        handler.Unobserve();
        syncEvent.Reset();
        await connection.RaiseDataReceivedEventAsync(eventJson);
        eventSet = syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.False);
    }

    [Test]
    public async Task TestCanExecuteEventHandlersAsynchronously()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        Task? eventTask = null;
        ManualResetEvent syncEvent = new(false);
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            eventTask = taskCompletionSource.Task;
            taskCompletionSource.SetResult();
            syncEvent.Set();
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost");
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
        bool eventSet = syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.True);
        await eventTask!;
        Assert.That(eventTask!.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public async Task TestAsyncExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(async (TestEventArgs e) =>
        {
            try
            {
                await Task.Delay(50).ConfigureAwait(false);
                throw new WebDriverBiDiException("Async module handler exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost");
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async handler failure before shutdown.");
        Assert.That(async () => await driver.StopAsync(), Throws.InstanceOf<AggregateException>().With.InnerException.InstanceOf<WebDriverBiDiException>().And.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Async module handler exception"));
    }

    [Test]
    public async Task TestAsyncExceptionInModuleEventHandlerCapturedByCheckpointDoesNotCollect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async (TestEventArgs e) =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.SetCheckpoint();

        await driver.StartAsync("ws:localhost");
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

        bool checkpointReached = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(1));
        Assert.That(checkpointReached, Is.True);

        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(async () => await Task.WhenAll(tasks), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Async module handler exception"));
        Assert.That(async () => await driver.StopAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanCollect()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(50).ConfigureAwait(false);
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

        await driver.StartAsync("ws:localhost");
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async aggregate handler failure before shutdown.");

        AggregateException exception = Assert.ThrowsAsync<AggregateException>(async () => await driver.StopAsync())!;
        Assert.That(exception.InnerException, Is.InstanceOf<AggregateException>());

        AggregateException innerAggregateException = (AggregateException)exception.InnerException!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(innerAggregateException.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(innerAggregateException.InnerExceptions[0], Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("First aggregate failure"));
            Assert.That(innerAggregateException.InnerExceptions[1], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Second aggregate failure"));
        }
    }

    [Test]
    public async Task TestAsyncExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver(async (TestEventArgs e) =>
        {
            try
            {
                await Task.Delay(50).ConfigureAwait(false);
                throw new WebDriverBiDiException("Async module handler exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost");
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async handler failure before shutdown.");
        Assert.That(async () => await driver.Session.StatusAsync(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Unhandled exception in user event handler").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Async module handler exception"));
    }

    [Test]
    public async Task TestAsyncExceptionInModuleEventHandlerCapturedByCheckpointDoesNotTerminate()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        TestProtocolModule module = new(driver);

        EventObserver<TestEventArgs> observer = module.OnEventInvoked.AddObserver(async (TestEventArgs e) =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            throw new WebDriverBiDiException("Async module handler exception");
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.SetCheckpoint();

        await driver.StartAsync("ws:localhost");
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

        bool checkpointReached = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(1));
        Assert.That(checkpointReached, Is.True);

        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(async () => await Task.WhenAll(tasks), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Async module handler exception"));
        Assert.That(async () => await driver.StopAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestAsyncAggregateExceptionInModuleEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        TestProtocolModule module = new(driver);
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
            TaskCompletionSource firstTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource secondTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(50).ConfigureAwait(false);
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

        await driver.StartAsync("ws:localhost");
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
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async aggregate handler failure before shutdown.");

        WebDriverBiDiException exception = Assert.ThrowsAsync<WebDriverBiDiException>(async () => await driver.Session.StatusAsync())!;
        Assert.That(exception.InnerException, Is.InstanceOf<AggregateException>());

        AggregateException innerAggregateException = (AggregateException)exception.InnerException!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(innerAggregateException.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(innerAggregateException.InnerExceptions, Has.One.InstanceOf<InvalidOperationException>().With.Message.EqualTo("First aggregate failure"));
            Assert.That(innerAggregateException.InnerExceptions, Has.One.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Second aggregate failure"));
        }
    }

    [Test]
    public void TestCanGetMaxObserverCount()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.That(module.OnEventInvoked.MaxObserverCount, Is.EqualTo(0));
    }

    [Test]
    public void TestSubclassCanGetAccessDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.That(module.ModuleName, Is.EqualTo("protocol"));
        Assert.That(module.HostingDriver, Is.EqualTo(driver));
    }

    [Test]
    public void TestExceedingMaxObserverCountThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver, 1);

        module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
        });

        Assert.That(() => module.OnEventInvoked.AddObserver((e) => { }), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
