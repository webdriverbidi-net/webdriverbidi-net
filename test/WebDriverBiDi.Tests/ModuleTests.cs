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

        module.EventInvoked += (object? obj, TestEventArgs e) =>
        {
        };

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
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.Multiple(() =>
        {
            Assert.That(driverLog, Has.Count.EqualTo(1));
            Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(unknownMessage, Is.Not.Empty);
        });
    }
}
