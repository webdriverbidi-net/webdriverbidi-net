namespace WebDriverBidi;

using Newtonsoft.Json;
using TestUtilities;

[TestFixture]
public class ProtocolModuleTests
{
    [Test]
    public void TestContextCreatedEventWithInvalidEventArgsThrows()
    {
        string eventJson = @"{ ""method"": ""protocol.event"", ""params"": { ""context"": ""invalid"" } }";
        TestDriver driver = new();
        TestProtocolModule module = new(driver);
        module.EventInvoked += (object? obj, TestEventArgs e) =>
        {
        };

        Assert.That(() => driver.EmitResponse(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }
}