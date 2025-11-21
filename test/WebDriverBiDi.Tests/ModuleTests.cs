namespace WebDriverBiDi;

using TestUtilities;
using WebDriverBiDi.Protocol;

[TestFixture]
public class ModuleTests
{
    [Test]
    public async Task TestEventWithInvalidEventArgsThrows()
    {
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
    public void TestCanGetMaxObserverCount()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.That(module.OnEventInvoked.MaxObserverCount, Is.EqualTo(0));
    }

    [Test]
    public void TestSubclassCanGetAccessDriver()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        Assert.That(module.ModuleName, Is.EqualTo("protocol"));
        Assert.That(module.HostingDriver, Is.EqualTo(driver));
    }

    [Test]
    public void TestExceedingMaxObserverCountThrows()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver, 1);

        module.OnEventInvoked.AddObserver((TestEventArgs e) =>
        {
        });

        Assert.That(() => module.OnEventInvoked.AddObserver((e) => { }), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
