namespace WebDriverBiDi;

using TestUtilities;
using WebDriverBiDi.Protocol;

[TestFixture]
public class ModuleTests
{
    [Test]
    public async Task TestContextCreatedEventWithInvalidEventArgsThrows()
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
        transport.LogMessage += (sender, e) =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Error)
            {
                driverLog.Add(e.Message);
            }
        };

        string unknownMessage = string.Empty;
        transport.UnknownMessageReceived += (sender, e) =>
        {
            unknownMessage = e.Message;
            syncEvent.Set();
        };

        string eventJson = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""context"": ""invalid"" } }";
        await connection.RaiseDataReceivedEventAsync(eventJson);
        syncEvent.WaitOne(TimeSpan.FromSeconds(10));
        Assert.Multiple(() =>
        {
            Assert.That(driverLog, Has.Count.EqualTo(1));
            Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(unknownMessage, Is.Not.Empty);
        });
    }
}
