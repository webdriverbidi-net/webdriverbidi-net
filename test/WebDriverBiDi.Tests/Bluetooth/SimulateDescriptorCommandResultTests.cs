namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateDescriptorCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateDescriptorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}