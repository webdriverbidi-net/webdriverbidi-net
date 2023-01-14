namespace WebDriverBidi;

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

        Assert.That(() => driver.EmitResponse(eventJson), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Unable to cast received event data to TestEventArgs"));
        driver.EmitNullEventArgs = true;
        Assert.That(() => driver.EmitResponse(eventJson), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Unable to cast received event data to TestEventArgs"));
    }
}