namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class HandleRequestDevicePromptCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        HandleRequestDevicePromptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}