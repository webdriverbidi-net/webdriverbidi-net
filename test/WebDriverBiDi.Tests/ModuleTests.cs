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

        module.OnEventInvoked.AddHandler((TestEventArgs e) =>
        {
            return Task.CompletedTask;
        });

        ManualResetEvent syncEvent = new(false);
        List<string> driverLog = new();
        transport.OnLogMessage.AddHandler((e) =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Error)
            {
                driverLog.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        string unknownMessage = string.Empty;
        transport.OnUnknownMessageReceived.AddHandler((e) =>
        {
            unknownMessage = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });

        await driver.StartAsync("ws:localhost");
        string eventJson = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""context"": ""invalid"" } }";
        await connection.RaiseDataReceivedEventAsync(eventJson);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(driverLog, Has.Count.EqualTo(1));
            Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(unknownMessage, Is.Not.Empty);
        });
    }

    [Test]
    public async Task TestCanRemoveEventHandler()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddHandler((TestEventArgs e) =>
        {
            syncEvent.Set();
            return Task.CompletedTask;
        });

        await driver.StartAsync("ws:localhost");
        string eventJson = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }";
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
        EventObserver<TestEventArgs> handler = module.OnEventInvoked.AddHandler((TestEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            eventTask = taskCompletionSource.Task;
            taskCompletionSource.SetResult();
            syncEvent.Set();
            return taskCompletionSource.Task;
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws:localhost");
        string eventJson = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }";
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
    public void TestExceedingMaxObserverCountThrows()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver, 1);

        module.OnEventInvoked.AddHandler((TestEventArgs e) =>
        {
            return Task.CompletedTask;
        });

        Assert.That(() => module.OnEventInvoked.AddHandler((e) => { return Task.CompletedTask; }), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
