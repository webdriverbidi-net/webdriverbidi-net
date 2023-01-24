namespace WebDriverBidi;

using Newtonsoft.Json;
using TestUtilities;

[TestFixture]
public class ProtocolModuleTests
{
    [Test]
    public void TestContextCreatedEventWithInvalidEventArgsThrows()
    {
        TestConnection connection = new();
        Driver driver = new(new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection));
        TestProtocolModule module = new(driver);

        module.EventInvoked += (object? obj, TestEventArgs e) =>
        {
        };

        string eventJson = @"{ ""method"": ""protocol.event"", ""params"": { ""context"": ""invalid"" } }";
        Assert.That(() => connection.RaiseDataReceivedEvent(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }
}