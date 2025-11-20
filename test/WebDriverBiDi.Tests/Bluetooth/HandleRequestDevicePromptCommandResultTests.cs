namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class HandleRequestDevicePromptCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        HandleRequestDevicePromptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}